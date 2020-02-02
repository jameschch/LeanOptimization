using GeneticSharp.Domain.Chromosomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class WalkForwardSharpeMaximizer : SharpeMaximizer
    {

        public override string Name { get; set; } = "WalkForwardSharpe";
        private WalkForwardPeriodCalculator _calculator = new WalkForwardPeriodCalculator();

        public WalkForwardSharpeMaximizer(IOptimizerConfiguration config, IFitnessFilter filter) : base(config, filter)
        {
            if (config.Fitness == null)
            {
                config.Fitness = new FitnessConfiguration { OptimizerTypeName = Enums.OptimizerTypeOptions.RandomSearch.ToString() };
            }
            config.Fitness.Folds = config.Fitness?.Folds ?? 2;
        }

        public override double Evaluate(IChromosome chromosome)
        {
            var AllScores = new List<FitnessResult>();

            foreach (var item in _calculator.Calculate(Config))
            {
                var inSampleConfig = Clone((OptimizerConfiguration)Config);
                inSampleConfig.StartDate = item.Value[0];
                inSampleConfig.EndDate = item.Value[1];
                var inSampleOptimizer = new SharpeMaximizer(inSampleConfig, Filter);
                inSampleOptimizer.Evaluate(chromosome);
                Best = inSampleOptimizer.Best;
                chromosome = Best;

                //keep new actual in config
                foreach (var actual in ((Chromosome)Best).ToDictionary())
                {
                    var bestActual  = Config.Genes.Single(g => g.Key == actual.Key);
                    if ((bestActual.Precision ?? 0) > 0)
                    {
                        bestActual.ActualDecimal = (decimal)actual.Value;
                    }
                    else
                    {
                        bestActual.ActualInt = (int)actual.Value;
                    }
                }

                var list = ((Chromosome)Best).ToDictionary();
                var id = Guid.NewGuid().ToString("N");
                list.Add("Id", id);

                var outSampleConfig = Clone((OptimizerConfiguration)Config);
                outSampleConfig.StartDate = item.Value[2];
                outSampleConfig.EndDate = item.Value[3];
                var score = base.GetScore(list, outSampleConfig);

                var fitness = CalculateFitness(score);

                AllScores.Add(fitness);

                var output = new StringBuilder();
                output.Append("Id: " + id + ", ");
                output.Append(((Chromosome)Best).ToKeyValueString());
                output.Append(", ");
                output.AppendFormat("Start: {0}, End: {1}, ", outSampleConfig.StartDate, outSampleConfig.EndDate);
                output.AppendFormat("{0}: {1}", Name, fitness.Value.ToString("0.##"));
                LogProvider.GenerationsLogger.Info(output);
            }

            return (double)AllScores.Average(a => a.Value);
        }


    }
}
