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
        private Process _instance { get; set; }

        public string ReadLine()
        {
             return _instance?.StandardOutput.ReadLine();
        }

        public void Kill()
        {
            try
            {
                _instance.Kill();
                _instance = null;
            }
            catch (Exception ex)
            {
                //todo: log
            }
        }

        public void Start(ProcessStartInfo info)
        {
            _instance = Process.Start(info);
        }
    }
}
