using GeneticSharp.Domain.Chromosomes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class DualPeriodSharpeFitness : OptimizerFitness
    {

        public DualPeriodSharpeFitness(IOptimizerConfiguration config, IFitnessFilter filter) : base(config, filter)
        {
        }

        public override double Evaluate(IChromosome chromosome)
        {
            this.Name = "DualPeriodSharpe";

            var dualConfig = Clone<OptimizerConfiguration>((OptimizerConfiguration)Config);
            var start = Config.StartDate.Value;
            var end = Config.EndDate.Value;
            var diff = end - start;

            dualConfig.StartDate = end;
            dualConfig.EndDate = end + diff;

            var dualFitness = new OptimizerFitness(dualConfig, this.Filter);

            var first = base.Evaluate(chromosome);
            var second = dualFitness.Evaluate(chromosome);

            var fitness = new FitnessResult
            {
                Fitness = (first + second) / 2
            };
            fitness.Value = (decimal)base.GetValueFromFitness(fitness.Fitness);

            var output = string.Format("Start: {0}, End: {1}, Start: {2}, End: {3}, Dual Period {4}: {5}", start, end, Config.StartDate, Config.EndDate, this.Name, fitness.Value);
            Program.Logger.Info(output);

            Config.StartDate = start;
            Config.EndDate = end;

            return fitness.Fitness;
        }

        public static T Clone<T>(T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

    }
}
