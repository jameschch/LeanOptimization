using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class NFoldCrossSharpeMaximizer : SharpeMaximizer
    {

        private int _folds = 2;
        public override string Name { get; set; } = "NFoldCrossSharpe";


        public NFoldCrossSharpeMaximizer(IOptimizerConfiguration config, IFitnessFilter filter) : base(config, filter)
        {
            var folds = config.Fitness?.Folds ?? 2;
            if (folds > 0)
            {
                _folds = folds;
            }
        }

        protected override Dictionary<string, decimal> GetScore(Dictionary<string, object> list)
        {
            var score = base.GetScore(list);

            var endDate = Config.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            var period = Config.StartDate.Value.Date - endDate;
            var foldSize = Math.Min(Math.Abs(period.Ticks) + 1 / _folds, TimeSpan.FromDays(1).Ticks);

            for (int i = 0; i < _folds - 1; i++)
            {
                var iterationConfig = Clone((OptimizerConfiguration)Config);
                iterationConfig.StartDate = endDate + TimeSpan.FromTicks(1);
                iterationConfig.EndDate = iterationConfig.StartDate.Value.AddTicks(foldSize);
                OptimizerAppDomainManager.RunAlgorithm(list, iterationConfig).Select(s => score[s.Key] += s.Value);
            }

            var average = score.ToDictionary(d => d.Key, d => d.Value / _folds);

            return average;
        }

    }
}
