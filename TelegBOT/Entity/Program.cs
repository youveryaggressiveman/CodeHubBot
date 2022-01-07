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

            client.StartListen();

            Console.ReadKey();
        }
    }
}
