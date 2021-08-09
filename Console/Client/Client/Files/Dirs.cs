using System.IO;

namespace Client.Files
{
    public static class Dirs
    {
        public static void ClearEmptyDirs(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                ClearEmptyDirs(directory);
                if (Directory.GetFiles(directory).Length == 0 && 
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
            }
        }
    }
}