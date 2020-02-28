using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using lib;

namespace ServerTest
{
    class TestServerAndWindowsClient
    {
        public static void Main()
        {
            Console.ReadLine();
            lib.Client client = new lib.Client("CYY",func , "127.0.0.1", 5358);
            client.Open();
            Console.WriteLine("Enter any key to send message to server");
            Thread.Sleep(2000);
            client.SendClipboardToServer("[CYY-ROBOT send a msg");
            Thread.Sleep(10000000);
        }
        public static void func(lib.Message msg)
        {
            Console.WriteLine("CYY-ROBOT Received a msg. {0}",msg.ToJson());
        }
    }
}
