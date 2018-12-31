namespace Optimization
{
    public class NFoldCrossReturnMaximizer : SharpeMaximizer
    {

        private int _folds = 2;
        public override string Name { get; set; } = "NFoldCrossReturn";
        public override string ScoreKey { get; set; } = "CompoundingAnnualReturn";

        public NFoldCrossReturnMaximizer(IOptimizerConfiguration config, IFitnessFilter filter) : base(config, filter)
        {
        }

    }
}
