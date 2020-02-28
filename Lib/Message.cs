using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace lib
{
    public class Message
    {
        /// <summary>
        /// BindDevice: data is username
        /// sendcopy: data is copyboard content
        /// </summary>
        public enum Action { Error=0,BindDevice = 2, SendCopy = 1 };
        public Action Act { get; set; }
        public string ClipboardContent { get; set; } //clipboard content
        public string Username { get; set; }
        public string ErrorMsg { get; set; }
        public override string ToString()
        {
            return ToJson();
        }
        public string ToEncryJson()
        {
            return Encrypt.EncryptFunc(this.ToJson());
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this,SettingsIgnoreNullValue);
        }
        public byte[] ToBytesUtf8()
        {
            return Encoding.UTF8.GetBytes(this.ToJson());
        }
        static public Message FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Message>(json,SettingsIgnoreNullValue);
        }
        static public Message FromEncryJson(string encryJson)
        {
            return FromJson(Encrypt.DecryptFunc(encryJson));
        }
        static JsonSerializerSettings SettingsIgnoreNullValue = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
    }
}
