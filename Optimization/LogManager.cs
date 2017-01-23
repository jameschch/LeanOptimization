using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class LogManager : ILogManager
    {

        public void Output(string line, params object[] format)
        {
            Program.Output(line, format);
        }

        public void Output(string line)
        {
            Program.Output(line);
        }

    }
}
