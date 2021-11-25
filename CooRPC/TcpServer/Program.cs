using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener tcpListener = new TcpListener(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 1231));
            tcpListener.Start();
            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
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
                        data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
                        Console.WriteLine("接收消息:{0}", data);
                        stream.Write(bytes, 0, bytes.Length);
                    }
                });
            }
                




            //while (true)
            //{
            //    Console.Write("发送消息：");
            //    data = Console.ReadLine();//服务器发送消息
            //    byte[] msg = System.Text.Encoding.UTF8.GetBytes(data);
            //    stream.Write(msg, 0, msg.Length);
            //}
            
            //client.Close();


        }
    }
}
