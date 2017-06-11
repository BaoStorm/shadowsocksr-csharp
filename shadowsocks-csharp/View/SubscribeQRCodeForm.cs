using Shadowsocks.Controller;
using Shadowsocks.Model;
using Shadowsocks.Properties;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System;

namespace Shadowsocks.View
{
    public partial class SubscribeQRCodeForm : Form
    {
        private ShadowsocksController controller;
        // this is a copy of configuration that we are working on
        private Configuration _modifiedConfiguration;
        public SubscribeQRCodeForm(ShadowsocksController controller)
        {
            this.Font = System.Drawing.SystemFonts.MessageBoxFont;
            InitializeComponent();

            this.Icon = Icon.FromHandle(Resources.ssw128.GetHicon());
            this.controller = controller;

            UpdateTexts();
            controller.ConfigChanged += controller_ConfigChanged;

            LoadCurrentConfiguration();
        }

        private void UpdateTexts()
        {
            this.Text= I18N.GetString("Subscribe QRCode setting...");
            labelUrlList.Text = I18N.GetString("Url List");
            labelInput.Text = I18N.GetString("Input");
            labelGroupName.Text = I18N.GetString("Group Name");
            checkBoxAutoUpdate.Text = I18N.GetString("Auto Update");
            buttonAdd.Text = I18N.GetString("&Add");
            buttonDelete.Text = I18N.GetString("&Delete");
            buttonCancel.Text = I18N.GetString("Cancel");
            buttonOK.Text = I18N.GetString("OK");
        }

        private void SubscribeQRCodeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            controller.ConfigChanged -= controller_ConfigChanged;
        }

        private void buttonAdd_Click(object sender, System.EventArgs e)
        {
            Regex re = new Regex(@"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?");
            if (!re.IsMatch(textBoxUrl.Text))
            {
                MessageBox.Show("请输入正确的URL");
            }
            else if(listBoxUrls.Items.IndexOf(textBoxUrl.Text)==-1){
                listBoxUrls.Items.Add(textBoxUrl.Text);
            }
        }

        private void buttonDelete_Click(object sender, System.EventArgs e)
        {
            if (listBoxUrls.SelectedItem != null)
            {
                MessageBox.Show("请输入选择列表项");
            }
            else if(listBoxUrls.Items.Count>0){
                listBoxUrls.Items.Remove(listBoxUrls.SelectedItem);
            }
        }

        private void controller_ConfigChanged(object sender, EventArgs e)
        {
            LoadCurrentConfiguration();
        }

        private void LoadCurrentConfiguration()
        {
            _modifiedConfiguration = controller.GetConfiguration();
            LoadAllSettings();
        }

        private void LoadAllSettings()
        {
            listBoxUrls.Items.Clear();
            listBoxUrls.Items.AddRange(_modifiedConfiguration.nodeFeedQRCodeURLs.ToArray());
            textBoxGroup.Text = _modifiedConfiguration.nodeFeedQRCodeGroup;
            checkBoxAutoUpdate.Checked = _modifiedConfiguration.nodeFeedQRCodeAutoUpdate;
        }

        private int SaveAllSettings()
        {
            _modifiedConfiguration.nodeFeedQRCodeURLs.Clear();
            foreach (var url in listBoxUrls.Items)
            {
                _modifiedConfiguration.nodeFeedQRCodeURLs.Add(url.ToString());
            }
            _modifiedConfiguration.nodeFeedQRCodeGroup = textBoxGroup.Text;
            _modifiedConfiguration.nodeFeedQRCodeAutoUpdate = checkBoxAutoUpdate.Checked;
            return 0;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (SaveAllSettings() == -1)
            {
                return;
            }
            controller.SaveServersConfig(_modifiedConfiguration);
            this.Close();
        }
    }
}
