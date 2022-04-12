using CoocRPCCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CooRPCCore
{
    public class CooRPCServer
    {
        TcpServer server;
        public CooRPCServer(string ip, int port)
        {
            server = new TcpServer() { ip = ip, port = port };
            server.Start();
            RequestDealer dealer = new RequestDealer();

            Thread thread = new Thread(()=> dealer.RequestSplit(server));
            thread.IsBackground = true;
            thread.Start();

            Thread thread2 = new Thread(() => dealer.RequestDeal());
            thread2.IsBackground = true;
            thread2.Start();

            Thread thread3 = new Thread(() => dealer.ResultSendToClient());
            thread3.IsBackground = true;
            thread3.Start();
        }





    }
}
