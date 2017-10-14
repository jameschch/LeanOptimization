using GeneticSharp.Domain.Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;

namespace Optimization
{


    /// <summary>
    /// Default optimizer baheviour using sharpe ratio.
    /// </summary>
    /// <remarks>Default behaviour will nullify fitness for negative return</remarks>
    public class OptimizerFitness : IFitness
    {

        public string Name { get; protected set; }
        public IOptimizerConfiguration Config { get; protected set; }
        public IFitnessFilter Filter { get; set; }
        double scale = 0.02;

        public OptimizerFitness(IOptimizerConfiguration config, IFitnessFilter filter)
        {
            Config = config;
            Filter = filter;
        }

        public virtual double Evaluate(IChromosome chromosome)
        {
            this.Name = "Sharpe";

            try
            {
                string output = "";
                var list = ((Chromosome)chromosome).ToDictionary();

                list.Add("Id", ((Chromosome)chromosome).Id);

                foreach (var item in list)
                {
                    output += item.Key + ": " + item.Value.ToString() + ", ";
                }

                if (Config.StartDate.HasValue && Config.EndDate.HasValue)
                {
                    output += string.Format("Start: {0}, End: {1}, ", Config.StartDate, Config.EndDate);
                }

                var result = OptimizerAppDomainManager.RunAlgorithm(list);

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

        protected virtual FitnessResult CalculateFitness(Dictionary<string, decimal> result)
        {
            var fitness = new FitnessResult();

            var ratio = result["SharpeRatio"];

            if (Filter != null && !Filter.IsSuccess(result, this))
            {
                ratio = -10m;
            }

            fitness.Value = ratio;

            fitness.Fitness = (double)(System.Math.Max(ratio, -10) + 10) * scale;

            return fitness;
        }

        public virtual double GetValueFromFitness(double? fitness)
        {
            return fitness.Value / scale - 10;
        }

        protected class FitnessResult
        {
            public decimal Value { get; set; }
            public double Fitness { get; set; }
        }

    }
}
