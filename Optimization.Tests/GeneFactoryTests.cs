using NUnit.Framework;
using Optimization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Tests
{
    [TestFixture()]
    public class GeneFactoryTests
    {

        [SetUp]
        public void Setup()
        { 
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }

        [Test()]
        public void InitializeTest()
        {
            GeneFactory.Initialize(new GeneConfiguration[0]);
            Assert.IsNotNull(GeneFactory.Config);
        }

        [Test()]
        public void RandomBetweenTest()
        {
            var actual = GeneFactory.RandomBetween(0, 1);
            Assert.IsTrue(actual < 2);
        }

        [Test()]
        public void RandomBetweenPrecisionTest()
        {
            var actual = GeneFactory.RandomBetween(1.1m, 1.2m, 1);
            Assert.IsTrue(actual >= 1.1m && actual <= 1.2m);
        }

        [Test()]
        public void GenerateTest()
        {
            var config = new[] { new GeneConfiguration { Key = "slow", ActualInt = 200 }, new GeneConfiguration { Key = "take", Precision = 2, MaxDecimal= 0.06m,
                MinDecimal = 0.04m, ActualDecimal = 0.05m } };

            var actual = GeneFactory.Generate(config.Where(c => c.Key == "slow").Single(), true);
            Assert.AreEqual(200, (int)((KeyValuePair<string, object>)actual.Value).Value);

            actual = GeneFactory.Generate(config.Where(c => c.Key == "take").Single(), false);
            decimal parsed;
            Assert.IsTrue(decimal.TryParse(((KeyValuePair<string, object>)actual.Value).Value.ToString(), out parsed));
            Assert.AreEqual(2, GeneFactory.GetPrecision(parsed));

        }
    }
}