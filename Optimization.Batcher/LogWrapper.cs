using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Batcher
{
    public class LogWrapper : ILogWrapper
    {
        public void Info(string message)
        {
           Optimization.Program.Logger.Info(message);
        }

        public void Result(string message)
        {
            Optimization.Batcher.Program.Logger.Info(message);
        }

    }
}
