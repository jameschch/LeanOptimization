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
    /// Adaptive fitness that increases period proportional to improvement in sharpe ratio
    /// </summary>
    public class AdaptiveSharpeRatioFitness : OptimizerFitness
    {

        private double _previousFitness = -10;

        public AdaptiveSharpeRatioFitness(IOptimizerConfiguration config, IFitnessFilter filter) : base(config, filter)
        {
        }

        public override double Evaluate(IChromosome chromosome)
        {
            var fitness = EvaluateBase(chromosome);

            //fitness has improved: adapt the period to steepen ascent. Don't adapt on degenerate negative return
            if (_previousFitness > 0 && fitness > _previousFitness && Config.StartDate.HasValue)
            {
                var hours = Config.EndDate.Value.Subtract(Config.StartDate.Value).TotalHours;
                var improvement = fitness / _previousFitness;
                var adding = hours - (hours * improvement);
                //todo: after config is modified, executions in process will still return for previous dates
                Config.StartDate = Config.StartDate.Value.AddHours(adding);

                //restart with longer in sample. History will now be ignored
                //todo: retain history for failure (-10 Sharpe) executions			
                //resample current alpha
                _previousFitness = EvaluateBase(chromosome);
                fitness = _previousFitness;
            }

            if (fitness > _previousFitness)
            {
                _previousFitness = fitness;
            }
            return fitness;
        }

        protected virtual double EvaluateBase(IChromosome chromosome)
        {
            return base.Evaluate(chromosome);
        }


    }
}
