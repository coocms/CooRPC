using CooRPCCore.Interface;
using System;

namespace RPCIServices
{
    public interface ITestService : ICoocRPCService<ITestService>
    {
        public long Add(long a, long b);
        public string StringAdd(string a, string b);

        public TestRes StringAddPro(string a, string b);
    }
    [MessagePack.MessagePackObject(true)]
    public class TestRes
    {
        public string res { get; set; }
        public string test { get; set; }
    }
}
