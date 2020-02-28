using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;

namespace lib
{
    public class Client : WebSocket
    {
        string UserName;
        Action<Message> ReceiveCallback;
        public Client(string username, Action<Message> receiveCallback, string serverIp, int port)
            : base(string.Format("ws://{0}:{1}", serverIp, port))
        {
            UserName = username;
            this.ReceiveCallback = receiveCallback;

            this.Opened += Client_Opened;
            this.MessageReceived += Client_MessageReceived;
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            ReceiveCallback(Message.FromEncryJson(e.Message));
        }
        public void Send(Message msg)
        {
            Send(msg.ToEncryJson());
        }

        private void Client_Opened(object sender, EventArgs e)
        {
            Send(new lib.Message()
            {
                Act = lib.Message.Action.BindDevice,
                Username = UserName
            });
        }

        public void SendClipboardToServer(string clipboardContent)
        {
            this.Send(
                new lib.Message()
                {
                    Act = lib.Message.Action.SendCopy,
                    ClipboardContent = clipboardContent,
                    Username = UserName
                });
        }
    }
}
