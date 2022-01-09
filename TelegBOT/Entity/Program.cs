using LoggerLib.Core.Services;
using System;
using System.Runtime.InteropServices.ComTypes;
using TelegBOT.Core;

namespace TelegBOT.Entity
{
    class Program
    {
        static void Main(string[] args)
        {
            TGClient client = new TGClient();

            IDirectoryManager directoryManager = new DirectoryManager(new System.IO.DirectoryInfo("data/log"));

            var dr = directoryManager.Initialize();

            var fileManager = LoggerSinglton.GetFileManager();

            LoggerSinglton.FileInfo = fileManager.CreateFile(dr, DateTime.Now.ToString().Replace(".", "").Replace(" ", "").Replace(":", "") + ".txt");

            LoggerSinglton.DirectoryInfo = dr;

            client.StartListen();

            Console.ReadKey();
        }
    }
}
