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

        public TcpClient client;
        public ResponseDealer responseDealer = new ResponseDealer();
        Func<object, byte[]> serializeFunc = o =>
        {
            return Encoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(o));
        };
        Func<byte[], Type, object> deserializeFunc = (o, t) =>
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(Encoding.ASCII.GetString(o), t);
        };
        public CooRPCClient()
        {
        }
        public CooRPCClient ConfigSerialize(Func<object, byte[]> serializeFunc)
        {
            this.serializeFunc = serializeFunc;
            return this;
        }
        public CooRPCClient ConfigDeserialize(Func<byte[], Type, object> deserializeFunc)
        {
            this.deserializeFunc = deserializeFunc;
            return this;
        }
        public CooRPCClient ConfigConnection(string ip, int port)
        {
            client = new TcpClient() { ip = ip, port = port };
            
            return this;
        }
        public void Build()
        {
            client.Start();

            myIntercept = new MyInterceptor(client, responseDealer, serializeFunc, deserializeFunc);

            Thread thread = new Thread(() => responseDealer.ResponseSplit(client, deserializeFunc));
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
            System.Collections.Concurrent.ConcurrentQueue<byte[]> requestMsgs = new System.Collections.Concurrent.ConcurrentQueue<byte[]>();

            Func<object, byte[]> serializeFunc;
            Func<byte[], Type, object> deserializeFunc;
            public MyInterceptor(TcpClient client, ResponseDealer responseDealer, Func<object, byte[]> serializeFunc, Func<byte[], Type, object> deserializeFunc)
            {
                this.client = client;
                this.responseDealer = responseDealer;
                this.serializeFunc = serializeFunc;
                this.deserializeFunc = deserializeFunc;
                Thread thread = new Thread(Sender);
                thread.IsBackground = true;
                thread.Start();
            }

            public void Intercept(IInvocation invocation)
            {
                string fullName = invocation.Method.DeclaringType.FullName;
                string methodName = invocation.Method.Name;
                List<object> args = invocation.Arguments.ToList();

                Guid guid = Guid.NewGuid();
                

                var mo = new RequestModel
                {
                    assemblyName = fullName,
                    methodName = methodName,
                    args = args,
                    guid = guid.ToString()
                };
                
                byte[] request = serializeFunc(mo);

                
                var temp = request.ToList();
                
                temp.Add((byte)'|');
                
                requestMsgs.Enqueue(temp.ToArray());
                
                Task<byte[]> tt = responseDealer.GetResult(guid.ToString());
                var res = tt.GetAwaiter().GetResult();
                
                var returnType = invocation.Method.ReturnType;
                

                invocation.ReturnValue = deserializeFunc(res, returnType);

            }
            public void Sender()
            {
                while (true)
                {
                    if (requestMsgs.TryDequeue(out byte[] message))
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
            
            if (myIntercept == null)
            {
                Console.WriteLine("Please Build Before Create");
                return null;
            }

            return generator.CreateInterfaceProxyWithoutTarget<T>(myIntercept);
        }


    }
}
