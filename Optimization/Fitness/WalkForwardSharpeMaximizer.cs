using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class WalkForwardSharpeMaximizer : SharpeMaximizer
    {

        private int _folds = 2;
        public override string Name { get; set; } = "WalkForwardSharpe";

        public WalkForwardSharpeMaximizer(IOptimizerConfiguration config, IFitnessFilter filter) : base(config, filter)
        {
            var folds = config.Fitness?.Folds ?? 2;
            if (folds > 0)
            {
                _folds = folds;
            }
        }

        public override Dictionary<string, decimal> GetScore(Dictionary<string, object> list, IOptimizerConfiguration config)
        {
            var firstConfig = Clone((OptimizerConfiguration)Config);

            firstConfig.EndDate = firstConfig.EndDate.Value.Date.AddDays(1);
            firstConfig.StartDate = firstConfig.StartDate.Value.Date;
            var period = (int)(firstConfig.EndDate.Value - firstConfig.StartDate.Value).TotalDays;
            var minimumPeriod = TimeSpan.FromDays(1);

            var foldSize = DeriveLength(period, config.Fitness.Folds);

            firstConfig.EndDate = firstConfig.StartDate.Value.AddDays(foldSize);

            var score = base.GetScore(list, firstConfig);
            //early stopping
            if (CalculateFitness(score).Value == ErrorRatio)
            {
                return score;
            }

            var previousConfig = firstConfig;

            for (int i = 0; i < _folds - 1; i++)
            {
                var iterationConfig = Clone(previousConfig);
                //iterationConfig.StartDate = iterationConfig.StartDate.Value.AddTicks(1).AddTicks(foldSize / 2);
                //iterationConfig.EndDate = iterationConfig.StartDate.Value.AddTicks(foldSize);

                var foldScore = base.GetScore(list, iterationConfig);
                //early stopping
                if (CalculateFitness(foldScore).Value == ErrorRatio)
                {
                    return foldScore;
                }

                score = foldScore.ToDictionary(k => k.Key, v => score[v.Key] += v.Value);

                previousConfig = iterationConfig;
            }

            var average = score.ToDictionary(d => d.Key, d => d.Value / _folds);

            return average;
        }

        private int DeriveLength(int days, int folds)
        {

            var foldSize = (days / folds);


        }

    }
}
