using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class OptimizerConfiguration
    {

        public bool CrossoverEnabled { get; set; } = true;

        public double CrossoverPercent { get; set; } = 0.6;

        public int PopulationSize { get; set; } = 24;

        public int Generations { get; set; } = 1000;

    }
}
