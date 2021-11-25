using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CooRPCCore
{
    public class CooClient
    {
        public string ip { get; init; }
        public int port { get; init; } = 1231;
        Socket tcpClient;
        public void Start()
        {
            #region 建立Tcp连接
            tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipaddress = IPAddress.Parse(ip);
            EndPoint point = new IPEndPoint(ipaddress, port);
            tcpClient.Connect(point);//通过IP和端口号来定位一个所要连接的服务器端
            #endregion


            Thread thread = new Thread(ReceiveThread);
            thread.IsBackground = true;
            thread.Start();
        }

        public void SendThread()
        {
            //向服务器端发送消息
            Console.Write("Client：");
            string message2 = Console.ReadLine();//读取用户的输入
            //将字符串转化为字节数组，然后发送到服务器端
            tcpClient.Send(Encoding.UTF8.GetBytes(message2));
        }
        public void Send(string msg)
        {
            tcpClient.Send(Encoding.UTF8.GetBytes(msg));
        }
        public void ReceiveThread()
        {
            byte[] data = new byte[1024];
            //传递一个byte数组，用于接收数据。length表示接收了多少字节的数据

            while (true)
            {
                int length = tcpClient.Receive(data);
                string message = Encoding.UTF8.GetString(data, 0, length);//只将接收到的数据进行转化
                Console.WriteLine("Server：" + message);
            }

        }
    }
}
