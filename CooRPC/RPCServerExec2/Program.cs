using RPCServices;
using System;
using System.Threading;

namespace RPCServerExec2
{
    class Program
    {
        static void Main(string[] args)
        {
            var o = typeof(TestService);


            ThreadPool.SetMinThreads(5000, 500);





            new CooRPCCore.CooRPCServer("0.0.0.0", 8909);




            while (true)
            {
                Console.WriteLine("Health Check");
                Thread.Sleep(100000);
                
            }
        }
    }
}
