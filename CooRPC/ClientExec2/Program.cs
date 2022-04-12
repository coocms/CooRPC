using CooRPCCore;
using RPCIServices;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ClientExec2
{
    class Program
    {
        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(5000, 1000);
            
            object countLock = new object();
            var service = new CooRPCClient("127.0.0.1", 8909).Create<ITestService>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int count = 0;
            for (long i = 0; i < 500; i++)
            {

                long ii = i;

                Task.Run(() =>
                {
                    long res = service.Add(ii, ii);
                    Console.WriteLine(ii + " Return" + res);
                });
                //lock (countLock)
                //    Console.WriteLine("Count = " + count++);

                //Task.Run(() =>
                //{
                //    long res = service.Add(ii, ii);
                //    Console.WriteLine(ii + " Return" + res);
                //}).ContinueWith(o =>
                //{
                //    lock (countLock)
                //    {
                //        if (++count % 10000 == 0)
                //        {
                //            stopwatch.Stop();
                //            Console.WriteLine("Count = {0}, spendTime = {1}ms", count, stopwatch.ElapsedMilliseconds);
                //            stopwatch.Restart();
                //        }
                //    }
                //});
            }
            Console.WriteLine("Console.Read();");
            Console.Read();
        }
    }
}
