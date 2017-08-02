using GeneticSharp.Domain.Chromosomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class DualPeriodSharpeFitness : OptimizerFitness
    {

        public DualPeriodSharpeFitness(IOptimizerConfiguration config) : base(config)
        {
        }

        public override double Evaluate(IChromosome chromosome)
        {
            this.Name = "DualPeriodSharpe";

            var list = ((Chromosome)chromosome).ToDictionary();
            var start = Config.StartDate.Value;
            var end = Config.EndDate.Value;

            var first = base.Evaluate(chromosome);

            var diff = end - start;

            Config.StartDate = end;
            Config.EndDate = end + diff;

            OptimizerAppDomainManager.ReInitialize(Config);
            var second = base.Evaluate(chromosome);

            var fitness = new FitnessResult
            {
                Fitness = (first + second) / 2
            };
            fitness.Value = (decimal)base.GetValueFromFitness(fitness.Fitness);

            var output = string.Format("Start: {0}, End: {1}, Start: {2}, End: {3}, {4}: {5}", start, end, Config.StartDate, Config.EndDate, this.Name, fitness.Value);
            Program.Logger.Info(output);

            Config.StartDate = start;
            Config.EndDate = end;

            return fitness.Fitness;
        }

    }
}
