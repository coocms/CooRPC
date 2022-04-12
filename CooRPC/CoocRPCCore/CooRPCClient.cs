using Castle.DynamicProxy;
using CooRPCCore.Interface;
using CooRPCCore.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CooRPCCore
{
    public class CooRPCClient
    {
        static Dictionary<string, Type> typeDic = new Dictionary<string, Type>();

        static CooRPCClient()
        {
            

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly ass in assemblies)
            {
                List<Type> t = ass.GetTypes().ToList().Where(type =>
                {
                    return type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICoocRPCService<>)).Count() > 0;

                })?.ToList();

                t.ForEach(o =>
                {
                    if (!typeDic.ContainsKey(o.Name))
                        typeDic.Add(o.Name, o);
                });
            }


        }

        public TcpClient client;
        public ResponseDealer responseDealer = new ResponseDealer();
        public CooRPCClient(string ip, int port)
        {
            client = new TcpClient() { ip = ip, port = port };
            client.Start();
            myIntercept = new MyInterceptor(client, responseDealer);

            Thread thread = new Thread(()=>responseDealer.ResponseSplit(client));
            thread.IsBackground = true;
            thread.Start();

            Thread thread1 = new Thread(() => responseDealer.DequeueResToList());
            thread1.IsBackground = true;
            thread1.Start();
        }

        internal class MyInterceptor : IInterceptor
        {
            TcpClient client;
            ResponseDealer responseDealer;
            System.Collections.Concurrent.ConcurrentQueue<string> requestMsgs = new System.Collections.Concurrent.ConcurrentQueue<string>();
            public MyInterceptor(TcpClient client, ResponseDealer responseDealer)
            {
                this.client = client;
                this.responseDealer = responseDealer;
                Thread thread = new Thread(Sender);
                thread.IsBackground = true;
                thread.Start();
            }

            public void Intercept(IInvocation invocation)
            {
                //Console.WriteLine("Intercept ");
                string fullName = invocation.Method.DeclaringType.FullName;
                string methodName = invocation.Method.Name;
                List<object> args = invocation.Arguments.ToList();

                Guid guid = Guid.NewGuid();
                string request = Newtonsoft.Json.JsonConvert.SerializeObject(new RequestModel { assemblyName = fullName, methodName = methodName, args = args, guid = guid.ToString() });
                requestMsgs.Enqueue(request + "|");
                //client.Send(request + "|");
                //var res = client.GetReceiveMessage(guid);


                //Console.WriteLine("After Send Now Waitting");
                Task<object> tt = responseDealer.GetResult(guid.ToString());
                invocation.ReturnValue = tt.GetAwaiter().GetResult();

            }
            public void Sender()
            {
                while (true)
                {
                    if (requestMsgs.TryDequeue(out string message))
                    {
                        Console.WriteLine("Start Send");
                        
                        client.Send(message);
                    }
                    Thread.Sleep(1);
                }
            }
        }
        ProxyGenerator generator = new ProxyGenerator();
        IInterceptor myIntercept;
        
        public T Create<T>() where T : class, ICoocRPCService<T>
        {
            Console.WriteLine("Create Proxy");
            return generator.CreateInterfaceProxyWithoutTarget<T>(myIntercept);
        }


    }
}
