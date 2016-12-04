using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class OptimizerConfiguration
    {

        public bool EliteEnabled { get; set; } = true;

        public int ElitePercent { get; set; } = 5;

        public bool CrossoverEnabled { get; set; } = true;

        public double CrossoverPercent { get; set; } = 1;

        public bool RandomReplaceEnabled { get; set; } = true;

        public double RandomReplacePercent { get; set; } = 0.5;

        public int PopulationSize { get; set; } = 12;

        public int Generations { get; set; } = 1000;

    }
}
