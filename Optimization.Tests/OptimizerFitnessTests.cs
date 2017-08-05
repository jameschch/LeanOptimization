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
    public class OptimizerFitnessTests
    {

        Wrapper unit;

        public OptimizerFitnessTests()
        {
            unit = new Wrapper(new OptimizerConfiguration
            {
                FitnessTypeName = "Optimization.OptimizerFitness",
            });
        }

        [TestCase(1, 12, 0.22)]
        [TestCase(-1, 12, 0)]
        [TestCase(-1, 0, 0)]
        public void CalculateFitnessTest(decimal car, int trades, double expected)
        {
            var actual = unit.CalculateFitnessWrapper(new Dictionary<string, decimal> {
                { "SharpeRatio", 1 },
                { "CompoundingAnnualReturn", car },
                { "TotalNumberOfTrades", trades }
            });
            Assert.AreEqual(expected, actual.Item2);
        }

        private class Wrapper : OptimizerFitness
        {
            public Wrapper(IOptimizerConfiguration config) : base(config)
            {
            }

            public Tuple<decimal, double> CalculateFitnessWrapper(Dictionary<string, decimal> result)
            {
                var fitness = base.CalculateFitness(result);
                return new Tuple<decimal, double>(fitness.Value, fitness.Fitness);
            }
        }
    }
}