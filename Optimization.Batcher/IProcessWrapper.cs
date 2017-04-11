using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Batcher
{
    public interface IProcessWrapper
    {
        string ReadLine();

        void Start(ProcessStartInfo info);
        void Kill();
    }
}
