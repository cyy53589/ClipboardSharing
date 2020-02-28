using Fleck;
using System.Collections.Concurrent;

namespace ClipboardSharing
{
    public class UserTable
    {
        public static ConcurrentDictionary<string, UserTable> TablePool = new ConcurrentDictionary<string, UserTable>();
        public ConcurrentDictionary<IWebSocketConnection, byte> SocketSet = new ConcurrentDictionary<IWebSocketConnection, byte>();
        public string UserName = null;
        public string msgBuffer = null;

        public int BindSocketCount
        {
            get { return SocketSet.Count; }
        }

        public UserTable(string username)
        {
            UserName = username;
        }

        /// <returns>If a NEW socket insert success</returns>
        public bool InsertSocket(IWebSocketConnection socket)
        {
            return SocketSet.TryAdd(socket, new byte());
        }

        /// if user tables do not contain key 'username', insert a empty table .
        public static UserTable GetUserTable(string username)
        {
            return TablePool.GetOrAdd(username, new UserTable(username));
        }
        /// <return>If connection already exits, return false. Otherwise return true </return>
        public static bool InsertConnections(string username, IWebSocketConnection cnn)
        {
            return GetUserTable(username).SocketSet.TryAdd(cnn, new byte());
        }
        public static void RemoveInvalidConnection()
        {
            var usernameToRm = new ConcurrentBag<string>();
            foreach (var name__table in TablePool)
            {
                var username = name__table.Key;
                var userTable = name__table.Value;
                var toRemove = new ConcurrentBag<IWebSocketConnection>();
                if (userTable.SocketSet.IsEmpty)
                {
                    usernameToRm.Add(username);
                }
                foreach (var socket_ in userTable.SocketSet)
                {
                    var socket = socket_.Key;
                    if (!socket.IsAvailable) toRemove.Add(socket);
                }
                foreach (var socket in toRemove)
                {
                    userTable.SocketSet.TryRemove(socket, out byte _);
                }
            }
            foreach (var i in usernameToRm)
            {
                TablePool.TryRemove(i, out var _);
            }
        }
    }
}
