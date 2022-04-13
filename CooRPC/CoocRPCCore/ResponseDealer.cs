using CooRPCCore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CooRPCCore
{
    public class ResponseDealer
    {
        System.Collections.Concurrent.ConcurrentQueue<ResponseModel> responseModels = new System.Collections.Concurrent.ConcurrentQueue<ResponseModel>();

        List<ResponseModel> responseModelsList = new List<ResponseModel>();
        readonly object responseListLock = new object();
        public void ResponseSplit(TcpClient client, Func<byte[], Type, object> deserilizeFunc)
        {
            List<byte> temp = new List<byte>();
            System.Collections.Concurrent.ConcurrentQueue<byte[]> tcpStringQueue = client.responseMessage;
            while (true)
            {
                bool bFirstNotParse = false;
                if (tcpStringQueue.TryDequeue(out byte[] tbyte))
                {
                    List<byte[]> response = ByteSplit(tbyte, ((byte)'|'));
                    //List<string> response = tbyte.Split("|").ToList();
                    if (temp.Count > 0)
                    {
                        temp.AddRange(response.First());
                        var responseModel = deserilizeFunc(temp.ToArray(), typeof(ResponseModel)) as ResponseModel;
                        responseModels.Enqueue(responseModel);


                        temp.Clear();
                        bFirstNotParse = true;
                    }

                    byte[] lastMessage = response.Last();
                    if (lastMessage.Length > 0)
                    {
                        //最后一组数据有值
                        temp = lastMessage.ToList();
                    }

                    for (int i = bFirstNotParse ? 1 : 0; i < response.Count - 1; i++)
                    {
                        ResponseModel responseModel = null;

                        
                        responseModel = deserilizeFunc(response[i], typeof(ResponseModel)) as ResponseModel;
                        if (responseModel == null)
                            continue;
                        responseModels.Enqueue(responseModel);
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
                        temp.Clear();
                    }
                }
                else
                {
                    temp.Add(bytes[i]);
                }
            }
            return res;
        }
        public void DequeueResToList()
        {
            while (true)
            {
                if (responseModels.TryDequeue(out ResponseModel responseModel))
                {
                    lock(responseListLock)
                        responseModelsList.Add(responseModel);
                }
                Thread.Sleep(10);
            }
        }

        public async Task<object> GetResult(string guid)
        {
            return await Task.Run(() =>
            {
                while (true)
                {
                    ResponseModel temp;
                    lock (responseListLock)
                        temp = responseModelsList.FirstOrDefault(o => o.guid == guid);

                    if (temp != null)
                    {
                        lock (responseListLock)
                        {
                            responseModelsList = responseModelsList.Where(o => o.guid != guid).ToList();
                        }

                        return temp.resultList;
                    }


                    Thread.Sleep(10);
                }
            });

        }

    }
}
