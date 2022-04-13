using RPCServices;
using System;
using System.Threading;

namespace RPCServerExec2
{
    class Program
    {
        

        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(5000, 500);

            var server = new CooRPCCore.CooRPCServer()
            .ConfigConnection("0.0.0.0", 8909);
            server.ConfigSerialize(o =>
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(o);
            });
            server.ConfigDeserialize((o, t) =>
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject(o, t);
            });

            server.Build();

            while (true)
            {
                Console.WriteLine("Health Check");
                Thread.Sleep(100000);
                
            }
        }
    }
}
