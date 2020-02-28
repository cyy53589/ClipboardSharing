using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using NLog;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WK.Libraries.SharpClipboardNS;
/* todo:
 * 1. send 时候valideCode验证
 * 2. connect 时候validCode验证
 * 3. 交流时候加密方式
 */

namespace ClipboardSharing
{
    class Configuration
    {
        public static string configurationFileLoation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "clipboardSharing.config");
        public static Configuration config = new Configuration();
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static void loadFromFile()
        {
            if (!File.Exists(Configuration.configurationFileLoation))
            {
                config.saveToFile();
            }
            else
            {

                logger.Info("Load configuration from {0}", Configuration.configurationFileLoation);
                try
                {
                    var stream = new StreamReader(configurationFileLoation);
                    Configuration.config = JsonConvert.DeserializeObject<Configuration>(stream.ReadToEnd());
                    stream.Close();
                }
                catch (Exception e)
                {
                    logger.Error("ERROR. Configuration file can't be parsed.\n {0}", e.Message);
                }
            }
            NetWorkUtil.serverEndPoint = new IPEndPoint(IPAddress.Parse(Configuration.config.serverIp), Configuration.config.serverPort);
        }
        public void saveToFile()
        {
            try
            {
                var stream = new StreamWriter(configurationFileLoation, false);
                stream.Write(JsonConvert.SerializeObject(config));
                stream.Close();
            }catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        //protected Configuration() { }
        public string serverIp = "127.0.0.1";
        public int serverPort = 5358;
        public string username = "default";
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static lib.Client webClient;
        static Logger logger = LogManager.GetCurrentClassLogger();
        static SharpClipboard clipboard;
        static ClipboardManager clipboardManager = null;

        public MainWindow()
        {
            Configuration.loadFromFile();
            if(NetWorkUtil.serverEndPoint == null)
            {
                Application.Current.Shutdown ();
            }

            InitializeComponent();
            InitTray();


            // init clipboard
            clipboardManager = ClipboardManager.GetManager(new Action<string>((target)=> {
                if (webClient == null) return;
                webClient.Send(new lib.Message()
                {
                    Act = lib.Message.Action.SendCopy,
                    Username = Configuration.config.username,
                    ClipboardContent = target
                });
            }),this);
           

            new Thread(() =>
            {
                TcpListener listener = new TcpListener(IPAddress.Any, 0);
                listener.Start();

                //init qrcode - format = validacode,port,local ips
                string connectCode = Guid.NewGuid().ToString("N").Substring(0, 8);
                int listenPort = ((IPEndPoint)listener.LocalEndpoint).Port;
                qrcode.Dispatcher.Invoke(() =>
                {
                    qrcode.Source = QRCUtil.getQRC(String.Format("{0},{1},{2}", connectCode, listenPort,NetWorkUtil.getLocalIPv4WithGateway().First.Value.ToString()));
                });

                while (true)
                {
                    Socket handler = listener.AcceptSocket();
                    var msg = NetWorkUtil.receivce(handler);
                    if (msg == null) handler.Close();
                    if (msg.Act == lib.Message.Action.BindDevice && msg.Username != null && msg.Username.Length > 0 && msg.ClipboardContent == connectCode)
                    {
                        MessageBox.Show(String.Format("扫描成功. 用户名:{0}\nScane Done. Usrname{0}", msg.Username));
                        Configuration.config.username = msg.Username;
                        Configuration.config.saveToFile();
                        RestartClient(Configuration.config);
                    }
                    handler.Close();
                }
            })
            { IsBackground = true }.Start();
            if(Configuration.config.username != null)
                RestartClient(Configuration.config);
        }

        private NotifyIcon notifyIcon;
        private void InitTray()
        {
            tb.Icon = new System.Drawing.Icon("share.ico");
            tb.TrayMouseDoubleClick += Tb_TrayMouseDoubleClick;
        }

        private void Tb_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
        }

        void RestartClient(Configuration config)
        {
            if (webClient != null)
                webClient.Close();
            webClient = new lib.Client(config.username, (lib.Message msg) => reponseToMessage(msg), config.serverIp, config.serverPort);
            webClient.Open();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
        protected override void OnClosed(EventArgs e)
        {
            Configuration.config.saveToFile();
            webClient.Close();
            base.OnClosed(e);
        }

        public static void reponseToMessage(lib.Message msg)
        {
            if (msg.Username != Configuration.config.username)
            {
                logger.Info("Message Received. BUt Username don't match. Data:{0}'",msg.ToJson());
                return;
            }
            switch (msg.Act)
            {
                case lib.Message.Action.SendCopy:
                    clipboardManager.writeClipboard(msg.ClipboardContent);
                    break;
                default:
                    logger.Info("Unkown action. Message:{0}", msg.ToJson());
                    break;
            }
        }

        private void MenuItemClickExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItemClickSettings(object sender, RoutedEventArgs e)
        {
            try
            {
                new Process()
                {
                    StartInfo = new ProcessStartInfo(Configuration.configurationFileLoation)
                }.Start();
            }catch(Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }
    }
   

    }
    public class QRCUtil
    {
        static QRCodeGenerator qrcg = new QRCodeGenerator();
        public static DrawingImage getQRC(string str)
        {
            QRCodeData qRCodeData = qrcg.CreateQrCode(str, QRCodeGenerator.ECCLevel.M);
            return new XamlQRCode(qRCodeData).GetGraphic(64);
        }
        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

    }
public class NetWorkUtil
{
    static private Logger logger = LogManager.GetCurrentClassLogger();
    static int bufferSize = 10 * 1024 * 1024;
    static byte[] buffer = new Byte[bufferSize];
    public static IPEndPoint serverEndPoint;
    public static Socket client = null;

    public static lib.Message receivce(Socket handler)
    {
        int bytesRec = handler.Receive(buffer);
        string content = Encoding.UTF8.GetString(buffer, 0, bytesRec);
        return JsonConvert.DeserializeObject<lib.Message>(content);
    }
    public static LinkedList<IPAddress> getLocalIPv4WithGateway()
    {
        LinkedList<IPAddress> forRet = new LinkedList<IPAddress>();
        NetworkInterface[] allInterface = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var itf in allInterface)
        {
            var gateways = itf.GetIPProperties().GatewayAddresses;
            if (gateways.Count > 0)
            {
                foreach (var address in itf.GetIPProperties().UnicastAddresses)
                {
                    if (address.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        forRet.AddLast(address.Address);
                    }
                }
            }
        }
        return forRet;
    }
}
