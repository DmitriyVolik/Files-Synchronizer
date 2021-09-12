using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Chat.Packets;
using Client.Files;
using Client.Helpers;
using Client.Models;
using Client.Workers;
using Server.Files;

namespace Client
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var userData = JsonWorker<UserData>.JsonToObj(File.ReadAllText(currentPath + @"\" + "UserData.json"));

            var mainDir = userData.FolderPath;

            var serverFiles = new List<FileM>();

            string data;

            using (var client = TcpHelper.GetClient())
            {
                using (var stream = client.GetStream())
                {
                    PacketSender.SendJsonString(stream,
                        "GET:FILES:LIST:" + Encryptor.EncodeDecrypt(userData.SessionToken));

                    data = PacketRecipient.GetJsonData(stream, client.SendBufferSize);
                }
            }

            serverFiles = JsonWorker<List<FileM>>.JsonToObj(data);

            if (serverFiles.Count == 0)
            {
                var folder = new DirectoryInfo(mainDir);

                foreach (var file in folder.GetFiles()) file.Delete();

                foreach (var dir in folder.GetDirectories()) dir.Delete(true);
            }


            var files = new List<FileM>();

            ScanFiles.Start(mainDir, files);

            long totalSize = 0;

            foreach (var item in serverFiles)
            {
                totalSize += item.Size;

                if (item.Size == 0)
                {
                    if (!Directory.Exists(item.Path))
                        Directory.CreateDirectory(mainDir + Path.GetDirectoryName(item.Path));
                    File.Create(mainDir + item.Path);
                    continue;
                }

                var selectedItem = files.FirstOrDefault(x => x.Path == item.Path);

                if (selectedItem == null)
                {
                    PacketFile.GetFile(item.Path, serverFiles, Encryptor.EncodeDecrypt(userData.SessionToken), mainDir);
                    continue;
                }

                if (item.Size != selectedItem.Size)
                {
                    File.Delete(mainDir + selectedItem.Path);
                    PacketFile.GetFile(item.Path, serverFiles, Encryptor.EncodeDecrypt(userData.SessionToken), mainDir);
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
                        PacketFile.GetFile(item.Path, serverFiles, Encryptor.EncodeDecrypt(userData.SessionToken),
                            mainDir);
                    }
                }
            }

            foreach (var i in files)
                if (!serverFiles.Exists(x => x.Path == i.Path))
                    File.Delete(mainDir + i.Path);

            Dirs.ClearEmptyDirs(mainDir);

            long totalSizeClient = 0;

            var clientFiles = new List<FileM>();
        }
    }
}