using System;
using System.Net;
using Fleck;
using WebSocket4Net;
using System.Collections;
using System.Threading;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Concurrent;
using NLog;

/// <summary>
/// Message action:
///     BindDevice: 1.Register username. 2. Server send clipboard buffer saved at server to this client if server has.
///     SendCopy: 1. Register username. 2. Server send clipboard content from this client to other client.
/// </summary>
namespace ClipboardSharing
{
    public class Server
    {
        static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            var server = new Server(5358);
            while (true)
            {
                Thread.Sleep(3600 * 1000); // scan invalid connection evey n-sec
                UserTable.RemoveInvalidConnection();
            }
        }
     
        public Server(int listenPort)
        {

            lib.LogHelper.InitLogger();
            var server = new Fleck.WebSocketServer(string.Format("ws://0.0.0.0:{0}", listenPort))
            {
                RestartAfterListenError = true
            };
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    GetLogerOfSocket(socket).Info("Receive a new client.");
                };
                socket.OnClose = () => Server_OnClose(socket);
                socket.OnMessage = (string msg) => Server_OnMessage(socket, msg);
                socket.OnError = (Exception exc) => Server_OnError(socket, exc);
            });
        }
        private Logger GetLogerOfSocket(IWebSocketConnection socket)
        {
            return NLog.LogManager.GetLogger(string.Format("Client={0}:{1}", socket.ConnectionInfo.ClientIpAddress, socket.ConnectionInfo.ClientPort));
        }
        private void Server_OnClose(IWebSocketConnection socket)
        {
            var logger = GetLogerOfSocket(socket);
            logger.Info("Client close socket.");
            foreach (var userTable in UserTable.TablePool.Values)
            {
                if (userTable.SocketSet.TryRemove(socket, out byte _))
                {
                    logger.Info("Remove client[username={0}] from connection pool",userTable.UserName);
                }
            }

        }
        private void Server_OnError(IWebSocketConnection socket,Exception exception)
        {
            var logger = GetLogerOfSocket(socket);
            logger.Info(exception);
            foreach (var userTable in UserTable.TablePool.Values)
            {
                if (userTable.SocketSet.TryRemove(socket, out byte _))
                {
                    logger.Info("Remove client[username={0}] from connection pool", userTable.UserName);
                }
            }
            socket.Close();
        }
        private void Server_OnMessage(IWebSocketConnection socket, string encryptedMsg)
        {
            try
            {
                var logger = GetLogerOfSocket(socket);

                lib.Message msg = lib.Message.FromEncryJson(encryptedMsg);
                logger.Info("Receive msg. Msg => {0}", msg);

                switch (msg.Act)
                {
                    case lib.Message.Action.BindDevice:
                        BindDevice(socket, msg);
                        break;
                    case lib.Message.Action.SendCopy:
                        SendCopy(socket, msg);
                        break;
                }
                logger.Info("End receive msg");
            }
            catch (lib.Encrypt.EncryptException encryptEcetion)
            {
                logger.Info(encryptEcetion);
            }
            catch (Exception e)
            {
                try
                {
                    logger.Info("Bad message.JsonContent={0}", lib.Encrypt.EncryptFunc(encryptedMsg));
                }
                catch (Exception e2) { logger.Info(e2); }
            }
        }
        private UserTable Register(IWebSocketConnection socket,string username)
        {
            var logger = GetLogerOfSocket(socket);

            UserTable userTable = UserTable.GetUserTable(username);
            if (userTable.InsertSocket(socket))
            {
                logger.Info("New client username={0}. Number of bind devices:{1}", userTable.UserName, userTable.BindSocketCount);
            }
            return userTable;
        }
        private void BindDevice(IWebSocketConnection socket,lib.Message msg)
        {
            var logger = GetLogerOfSocket(socket);

            var userTable = Register(socket, msg.Username);

            // send message in buffer
            if (userTable.msgBuffer != null)
            {
                socket.Send(new lib.Message()
                {
                    Act = lib.Message.Action.SendCopy,
                    ClipboardContent = userTable.msgBuffer,
                    Username = msg.Username
                }.ToEncryJson());
            }
        }
        private void SendCopy(IWebSocketConnection socket,lib.Message msg)
        {
            var logger = GetLogerOfSocket(socket);

            var userTable = Register(socket, msg.Username);

            logger.Info("Begin boardcast. Number of online devices:{0}", userTable.BindSocketCount);
            userTable.msgBuffer = msg.ClipboardContent;
            foreach (var _client in userTable.SocketSet)
            {
                var client = _client.Key;
                if (client == socket) continue;
                client.Send(msg.ToEncryJson());
            }
        }
    }
}
