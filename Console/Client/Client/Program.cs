using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Chat.Packets;
using Client.Files;
using Client.Helpers;
using Client.Models;
using Client.Workers;

namespace Client
{
    internal class Program
    {

        public static void Main(string[] args)
        {
            var mainDir = ConfigurationManager.AppSettings.Get("MainDirectory");

            while (true)
            {
                try
                {
                    var serverFiles = new List<FileM>();

                    string data;

                    using (var client = TcpHelper.GetClient())
                    {
                        
                        using (var stream = client.GetStream())
                        {
                            Console.WriteLine(client.SendBufferSize);
                            PacketSender.SendJsonString(stream, "GET:FILES:LIST");

                            data = PacketRecipient.GetJsonData(stream, client.SendBufferSize);
                            
                        }
                    }
                    
                    Console.WriteLine(data);
                    Console.WriteLine("-----------------");
                    
                    serverFiles = JsonWorker.JsonToFiles(data);

                    if (serverFiles.Count == 0)
                    {
                        DirectoryInfo folder = new DirectoryInfo(mainDir);

                        foreach (FileInfo file in folder.GetFiles())
                        {
                            file.Delete();
                        }

                        foreach (DirectoryInfo dir in folder.GetDirectories())
                        {
                            dir.Delete(true);
                        }
                    }


                    var files = new List<FileM>();

                    ScanFiles.ProcessDirectory(mainDir, files);

                    long totalSize = 0;
                    
                    foreach (var item in serverFiles)
                    {
                        totalSize += item.Size;

                        if (item.Size == 0)
                        {
                            /*if (!Directory.Exists(item.Path))
                            {
                                Directory.CreateDirectory(mainDir+Path.GetDirectoryName(item.Path));
                            }*/
                            continue;
                        }

                        FileM selectedItem = files.FirstOrDefault(x => x.Path == item.Path);

                        if (selectedItem == null)
                        {
                            PacketFile.GetFile(item.Path, serverFiles);
                            continue;
                        }
                        else
                        {
                            Console.WriteLine(selectedItem.Path);
                        }

                        if (item.Size != selectedItem.Size)
                        {
                            File.Delete(mainDir + selectedItem.Path);
                            PacketFile.GetFile(item.Path,serverFiles);
                        }
                        else
                        {
                            string hash;

                            try
                            {
                                hash = CreateFileHash.CreateMD5(mainDir + selectedItem.Path);
                            }
                            catch (IOException)
                            {
                                continue;
                            }

                            if (hash != item.Hash)
                            {
                                File.Delete(mainDir + selectedItem.Path);
                                PacketFile.GetFile(item.Path,serverFiles );
                            }
                        }

                        
                    }
                    foreach (var i in files)
                    {
                        if (!serverFiles.Exists(x => x.Path == i.Path))
                        {
                            File.Delete(mainDir + i.Path);
                            continue;
                        }
                        
                    }

                    Dirs.ClearEmptyDirs(mainDir);

                    Console.WriteLine("---------------------------");
                    
                    long totalSizeClient=0;

                    List <FileM> clientFiles = new List<FileM>();
                    
                    /*while (totalSize != totalSizeClient)
                    {
                        
                        Thread.Sleep(500);
                        
                        clientFiles.Clear();
                        
                        ScanFiles.ProcessDirectory(mainDir, clientFiles);
                        totalSizeClient = 0;
                        foreach (var item in clientFiles)
                        {
                            totalSizeClient += item.Size;
                        }
                        
                        Console.WriteLine(totalSize);
                        Console.WriteLine(totalSizeClient);
                        Console.WriteLine("===================");
                    }*/
                    Thread.Sleep(5000);
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
        }
    }
}