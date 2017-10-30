using GeneticSharp.Domain.Chromosomes;
using Moq;
using NUnit.Framework;
using Optimization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Tests
{
    [TestFixture()]
    public class AdaptiveSharpeRatioFitnessTests
    {


        [Test()]
        public void EvaluateTest()
        {
            var config = new OptimizerConfiguration
            {
                StartDate = new DateTime(2001, 1, 2),
                EndDate = new DateTime(2001, 1, 4)
            };

            var originalHours = CurrentHours(config);

            var unit = new Wrapper(config, Mock.Of<IFitnessFilter>());

            //will not adapt on first result
            unit.Evaluate(Mock.Of<IChromosome>());
            Assert.AreEqual(originalHours, CurrentHours(config));

            var fitness = unit.Evaluate(Mock.Of<IChromosome>());

            var actualHours = CurrentHours(config);

            //sharpe improved by 50%, period window should increase by 24 hours
            Assert.AreEqual(72, actualHours);

            fitness = unit.Evaluate(Mock.Of<IChromosome>());
            actualHours = CurrentHours(config);

            //50% again
            Assert.AreEqual(108, actualHours);
        }

        private double CurrentHours(OptimizerConfiguration config)
        {
            return config.EndDate.Value.Subtract(config.StartDate.Value).TotalHours;
        }

        private class Wrapper : AdaptiveSharpeRatioFitness
        {

            double _previous = 0.5;

            public Wrapper(IOptimizerConfiguration config, IFitnessFilter filter) : base(config, filter)
            {
            }

            protected override double EvaluateBase(IChromosome chromosome)
            {
                _previous += _previous * 0.5;
                return _previous;
            }
        }

    }
}