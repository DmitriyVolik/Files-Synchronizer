using System;
using System.Configuration;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Client.Helpers;

namespace Chat.Packets
{
    public static class PacketFile
    {
        

        public static void GetFile(string FileName)
        {
            string mainDir = ConfigurationManager.AppSettings.Get("MainDirectory");
            
            using (var client = TcpHelper.GetClient())
            {
                using (var stream = client.GetStream())
                {
                    PacketSender.SendJsonString(stream, "GET:FILE:" + FileName);

                    if (!Directory.Exists(FileName))
                    {
                        Directory.CreateDirectory(mainDir+Path.GetDirectoryName(FileName));
                    }
                    
                    using (var output = File.Create(mainDir + FileName))
                    {
                        Console.WriteLine("Client connected. Starting to receive the file");

                        while (true)
                        {
                            Console.WriteLine("start wait file");

                            var buffer = new byte[1024];
                            var bytes = 0;
                            do
                            {
                                Console.WriteLine("start write");
                                bytes = stream.Read(buffer, 0, buffer.Length);
                                output.Write(buffer, 0, bytes);
                            } while (stream.DataAvailable);

                            if (bytes > 0) break;
                        }
                    }
                }
            }
        }
    }
}