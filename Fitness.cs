using GeneticSharp.Domain.Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using System.Threading;
using System.Reflection;

namespace Optimization
{
    public class Fitness : IFitness
    {

        public double Evaluate(IChromosome chromosome)
        {
            try
            {
                var sharpe = Program.RunAlgorithm(chromosome);
                return (System.Math.Max(sharpe, -10) + 10) / 200;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }


    }
}
