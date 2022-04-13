using CooRPCCore.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoocRPCCore
{
    public class TcpMessageModel
    {
        public System.Net.Sockets.TcpClient client { get; set; }
        public byte[] message { get; set; }
    }
    /// <summary>
    /// 服务端通信对象
    /// </summary>
    public class TcpServer
    {
        public string ip { get; init; }
        public int port { get; init; } = 8908;
        TcpListener tcpListener;


        public ConcurrentQueue<TcpMessageModel> TcpStringQueue = new System.Collections.Concurrent.ConcurrentQueue<TcpMessageModel>();

        Dictionary<string, TcpClient> clients { get; set; } = new Dictionary<string, TcpClient>();
        public void Start()
        {
            Thread thread = new Thread(ReceiveClientThread);
            thread.IsBackground = true;
            thread.Start();

        }
        void ReceiveClientThread()
        {
            tcpListener = new TcpListener(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), port));
            tcpListener.Start();//开始监听客户端的请求
            Console.WriteLine("CooRPC Server Start Successfull");
            while (true)//循环监听
            {
                
                TcpClient client = tcpListener.AcceptTcpClient();
                
                string clientHashCode = client.GetHashCode().ToString();
                clients.Add(clientHashCode, client);
                
                Task.Run(() =>
                {
                    Console.WriteLine("已连接！" + client.Client.RemoteEndPoint.ToString());
                    

                    string data = null;
                    byte[] bytes = new byte[1024];
                    NetworkStream stream = client.GetStream();//获取用于读取和写入的流对象
                    
                    int i;

                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        //将借宿字节的数据转换成一个UTF8字符串
                        //data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
                        //Console.WriteLine(data);
                        


                        TcpStringQueue.Enqueue(new TcpMessageModel { client = client, message = bytes.Take(i).ToArray() });
                        
                    }
                });
            }
        }

        

        


    }
}
