using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApdOkFP.Model
{
    public class OkFpFrontPanel
    {
        public uint tQuench;
        public uint tWait;
        public uint tReset;
        public uint numberOfPulse;

        public OkFpFrontPanel()
        {
            tQuench = 5;
            tWait = 5;
            tReset = 5;
            numberOfPulse = 0;
        }
    }
}
