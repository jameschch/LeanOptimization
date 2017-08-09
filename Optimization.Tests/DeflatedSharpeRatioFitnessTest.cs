using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Tests
{

    [TestFixture]
    public class DeflatedSharpeRatioFitnessTest
    {


        [Test]
        public void CalculateExpectedMaximumTest()
        {
            var unit = new DeflatedSharpeRatioWrapper();
            unit.Initialize(null);
            var actual = unit.CalculateExpectedMaximum();

            Assert.AreEqual(4.2894064662728244, actual, 0.0000001);
        }

        [Test]
        public void CalculateDeflatedSharpeRatioTest()
        {
            var unit = new DeflatedSharpeRatioWrapper();
            unit.Initialize(null);
            var actual = unit.CalculateDeflatedSharpeRatio(0.1132);

            Assert.AreEqual(0.9004, actual, 0.0002);
        }


        private class DeflatedSharpeRatioWrapper : DeflatedSharpeRatio
        {

            public override void Initialize(IOptimizerConfiguration config)
            {
                N = 100;
                V = 0.5;
                T = 1250;
                Skewness = -3;
                Kurtosis = 10;
                SharpeRatio = 2.5;
            }

        }
    }

}
