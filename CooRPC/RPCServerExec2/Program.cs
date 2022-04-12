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





            new CooRPCCore.CooRPCServer("127.0.0.1", 8909);




            Console.Read();
        }
    }
}
