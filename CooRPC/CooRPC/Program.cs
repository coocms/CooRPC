using CooRPCCore;
using System;

namespace CooRPC
{
    class Program
    {
        static void Main(string[] args)
        {
            CooClient client = new CooClient() { ip = "127.0.0.1", port = 8909};
            client.Start();

            while (true)
            {
                Console.Write(">>");
                var str = Console.ReadLine();
                client.Send(str);

            }
        }
    }
}
