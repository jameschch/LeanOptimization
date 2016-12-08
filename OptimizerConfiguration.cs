using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class OptimizerConfiguration
    {

        public bool EliteEnabled { get; set; } = false;

        public int ElitePercent { get; set; } = 2;

        public bool CrossoverEnabled { get; set; } = true;

        public double CrossoverPercent { get; set; } = 0.6;

        public bool RandomReplaceEnabled { get; set; } = true;

        public double RandomReplacePercent { get; set; } = 0.25;

        public int PopulationSize { get; set; } = 100;

        public int Generations { get; set; } = 1000;

    }
}
