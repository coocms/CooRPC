using CooRPCCore;
using RPCIServices;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientExec2
{

    
    class Program
    {
        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(5000, 1000);
            

            for (int i = 0; i < 1; i++)
            {
                Task.Run(() =>
                {
                    var cooClient = new CooRPCClient();
                    cooClient.ConfigConnection("127.0.0.1", 8909);
                    cooClient.ConfigSerialize(o =>
                    {
                        return MessagePack.MessagePackSerializer.Serialize(o);

                    })
                    .ConfigDeserialize((o, t) =>
                    {
                        return MessagePack.MessagePackSerializer.Deserialize(t, o);
                    });


                    cooClient.Build();

                    var service = cooClient.Create<ITestService>();
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    int count = 0;
                    for (long i = 0; i < 100; i++)
                    {

                        long ii = i;
                        Task.Run(() =>
                        {
                            //string res2 = service.StringAdd(ii.ToString(), ii.ToString());
                            var res = service.StringAddPro(ii.ToString(), ii.ToString());
                            Console.WriteLine(ii + " Return" + res.res + " " + res.test );
                        });

                    }

                });
            }



            Console.WriteLine("Console.Read();");
            Console.Read();
        }
    }
}
