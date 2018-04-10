using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budford.Control
{
    static internal class Logger
    {
        internal static void Log(string logMessage)
        {
            try
            {
                File.AppendAllText("log.txt", "[" + DateTime.Now.ToLongTimeString() + "] " + logMessage + "\r\n");
            }
            catch (Exception)
            {
                // No Code
            }
        }

        internal static void Clear()
        {
            FileManager.SafeDelete("log.txt");
        }
    }
}
