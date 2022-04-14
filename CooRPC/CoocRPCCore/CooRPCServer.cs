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
        readonly RequestDealer dealer = new RequestDealer();
        Func<object, byte[]> serializeFunc = o =>
        {
            return Encoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(o));
        };
        Func<byte[], Type, object> deserializeFunc = (o, t) =>
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(Encoding.ASCII.GetString(o), t);
        };

        public void Build()
        {
            server.Start();


            Thread thread = new Thread(() => dealer.RequestSplit(server, deserializeFunc));
            thread.IsBackground = true;
            thread.Start();

            Thread thread2 = new Thread(() => dealer.RequestDeal(serializeFunc));
            thread2.IsBackground = true;
            thread2.Start();

            Thread thread3 = new Thread(() => dealer.ResultSendToClient(serializeFunc));
            thread3.IsBackground = true;
            thread3.Start();
        }
        public CooRPCServer ConfigConnection(string ip, int port)
        {
            server = new TcpServer() { ip = ip, port = port };
            return this;
        }
        public CooRPCServer ConfigSerialize(Func<object, byte[]> serializeFunc)
        {
            this.serializeFunc = serializeFunc;
            return this;
        }
        public CooRPCServer ConfigDeserialize(Func<byte[], Type, object> deserializeFunc)
        {
            this.deserializeFunc = deserializeFunc;
            return this;
        }







    }
}
