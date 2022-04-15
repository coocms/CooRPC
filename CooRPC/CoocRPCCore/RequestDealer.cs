using Autofac;
using CoocRPCCore;
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
    class RequestModelContext
    {
        public RequestModel request { get; set; }
        public System.Net.Sockets.TcpClient client { get; set; }

        public string guid { get; set; }

    }
    class ResultModel
    {
        public byte[] result { get; set; }
        public System.Net.Sockets.TcpClient client { get; set; }

        public string guid { get; set; }
    }
    public class RequestDealer
    {
        static ContainerBuilder builder = new ContainerBuilder();
        static IContainer container;
        static List<Type> iServiceTypes = new List<Type>();

        System.Collections.Concurrent.ConcurrentQueue<RequestModelContext> requestModelQueue = new System.Collections.Concurrent.ConcurrentQueue<RequestModelContext>();

        System.Collections.Concurrent.ConcurrentQueue<ResultModel> resultModelQueue = new System.Collections.Concurrent.ConcurrentQueue<ResultModel>();
        static RequestDealer()
        {

            //Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Assembly> assemblies = new List<Assembly>();

            List<string> files = Directory.GetFiles(Directory.GetCurrentDirectory()).ToList().Where(o => o.Contains(".dll") && !o.Contains("System.") && !o.Contains("Microsoft.")).ToList();
            files.ForEach(o =>
            {
                assemblies.Add(Assembly.LoadFile(o));
            });

            

            
            assemblies.ToList().ForEach(o =>
            {
                Console.WriteLine(o.FullName);
            });
            List<Type> serviceTypes = new List<Type>();
            foreach (Assembly ass in assemblies)
            {
                
                List<Type> t = ass.GetTypes().ToList().Where(type =>
                {
                    return type.IsClass && type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICoocRPCService<>)).Count() > 0;

                })?.ToList();
                if(t.Count > 0)
                    serviceTypes.AddRange(t);
            }
            foreach (var item in serviceTypes)
            {
                Type IServiceType = item.GetInterfaces().Where(o=>o.IsGenericType == false).FirstOrDefault();
                if (IServiceType != null)
                {
                    Console.WriteLine("register type = {0} iservice type = {1}", item.FullName, IServiceType.FullName);
                    builder.RegisterType(item).As(IServiceType);
                    iServiceTypes.Add(IServiceType);
                }   
            }

            container = builder.Build();
        }

        public static object Call(RequestModel requestModel, Func<byte[], Type, object> deserializeFunc)
        {
            Type iServiceType = iServiceTypes.FirstOrDefault(o => o.FullName == requestModel.assemblyName);
            if (iServiceType == null)
            {
                Console.WriteLine("iServiceType = null");
                return null;
            }

            object service = container.Resolve(iServiceType);
            MethodInfo method = service.GetType().GetMethods().FirstOrDefault(o => o.Name == requestModel.methodName && o.GetParameters().Length == requestModel.args.Count);
            if (method == null)
            {
                Console.WriteLine("method = null");
                return null;
            }
                
            if (!(method.GetParameters().Count() == requestModel.args.Count))
            {
                Console.WriteLine("!(method.GetParameters().Count() == requestModel.args.Count)");
                return null;
            }
            List<object> argsArray = new List<object>();
            ParameterInfo[] pp = method.GetParameters();
            for (int i = 0; i < requestModel.args.Count; i++)
            {
                Type target = pp[i].ParameterType;
                argsArray.Add(deserializeFunc(requestModel.args[i], target));
            }
            

            var res = method.Invoke(service, argsArray.ToArray());
            return res;
        }
        public void RequestSplit(TcpServer server, Func<byte[], Type, object> deserializeFunc)
        {
            List<byte> temp = new List<byte>();
            System.Collections.Concurrent.ConcurrentQueue<TcpMessageModel> tcpStringQueue = server.TcpStringQueue;
            while (true)
            {
                bool bFirstNotParse = false;
                if (tcpStringQueue.TryDequeue(out TcpMessageModel tcpStringModel))
                {
                    Console.WriteLine("RequestSplit: " + Encoding.ASCII.GetString(tcpStringModel.message).Trim());
                    
                    List<byte[]> requests = ByteSplit(tcpStringModel.message, ((byte)'|'));
                    if (temp.Count > 0)
                    {
                        temp.AddRange(requests.First());
                        var request = deserializeFunc(temp.ToArray(), typeof(RequestModel)) as RequestModel;
                        requestModelQueue.Enqueue(new RequestModelContext { request = request, client = tcpStringModel.client, guid = request.guid});

                        temp.Clear();
                        bFirstNotParse = true;
                    }

                    byte[] lastMessage = requests.Last();
                    if (lastMessage.Length > 0)
                    {
                        //最后一组数据有值
                        if(lastMessage.ToList().Count(o=>o != 0) > 0)
                            temp = lastMessage.ToList();
                    }

                    for (int i = bFirstNotParse ? 1 : 0; i < requests.Count - 1; i++)
                    {
                        RequestModel request = null;
                        request = deserializeFunc(requests[i], typeof(RequestModel)) as RequestModel;

                        if (request == null)
                            continue;

                        requestModelQueue.Enqueue(new RequestModelContext { request = request, client = tcpStringModel.client, guid = request.guid});
                        
                        //var res = Call(request);
                        //messageModel.client.GetStream().WriteAsync(System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(new ResponseModel { resultList = res, guid = request.guid })));

                    }

                }
                Thread.Sleep(10);
            }
        }
        static List<byte[]> ByteSplit(byte[] bytes, byte splitByte)
        {
            List<byte[]> res = new List<byte[]>();
            List<byte> temp = new List<byte>();

            for (int i = 0; i < bytes.Length; i++)
            {

                if (bytes[i] == splitByte)
                {
                    if (temp.Count != 0)
                    {
                        res.Add(temp.ToArray());
                        //temp.ForEach(o =>
                        //{
                        //    Console.Write("{0}  ", o);
                        //});
                        //Console.Write("\n\n");
                        temp.Clear();
                    }
                }
                else
                {
                    temp.Add(bytes[i]);
                }
            }

            res.Add(temp.ToArray());

            return res;
        }

        public void RequestDeal(Func<object, byte[]> serializeFunc, Func<byte[], Type, object> deserializeFunc)
        {
            while (true)
            {
                RequestModelContext requestModel;
                if (requestModelQueue.TryDequeue(out requestModel))
                {
                    object res = Call(requestModel.request, deserializeFunc);
                    resultModelQueue.Enqueue(new ResultModel { result = serializeFunc(res), client = requestModel.client, guid = requestModel.guid });

                }
                Thread.Sleep(10);
            }
        }

        public void ResultSendToClient(Func<object, byte[]> serializeFunc)
        {
            while (true)
            {
                ResultModel resultModel = null;
                if (resultModelQueue.TryDequeue(out resultModel))
                {
                    System.Net.Sockets.TcpClient client = resultModel.client;
                    byte[] responseMessage = serializeFunc(new ResponseModel { result = resultModel.result, guid = resultModel.guid });
                    List<byte> temp = responseMessage.ToList();
                    temp.Add((byte)'|');
                    client.GetStream().WriteAsync(temp.ToArray());
                }
                Thread.Sleep(10);
            }
        }
    }
}
