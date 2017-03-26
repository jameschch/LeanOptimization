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
    public class ConfiguredFitnessTests
    {

        Wrapper unit;

        public ConfiguredFitnessTests()
        {
            unit = new Wrapper(new OptimizerConfiguration
            {
                FitnessTypeName = "Optimization.ConfiguredFitness",
                Fitness = new FitnessConfiguration
                {
                    Scale = 0.1,
                    Modifier = -1,
                    Name = "TestName",
                    ResultKey = "TestResultKey"
                }
            });
        }

        [Test()]
        public void CalculateFitnessTest()
        {
            var actual = unit.CalculateFitnessWrapper(new Dictionary<string, string> { { "TestResultKey", "10%"  } });

            Assert.AreEqual(-1d, actual.Item2);
        }

        [Test()]
        public void GetValueFromFitnessTest()
        {
            var actual = unit.GetValueFromFitness(-1d);
            Assert.AreEqual(10, actual);
        }

        private class Wrapper : ConfiguredFitness
        {
            public Wrapper(IOptimizerConfiguration config) : base(config)
            {
            }

            public Tuple<string, double> CalculateFitnessWrapper(Dictionary<string, string> result)
            {
                var fitness = base.CalculateFitness(result);
                return new Tuple<string, double>(fitness.Value, fitness.Fitness);
            }
        }

    }
}