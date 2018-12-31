using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class NestedCrossSharpeMaximizer : SharpeMaximizer
    {

        public override string Name { get; set; } = "NestedCrossSharpe";


        public NestedCrossSharpeMaximizer(IOptimizerConfiguration config, IFitnessFilter filter) : base(config, filter)
        {
        }

        //todo: configurable k-folds
        protected override Dictionary<string, decimal> GetScore(Dictionary<string, object> list)
        {
            //todo: start date is rounded down
            var score = base.GetScore(list);

            var period = Config.StartDate.Value - Config.EndDate.Value.Date.AddDays(1).AddMilliseconds(-1);
            var oneThird = period.Ticks / 3;

            var oneThirdConfig = Clone((OptimizerConfiguration)Config);
            oneThirdConfig.EndDate = Config.StartDate.Value.AddTicks(oneThird);
            OptimizerAppDomainManager.RunAlgorithm(list, oneThirdConfig).Select(s => score[s.Key] += s.Value);

            var twoThirdsConfig = Clone((OptimizerConfiguration)Config);
            twoThirdsConfig.EndDate = Config.StartDate.Value.AddTicks(oneThird * 2);
            OptimizerAppDomainManager.RunAlgorithm(list, oneThirdConfig).Select(s => score[s.Key] += s.Value);
            return score.ToDictionary(d => d.Key, d => d.Value / 3);

        }

    }
}
