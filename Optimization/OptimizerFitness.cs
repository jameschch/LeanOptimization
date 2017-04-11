using GeneticSharp.Domain.Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;

namespace Optimization
{
    public class OptimizerFitness : IFitness
    {

        public string Name { get; protected set; }
        protected IOptimizerConfiguration Config;

        double scale = 0.02;

        public OptimizerFitness(IOptimizerConfiguration config)
        {
            Config = config;
        }

        public virtual double Evaluate(IChromosome chromosome)
        {
            try
            {
                string output = "";
                var list = ((Chromosome)chromosome).ToDictionary();

                foreach (var item in list)
                {
                    output += item.Key + ": " + item.Value.ToString() + ", ";
                }

                var result = AppDomainManager.RunAlgorithm(list);

                var fitness = CalculateFitness(result);

                output += string.Format("{0}: {1}", this.Name, fitness.Value);
                Program.Logger.Info(output);

                return fitness.Fitness;
            }
            catch (Exception ex)
            {
                Program.Logger.Error(ex);
                return 0;
            }
        }

        protected virtual FitnessResult CalculateFitness(Dictionary<string, string> result)
        {
            this.Name = "Sharpe";
            var fitness = new FitnessResult();

            var sharpe = -10m;
            var ratio = result["Sharpe Ratio"];
            Decimal.TryParse(ratio, out sharpe);
            var compound = result["Compounding Annual Return"];
            decimal parsed;
            Decimal.TryParse(compound.Trim('%'), out parsed);

            if (!Config.IncludeNegativeReturn)
            {
                sharpe = System.Math.Max(sharpe <= 0 || parsed < 0 ? -10 : sharpe, -10);
            }
            else
            {
                sharpe = System.Math.Max(sharpe, -10);
            }

            fitness.Value = sharpe.ToString("0.000");

            fitness.Fitness = (double)(System.Math.Max(sharpe, -10) + 10) * scale;

            return fitness;
        }

        public virtual double GetValueFromFitness(double? fitness)
        {
            return fitness.Value / scale - 10;
        }

        protected class FitnessResult
        {
            public string Value { get; set; }
            public double Fitness { get; set; }
        }

    }
}
