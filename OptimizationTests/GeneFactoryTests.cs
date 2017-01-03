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
    public class GeneFactoryTests
    {


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

            var actual = GeneFactory.Generate(config.First(), false);
            Assert.AreEqual(2, GeneFactory.GetPrecision((decimal)((KeyValuePair<string, object>)actual.Value).Value));

            actual = GeneFactory.Generate(config.Last(), false);
            int parsed;
            Assert.IsTrue(int.TryParse(((KeyValuePair<string, object>)actual.Value).Value.ToString(), out parsed));

            actual = GeneFactory.Generate(config.Last(), true);
            Assert.AreEqual((int)((KeyValuePair<string, object>)actual.Value).Value, 2);

        }
    }
}