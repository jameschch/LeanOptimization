using Moq;
using NUnit.Framework;
using Optimization.Batcher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Tests
{
    [TestFixture()]
    public class DynastyTests
    {

        [Test()]
        public void OptimizeTest()
        {
            var process = new Mock<IProcessWrapper>();
            var file = new Mock<IFileSystem>();
            var log = new Mock<ILogWrapper>();
            var q = new Queue<string>();

            file.Setup(f => f.File.ReadAllText(It.IsAny<string>())).Returns((string path) =>
            {
                return System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
            });
            file.Setup(f => f.File.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Callback((string path, string contents) =>
            {
                q.Enqueue("utyvoiuhpoih[j[09u875");
                q.Enqueue(GeneManager.Termination);
                q.Enqueue("Algorithm: Name, Generation: 987, Fitness: 100, sharpe: 1.23");
                q.Enqueue("take: 1.1, fast: 12, slow: 123.456");
                q.Enqueue(null);

                System.IO.File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path), contents);
            });

            process.Setup(p => p.Start(It.IsAny<ProcessStartInfo>()));
            process.Setup(p => p.Kill()).Verifiable();
            process.Setup(p => p.ReadLine()).Returns(() => { return q.Dequeue(); });

            List<string> actual = new List<string>();

            log.Setup(l => l.Info(It.IsAny<string>())).Callback<string>(m => { actual.Add(m); });

            var unit = new Dynasty(file.Object, process.Object, log.Object);

            unit.Optimize();

            process.Verify();

            Assert.IsTrue(actual.First().StartsWith("For period"));
            Assert.IsTrue(actual.ElementAt(1).StartsWith(GeneManager.Termination));
            Assert.IsTrue(actual.ElementAt(2).StartsWith("Algorithm:"));
            Assert.IsTrue(actual.ElementAt(3).StartsWith("take:"));
        }

    }
}