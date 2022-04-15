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

        public string StringAdd(string a, string b)
        {
            return a + b;
        }

        public TestRes StringAddPro(string a, string b)
        {
            return new TestRes() { res = a + b, test = "Test" };
        }

        public TestTypeA StringAddPro2(TestTypeA a, TestTypeA b)
        {
            return new TestTypeA() { str = a.str + b.str };
        }
    }



}
