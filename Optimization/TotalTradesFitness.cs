using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class TotalTradesFitness : OptimizerFitness
    {
        public TotalTradesFitness(IOptimizerConfiguration config) : base(config)
        {
        }

        double scale = 0.0001;

        //Fitness based on total trades
        protected override FitnessResult CalculateFitness(Dictionary<string, string> result)
        {
            this.Name = "Trades";
            var fitness = new FitnessResult();

            int parsed = 0;
            var raw = result["Total Trades"];
            int.TryParse(raw, out parsed);

            fitness.Value = parsed.ToString();

            fitness.Fitness = (double)(System.Math.Max(parsed, 0)) * scale;

            return fitness;
        }

        public override double GetValueFromFitness(double? fitness)
        {
            return fitness.Value / scale;
        }

    }
}
