using RPCIServices;
using System;

namespace RPCServices
{
    
    public class TestService : ITestService
    {
        public long Add(long a, long b)
        {
            return a + b;
        }
    }
}
