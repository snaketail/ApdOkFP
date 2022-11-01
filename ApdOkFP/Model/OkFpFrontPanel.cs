using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApdOkFP.Model
{
    public class OkFpFrontPanel
    {
        public UInt32 tQuench;
        public UInt32 tWait;
        public UInt32 tReset;
        public UInt32 numberOfPulse;

        public OkFpFrontPanel()
        {
            tQuench = 5;
            tWait = 5;
            tReset = 5;
            numberOfPulse = 0;
        }
    }
}
