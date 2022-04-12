using CooRPCCore.Interface;
using System;

namespace RPCIServices
{
    public interface ITestService : ICoocRPCService<ITestService>
    {
        public long Add(long a, long b);
    }
}
