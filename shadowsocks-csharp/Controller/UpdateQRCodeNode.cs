using Shadowsocks.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Shadowsocks.Controller
{

    public class UpdateQRCodeNode
    {
        public delegate void UpdateQRCodeNodeHandler(byte[] sender, EventArgs e);
        public event UpdateQRCodeNodeHandler NewQRCodeNodeFound;
        public byte[] QRCodeNodeResult;
        public void CheckUpdate(Configuration config, bool use_proxy)
        {
            QRCodeNodeResult = null;
            try
            {
                foreach (var url in config.nodeFeedQRCodeURLs)
                {
                    WebClient http = new WebClient();
                    http.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.3319.102 Safari/537.36");
                    http.Encoding = Encoding.UTF8;
                    //http.QueryString["rnd"] = Util.Utils.RandUInt32().ToString();
                    if (use_proxy)
                    {
                        WebProxy proxy = new WebProxy(IPAddress.Loopback.ToString(), config.localPort);
                        if (!string.IsNullOrEmpty(config.authPass))
                        {
                            proxy.Credentials = new NetworkCredential(config.authUser, config.authPass);
                        }
                        http.Proxy = proxy;
                    }
                    else
                    {
                        http.Proxy = null;
                    }
                    //UseProxy = !UseProxy;
                    http.DownloadDataCompleted += Http_DownloadDataCompleted;
                    http.DownloadDataAsync(new Uri(url));
                }           
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
            }
        }

        private void Http_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                byte[] response = e.Result;
                QRCodeNodeResult = response;

                if (NewQRCodeNodeFound != null)
                {
                    NewQRCodeNodeFound(QRCodeNodeResult, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                if (e.Error != null)
                {
                    Logging.Debug(e.Error.ToString());
                }
                Logging.Debug(ex.ToString());
                if (NewQRCodeNodeFound != null)
                {
                    NewQRCodeNodeFound(QRCodeNodeResult, new EventArgs());
                }
                return;
            }
        }

       
    }
}
