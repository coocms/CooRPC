using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CooRPCCore.Model
{
    [MessagePack.MessagePackObject(true)]
    public class ResponseModel
    {
        
        public object resultList { get; set; }
        public string guid { get; set; }
    }
}
