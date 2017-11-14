using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Randomizations;
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
    public class GeneManagerTests
    {

        [SetUp]
        public void Setup()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
            RandomizationProvider.Current = new BasicRandomization();
        }

        [Test()]
        public void StartTest()
        {
            var config = new Mock<IOptimizerConfiguration>();
            config.Setup(c => c.PopulationSize).Returns(2);
            config.Setup(c => c.Genes).Returns(new[] { new GeneConfiguration { Key = "abc", MinInt = 1, MaxInt = 3 }, new GeneConfiguration{ Key = "def", MinInt = 1, MaxInt = 3 } });

            var fitness = new Mock<OptimizerFitness>(config.Object, null);
            fitness.Setup(f => f.Evaluate(It.IsAny<IChromosome>())).Returns(-10).Verifiable();
            var unit = new GeneManager();
            unit.Initialize(config.Object, fitness.Object);
            unit.Start();
            fitness.Verify();

        }
    }
}