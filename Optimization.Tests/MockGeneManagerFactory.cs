using Moq;
using Optimization.Batcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Tests
{
    class MockGeneManagerFactory : IGeneManagerFactory
    {

        public IGeneManager Create()
        {

            Dynasty.LogOutput("utyvoiuhpoih[j[09u875");
            Dynasty.LogOutput(GeneManager.Termination);
            Dynasty.LogOutput("Algorithm: Name, Generation: 987, Fitness: 100, sharpe: 1.23");
            Dynasty.LogOutput("take: 1.1, fast: 12, slow: 123.456");
            Dynasty.LogOutput(null);

            return Mock.Of<IGeneManager>();
        }
    }
}
