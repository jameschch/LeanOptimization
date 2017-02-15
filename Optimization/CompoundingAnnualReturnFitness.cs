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

        //Fitness based on total trades
        protected override FitnessResult CalculateFitness(Dictionary<string, string> result)
        {
            this.Name = "Return";
            var fitness = new FitnessResult();

            double parsed = 0;
            var raw = result["Compounding Annual Return"];
            double.TryParse(raw.TrimEnd('%'), out parsed);

            fitness.Value = parsed.ToString();

            fitness.Fitness = parsed * scale;

            return fitness;
        }

        public override double GetValueFromFitness(double? fitness)
        {
            return fitness.Value / scale;
        }

    }
}
