using Shadowsocks.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace Shadowsocks.Controller
{

    public class UpdateQRCodeNode
    {

        public delegate void UpdateQRCodeNodeHandler(byte[] sender, EventArgs e);
        public event EventHandler NewQRCodeNodeFound;
        public bool QRCodeNodeResult;
        public void CheckUpdate(ShadowsocksController controller, bool use_proxy)
        {
            try
            {
                QRCodeNodeResult = false;
                Configuration config = controller.GetConfiguration();
                foreach (var item in config.nodeFeedQRCodeURLs)
                {
                    bool success = false;
                    byte[] qrCodeData = null;
                    qrCodeData = DownloadData(config, item.url, use_proxy);
                    //if (qrCodeData == null)
                    //{
                    //    qrCodeData = DownloadData(config, item.url, !use_proxy);
                    //}
                    if (qrCodeData != null)
                    {
                        //读入MemoryStream对象  
                        MemoryStream memoryStream = new MemoryStream(qrCodeData, 0, qrCodeData.Length);
                        memoryStream.Write(qrCodeData, 0, qrCodeData.Length);
                        //转成图片  
                        Image image = Image.FromStream(memoryStream);
                        Bitmap target = new Bitmap(image);
                        var source = new BitmapLuminanceSource(target);
                        var bitmap = new BinaryBitmap(new HybridBinarizer(source));
                        QRCodeReader reader = new QRCodeReader();
                        var result = reader.decode(bitmap);
                        if (result != null)
                        {
                            success = AddServerBySSURL(ref config, result.Text, config.nodeFeedQRCodeGroup, true); 
                        }
                    }
                    if (success)
                    {
                        item.state = QRCodeUrlState.Normal;
                    }
                    else
                    {
                        item.state = QRCodeUrlState.Exception;
                    }
                }
                controller.SaveServersConfig(config);
                QRCodeNodeResult = true;
                if (NewQRCodeNodeFound != null)
                {
                    NewQRCodeNodeFound(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                Logging.Debug(ex.ToString());
                if (NewQRCodeNodeFound != null)
                {
                    NewQRCodeNodeFound(this, new EventArgs());
                }
                return;
            }
        }

        private byte[] DownloadData(Configuration config, string url, bool use_proxy)
        {
            try
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
                return http.DownloadData(new Uri(url));
            }
            catch (WebException ex)
            {
                Logging.Debug(string.Format("Url:{0} {1}", url, ex.Message));
                return null;
            }
            catch (SocketException ex)
            {
                Logging.Debug(string.Format("Url:{0} {1}", url, ex.Message));
                return null;
            }
            catch (Exception ex)
            {
                Logging.Debug(ex.ToString());
                return null;
            }
        }

        private bool AddServerBySSURL(ref Configuration config,string ssURL, string force_group = null, bool isEdit = false)
        {
            if (ssURL.StartsWith("ss://", StringComparison.OrdinalIgnoreCase) || ssURL.StartsWith("ssr://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var server = new Server(ssURL, force_group);
                    if (isEdit)
                    {
                        int index = config.configs.FindIndex(p => p.server == server.server && (p.server_port == server.server_port || p.server_udp_port == server.server_udp_port));
                        if (index >= 0)
                        {
                            server.id = config.configs[index].id;
                            config.configs.RemoveAt(index);
                        }
                    }
                    config.configs.Add(server);
                    //SaveConfig(_config);
                    return true;
                }
                catch (Exception e)
                {
                    Logging.LogUsefulException(e);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
