using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace lib
{
    public class Encrypt
    {
        public delegate string EnDecryptFunc(string strToEnDe);
        static public EnDecryptFunc EncryptFunc = DesEncrypt;
        static public EnDecryptFunc DecryptFunc = DesDecrypt;
        static string DesEncrypt(string strText)
        {
            try
            {
                byte[] byKey = null;

                byKey = Encoding.UTF8.GetBytes(strEncrKey.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                des.Padding = PaddingMode.PKCS7;
                byte[] inputByteArray = Encoding.UTF8.GetBytes(strText);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch
            {
                throw new EncryptException("Encrypt error");
            }
        }

        static string DesDecrypt(string strText)
        {
            try
            {
                byte[] byKey = null;

                byte[] inputByteArray = new Byte[strText.Length];

                byKey = Encoding.UTF8.GetBytes(strEncrKey.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                des.Padding = PaddingMode.PKCS7;
                inputByteArray = Convert.FromBase64String(strText);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                Encoding encoding = new UTF8Encoding();
                return encoding.GetString(ms.ToArray());
            }
            catch
            {
                throw new EncryptException("Encrypt error");
            }
        }
        private static string strEncrKey = "Zxkl;kenshyl";
        private static byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x11, 0x13, 0x14, 0x15 };
        public class EncryptException :Exception{
            public EncryptException(string msg) : base(msg)
            {

            }
        }
    }
}
