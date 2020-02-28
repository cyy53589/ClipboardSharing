using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClipboardSharing;
using WebSocket4Net;
using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Text;

namespace ServerTest
{
    class Client : WebSocket
    {
        public string Name;
        public Boolean sendBindMsg = true;
        public Client(int serverPort, string name,bool sendBind=true) : base(string.Format("ws://127.0.0.1:{0}", serverPort))
        {
            Name = name;
            this.sendBindMsg = sendBind;
        }
        
    }
    public class Test
    {
        static int serverPort = 5859;
        static WebSocket client = new Client(serverPort, "PC-A"), client2 = new Client(serverPort, "Phone-A");
        static Client client3 = new Client(serverPort, "PC-B"), client4 = new Client(serverPort, "Phone-B",false);
        static void Main(String[] args)
        {
            Console.WriteLine(new lib.Message()
            {
                Act = lib.Message.Action.BindDevice,
                ClipboardContent = "Cyy's ClipboardContent",
                Username = "Cyy"
            }.ToEncryJson());
            Console.WriteLine(lib.Message.FromJson(Console.ReadLine()));
            return;

            Console.WriteLine(new lib.Message()
            {
                Act = lib.Message.Action.BindDevice,
                Username = "ctest"
            }.ToEncryJson());
            var server = new Server(5859);

            client.Opened += Client_Opened;
            client.MessageReceived += Client_MessageReceived;
            client.Open();

            client2.Opened += Client_Opened;
            client2.MessageReceived += Client_MessageReceived;
            client2.Open();

            client3.Opened += Client_Opened_B;
            client3.EnableAutoSendPing = true;
            client3.AutoSendPingInterval = 10;
            client3.MessageReceived += Client_MessageReceived;
            client3.Open();

            client4.Opened += Client_Opened_B;
            client4.MessageReceived += Client_MessageReceived;
            client4.Open();

            Console.Read();

            client4.Close();
            client3.Send(new lib.Message()
            {
                Act = lib.Message.Action.SendCopy,
                Username = "UserName-B",
                ClipboardContent = string.Format("[This is clipboard content from {0}. after Phone-B close]", client3.Name)
            }.ToEncryJson());
            Thread.Sleep(60 * 60);
        }

        static private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            string name = ((Client)sender).Name;
            Console.WriteLine("[Client Name={1}]Receive message {0}", lib.Message.FromEncryJson(e.Message), name);
        }

        static private void Client_Opened(object sender, System.EventArgs e)
        {
            string name = ((Client)sender).Name;
            Console.WriteLine("[Client Name={0}] Send msg connect", name);
            ((Client)sender).Send(
                new lib.Message()
                {
                    Act = lib.Message.Action.BindDevice,
                    Username = "UserName-A"
                }.ToEncryJson());

            ((Client)(sender)).Send(new lib.Message()
            {
                Act = lib.Message.Action.SendCopy,
                Username = "UserName-A",
                ClipboardContent = string.Format("[This is clipboard content from {0}]", name)
            }.ToEncryJson()
                );
        }
        static private void Client_Opened_B(object sender, System.EventArgs e)
        {
            var client = ((Client)sender);
            string name = ((Client)sender).Name;

            if (client.sendBindMsg)
            {
                Console.WriteLine("[Client Name={0}] Send Bind Connect", name);
                ((Client)sender).Send(
                    new lib.Message()
                    {
                        Act = lib.Message.Action.BindDevice,
                        Username = "UserName-B"
                    }.ToEncryJson());
            }

            ((Client)(sender)).Send(new lib.Message()
            {
                Act = lib.Message.Action.SendCopy,
                Username = "UserName-B",
                ClipboardContent = string.Format("[This is clipboard content from {0}]",name)
            }.ToEncryJson()
                );
        }
    }
}
