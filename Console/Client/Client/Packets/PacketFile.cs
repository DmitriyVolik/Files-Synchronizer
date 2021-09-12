using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using Client.Files;
using Client.Helpers;
using Client.Models;

namespace Chat.Packets
{
    public static class PacketFile
    {
        
        public static void GetFile(string FileName, List<FileM>ServerFiles, string token, string mainDir)
        {

            using (var client = TcpHelper.GetClient())
            {
                using (var stream = client.GetStream())
                {
                   
                    PacketSender.SendJsonString(stream, "GET:FILE:"+FileName+":"+token);

                    if (!Directory.Exists(FileName))
                    {
                        Directory.CreateDirectory(mainDir+Path.GetDirectoryName(FileName));
                    }
                    
                    using (var output = File.Create(mainDir + FileName))
                    {
                        List<FileM> clientFiles = new List<FileM>();

                        var candidateFile = ServerFiles.FirstOrDefault(x => x.Path == FileName);
                        long count=0;

                        do
                        {
                            var buffer = new byte[client.SendBufferSize];
                            var bytes = 0;
                            
                            do
                            {
                               
                                bytes = stream.Read(buffer, 0, buffer.Length);
                                count += bytes;
                                output.Write(buffer, 0, bytes);
                                
                            } while (stream.DataAvailable);

                        } while (candidateFile.Size> count);
                    }
                }
            }
        }
    }
}