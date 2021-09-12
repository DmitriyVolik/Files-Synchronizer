using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using Client.Files;
using Client.Models;

namespace Server.Files
{
    public static class ScanFiles
    {
        private static string _cutDir;

        public static void Start(string targetDirectory, List<FileM> list)
        {
            _cutDir = targetDirectory;

            ProcessDirectory(targetDirectory, list);
        }
        public static void ProcessDirectory(string targetDirectory, List<FileM> list)
        {
            // Process the list of files found in the directory.
            string [] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                FileM file = new FileM(){Path = fileName.Replace(_cutDir, "") };
                file.Size=new FileInfo(fileName).Length;
                list.Add(file);
            }

            // Recurse into subdirectories of this directory.
            string [] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach(string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory, list);
        }

        // Insert logic for processing found files here.

    }
    
}