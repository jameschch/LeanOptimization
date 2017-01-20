using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class OptimizerConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        public int PopulationSize { get; set; } = 12;

        /// <summary>
        /// 
        /// </summary>
        public int PopulationSizeMaximum { get; set; } = 24;

        /// <summary>
        /// 
        /// </summary>
        public int Generations { get; set; } = 1000;

        /// <summary>
        /// 
        /// </summary>
        public int StagnationGenerations { get; set; } = 10;

        /// <summary>
        /// 
        /// </summary>
        public int MaxThreads { get; set; } = 8;

        /// <summary>
        /// 
        /// </summary>
        public string AlgorithmTypeName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ConfigPath { get; set; } = "../../../../Lean/Launcher/config.json";

        /// <summary>
        /// 
        /// </summary>
        public bool OnePointCrossover { get; set; } = false;
    
        /// <summary>
        /// 
        /// </summary>
        public string AlgorithmLocation { get; set; }

    }
}
