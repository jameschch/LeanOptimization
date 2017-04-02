using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace Optimization.Batcher
{
    class Program
    {

        static void Main(string[] args)
        {

            var batcher = new Dynasty();
            batcher.Optimize();
            Console.ReadLine();
        }
    }
}
