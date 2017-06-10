using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.CompilerServices;
using NLog;

namespace Optimization.Batcher
{
    class Program
    {

        internal static Logger Logger = LogManager.GetLogger("batcher");
        static Dynasty batcher;

        static void Main(string[] args)
        {
            try
            {
                batcher = new Dynasty();
                batcher.Optimize();
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

    }
}
