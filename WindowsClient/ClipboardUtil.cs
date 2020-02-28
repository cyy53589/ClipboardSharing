using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WK.Libraries.SharpClipboardNS;
using NLog;
using System.Windows;
using System.Threading;
using System.Diagnostics;

namespace ClipboardSharing
{
    public class ClipboardManager
    {
        protected ClipboardManager(Action<string> SendClipboard)
        {
            sharpClipboard = new SharpClipboard();
            sharpClipboard.ClipboardChanged += Clipboard_ClipboardChanged;
            this.SendClipboard = SendClipboard;
        }
        public static ClipboardManager GetManager(Action<string> SendClipboard, MainWindow main)
        {
            mainWindow = main;
            return new ClipboardManager(SendClipboard);
        }

        static MainWindow mainWindow;
        public void writeClipboard(string clip_data)
        {
            lock (textReceived) { lastWrite = clip_data; }
            mainWindow.Dispatcher.Invoke(() =>
            {
                int tryTime = 10;
                bool isSuc = false;
                while (tryTime-- != 0 && isSuc == false)
                {
                    try
                    {
                        Clipboard.SetText(clip_data);
                        isSuc = true;
                        Debug.WriteLine("I'm writing {0}", clip_data);
                    }
                    catch (Exception e) {; }
                }
                if (tryTime == 0) logger.Error("无法访问剪贴板");
            });
        }

        string lastWrite = null;

        protected void Clipboard_ClipboardChanged(object sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            lock (textReceived)
            {
                // Is the content copied of text type?
                if (e.ContentType == SharpClipboard.ContentTypes.Text && e.Content.ToString() != lastWrite)
                {
                    logger.Info("Clipboard changed. content: {0}", e.Content.ToString());
                    SendClipboard(e.Content.ToString());
                }
            }
        }

        Action<string> SendClipboard;
        static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        int clipboardChangeTime = -1;
        string clipboardTextWritedByMe = null;
        readonly object textReceived = new object();
        SharpClipboard sharpClipboard;

    }
}
