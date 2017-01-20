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
        public void LoadTest()
        {
            var actual = GeneFactory.Load();
            Assert.IsNotNull(actual);
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
            var config = GeneFactory.Load();

            var actual = GeneFactory.Generate(config.Where(c => c.Key == "slow").Single(), true);
            Assert.AreEqual(200, (int)((KeyValuePair<string, object>)actual.Value).Value);

            actual = GeneFactory.Generate(config.Where(c => c.Key == "take").Single(), false);
            decimal parsed;
            Assert.IsTrue(decimal.TryParse(((KeyValuePair<string, object>)actual.Value).Value.ToString(), out parsed));
            Assert.AreEqual(2, GeneFactory.GetPrecision(parsed));

        }
    }
}