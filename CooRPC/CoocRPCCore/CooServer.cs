using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace CoocRPCCore
{
    public class CooServer
    {
        public string ip { get; init; }
        public int port { get; init; } = 8908;
        TcpListener tcpListener;

        List<TcpClient> clients = new List<TcpClient>();
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
            //Byte[] bytes = new Byte[256];//缓存读入的数据
            //String data = null;
            while (true)//循环监听
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                
                Console.WriteLine("已连接！" + client.Client.RemoteEndPoint.ToString());
                
                clients.Add(client);
                //data = null;
                //NetworkStream stream = client.GetStream();//获取用于读取和写入的流对象
                //int i;
                //while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                //{
                //    //将借宿字节的数据转换成一个UTF8字符串
                //    data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
                //    Console.WriteLine("接收消息:{0}", data);
                //    //Console.Write("发送消息：");
                //    //data = Console.ReadLine();//服务器发送消息
                //    //byte[] msg = System.Text.Encoding.UTF8.GetBytes(data);
                //    //stream.Write(msg, 0, msg.Length);
                //}
                //client.Close();
            }
        }
    }
}
