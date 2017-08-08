using NUnit.Framework;
using Optimization;
using QuantConnect.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Tests
{
    [TestFixture()]
    public class StatisticsAdapterTests
    {

        [Test()]
        public void TransformTest()
        {
            var actual = StatisticsAdapter.Transform(new AlgorithmPerformance
            {
                PortfolioStatistics = new PortfolioStatistics
                {
                    Alpha = 1.23m
                },
                TradeStatistics = new TradeStatistics
                {
                    TotalNumberOfTrades = 45
                }
            });

            Assert.AreEqual(1.23m, actual["Alpha"]);
            Assert.AreEqual(45m, actual["TotalNumberOfTrades"]);
        }

    }
}