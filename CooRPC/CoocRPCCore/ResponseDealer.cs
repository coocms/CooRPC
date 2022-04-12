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
        public void ResponseSplit(TcpClient client)
        {
            string temp = "";
            System.Collections.Concurrent.ConcurrentQueue<string> tcpStringQueue = client.responseMessage;
            while (true)
            {
                bool bFirstNotParse = false;
                if (tcpStringQueue.TryDequeue(out string tcpString))
                {
                    List<string> response = tcpString.Split("|").ToList();
                    if (!string.IsNullOrEmpty(temp))
                    {
                        temp += response.First();
                        var responseModel = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseModel>(temp);
                        responseModels.Enqueue(responseModel);


                        temp = "";
                        bFirstNotParse = true;
                    }

                    string lastMessage = response.Last();
                    if (!string.IsNullOrEmpty(lastMessage))
                    {
                        //最后一组数据有值
                        temp = lastMessage;
                    }

                    for (int i = bFirstNotParse ? 1 : 0; i < response.Count - 1; i++)
                    {
                        ResponseModel responseModel = null;

                        responseModel = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseModel>(response[i].Trim());
                        if (responseModel == null)
                            continue;
                        responseModels.Enqueue(responseModel);
                    }

                }
                Thread.Sleep(10);
            }
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
