using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class OptimizerConfiguration
    {

        public int PopulationSize { get; set; } = 24;

        public int Generations { get; set; } = 1000;

        public int StagnationGenerations { get; set; } = 10;

        public int MaxThreads { get; set; } = 8;

    }
}
