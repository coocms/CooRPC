using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CooRPCCore.Model
{
    [MessagePack.MessagePackObject(true)]
    public class RequestModel
    {
        public string assemblyName { get; set; }
        public string methodName { get; set; }
        public List<byte[]> args { get; set; }
        public string guid { get; set; }
    }
}
