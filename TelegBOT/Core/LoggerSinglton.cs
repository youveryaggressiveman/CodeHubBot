using LoggerLib.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegBOT.Core
{
    public static class LoggerSinglton
    {
        static FileManager fileManager;

        public static DirectoryInfo DirectoryInfo { get; set; }
        public static FileInfo FileInfo { get; set; }

        public static FileManager GetFileManager()
        {
            if (fileManager == null)
            {
                fileManager = new FileManager();
            }

            return fileManager;
        }
    }
}
