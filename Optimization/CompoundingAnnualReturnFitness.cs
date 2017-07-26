using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class CompoundingAnnualReturnFitness : OptimizerFitness
    {
        public CompoundingAnnualReturnFitness(IOptimizerConfiguration config) : base(config)
        {
        }

        double scale = 0.01;

        //Fitness based on Compounding Annual Return
        protected override FitnessResult CalculateFitness(Dictionary<string, decimal> result)
        {
            this.Name = "Return";
            var fitness = new FitnessResult();

            var raw = result["CompoundingAnnualReturn"];

            fitness.Value = raw;

            fitness.Fitness = (double)raw * scale;

            return fitness;
        }

        public override double GetValueFromFitness(double? fitness)
        {
            return fitness.Value / scale;
        }

    }
}
