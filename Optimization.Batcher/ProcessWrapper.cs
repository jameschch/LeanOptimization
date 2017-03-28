using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Batcher
{
    public class ProcessWrapper : IProcessWrapper
    {
        private Process instance { get; set; }

        public StreamReader StandardOutput
        {
            get { return instance.StandardOutput; }
        }

        public void Kill()
        {
            instance.Kill();
            instance = null;
        }

        public void Start(ProcessStartInfo info)
        {
            instance = Process.Start(info);
        }
    }
}
