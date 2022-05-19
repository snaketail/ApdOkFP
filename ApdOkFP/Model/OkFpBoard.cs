using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApdOkFP.Model
{
    public class OkFpBoard
    {
        public string devID;
        public string devFwVersion;
        public string devSN;

        public OkFpBoard()
        {
            devFwVersion = "unknown";
            devSN = "No SN";
            devID = "No device";
        }
    }
}
