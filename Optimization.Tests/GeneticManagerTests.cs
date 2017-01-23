using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
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
    public class GeneticManagerTests
    {

        [SetUp]
        public void Setup()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }

        [Test()]
        public void StartTest()
        {
            var config = new Mock<IOptimizerConfiguration>();
            config.Setup(c => c.PopulationSize).Returns(2);
            var fitness = new Mock<IFitness>();
            fitness.Setup(f => f.Evaluate(It.IsAny<IChromosome>())).Returns(-10).Verifiable();
            var unit = new GeneticManager(config.Object, fitness.Object, Mock.Of<ILogManager>());

            unit.Start();
            fitness.Verify();

        }
    }
}