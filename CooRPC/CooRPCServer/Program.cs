using CoocRPCCore;
using System;

namespace CooRPCServer
{
    class Program
    {
        static void Main(string[] args)
        {
            CooServer server = new CooServer() { ip = "127.0.0.1", port = 8909 };
            server.Start();


            Console.WriteLine("Holding");
            Console.Read();


        }
    }
}
