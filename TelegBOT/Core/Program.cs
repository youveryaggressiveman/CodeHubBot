using LoggerLib.Core.Services;
using System;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using TelegBOT.Core;

namespace TelegBOT.Core
{
    class Program
    {

        private static async Task StartBot()
        {
            TGClient client = new TGClient();

            await client.StartListen();
        }

        static void Main(string[] args)
        {
            IDirectoryManager directoryManager = new DirectoryManager(new System.IO.DirectoryInfo("data/log"));

            var dr = directoryManager.Initialize();

            var fileManager = LoggerSinglton.GetFileManager();

            LoggerSinglton.FileInfo = fileManager.CreateFile(dr, DateTime.Now.ToString().Replace(".", "").Replace(" ", "").Replace(":", "") + ".txt");

            LoggerSinglton.DirectoryInfo = dr;

            StartBot();

            Console.ReadKey();
        }
    }
}
