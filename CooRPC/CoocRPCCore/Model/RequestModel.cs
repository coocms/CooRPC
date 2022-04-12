using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CooRPCCore.Model
{
    public class RequestModel
    {
        public string assemblyName { get; set; }
        public string methodName { get; set; }
        public List<dynamic> args { get; set; }
        public string guid { get; set; }
    }
}
