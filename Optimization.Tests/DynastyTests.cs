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

            file.Setup(f => f.File.ReadAllText(It.IsAny<string>())).Returns((string path) => { return System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path)); });
            file.Setup(f => f.File.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Callback((string path, string contents) => { System.IO.File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path), contents); });

            process.Setup(p => p.Start(It.IsAny<ProcessStartInfo>()));
            process.Setup(p => p.Kill()).Verifiable();

            var q = new Queue<string>();
            q.Enqueue("utyvoiuhpoih[j[09u875");
            q.Enqueue(GeneticManager.Termination);
            q.Enqueue("take: 1.1, fast: 12, slow: 123.456, sharpe: 1.23");

            process.Setup(p => p.ReadLine()).Returns(() => { return q.Dequeue(); });

            var unit = new Dynasty(file.Object, process.Object);

            unit.Optimize();

            process.Verify();
        }


    }
}