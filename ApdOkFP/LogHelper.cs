using System.IO;
using System.Runtime.CompilerServices;

namespace ApdOkFP
{
    public class LogHelper
    {
        public static log4net.ILog GetLogger([CallerFilePath] string filename = "")
        {
            string name = Path.GetFileNameWithoutExtension(filename);
            return log4net.LogManager.GetLogger(name);
        }
    }
}
