using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CooRPCCore
{
    /// <summary>
    /// 客户端通信对象
    /// </summary>
    public class TcpClient : IDisposable
    {
        public string ip { get; init; }
        public int port { get; init; } = 1231;
        Socket tcpClient;

        bool bStart = false;

        public ConcurrentQueue<byte[]> responseMessage = new ConcurrentQueue<byte[]>();

        public void Start()
        {
            if (bStart)
                return;
            #region 建立Tcp连接
            
            tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            IPAddress ipaddress = IPAddress.Parse(ip);
            EndPoint point = new IPEndPoint(ipaddress, port);
            tcpClient.Connect(point);//通过IP和端口号来定位一个所要连接的服务器端
            
            #endregion


            Thread thread = new Thread(ReceiveThread);
            thread.IsBackground = true;
            thread.Start();
            bStart = true;
        }

        public void Send(string msg)
        {
            //tcpClient.Send(Encoding.UTF8.GetBytes(msg));

            byte[] buff = Encoding.ASCII.GetBytes(msg);
            tcpClient.BeginSend(Encoding.UTF8.GetBytes(msg), 0, buff.Length, 0, null, null);
        }
        public void Send(byte[] bytes)
        {
            //tcpClient.Send(bytes);
            tcpClient.BeginSend(bytes, 0, bytes.Length, 0, null, null);
        }
        List<string> receiveMessages = new List<string>();
        private readonly object msgLock = new object();

        public async Task<string> GetReceiveMessage(Guid guid)
        {
            List<string> tp;
            while (true)
            {
                var res = await Task.Delay(200).ContinueWith(o =>
                {
                    lock (msgLock)
                        tp = receiveMessages;

                    var res = tp.Where(o => o.Contains(guid.ToString())).FirstOrDefault();
                    if (!string.IsNullOrEmpty(res))
                    {
                        lock (msgLock)
                            receiveMessages = receiveMessages.Where(o => !o.Contains(guid.ToString())).ToList();
                        return res;
                    }

                    else return null;
                });
                if (string.IsNullOrEmpty(res))
                    continue;
                else
                    return res;
            }
        }

        public void ReceiveThread()
        {
            byte[] data = new byte[1024];
            //传递一个byte数组，用于接收数据。length表示接收了多少字节的数据

            while (true)
            {
                int length = tcpClient.Receive(data);
                
                responseMessage.Enqueue(data.Take(length).ToArray());
            }

        }

        public void Dispose()
        {
            tcpClient?.Disconnect(true);
            tcpClient?.Dispose();
        }
    }
}
