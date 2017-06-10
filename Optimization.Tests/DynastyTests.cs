using Moq;
using NUnit.Framework;
using Optimization.Batcher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
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
            var file = new Mock<IFileSystem>();
            var log = new Mock<ILogWrapper>();
            var q = new Queue<string>();

            file.Setup(f => f.File.ReadAllText(It.IsAny<string>())).Returns((string path) =>
            {
                return System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path.StartsWith("optimization") ? "test.json" : path));
            });

            file.Setup(f => f.File.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Callback((string path, string contents) =>
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path), contents);
            });

            List<string> actual = new List<string>();

            log.Setup(l => l.Result(It.IsAny<string>())).Callback<string>(m => { actual.Add(m); });
            //log.Setup(l => l.Info(It.IsAny<string>())).Callback<string>(m => { actual.Add(m); });


            OptimizerInitializerTests.SetEntryAssembly(Assembly.GetCallingAssembly());

            var unit = new Dynasty(file.Object, log.Object, new MockGeneManagerFactory());

            unit.Optimize();

            Assert.IsTrue(actual.First().StartsWith("For period"));
            Assert.IsTrue(actual.ElementAt(1).StartsWith(GeneManager.Termination));
            Assert.IsTrue(actual.ElementAt(2).StartsWith("Algorithm:"));
            Assert.IsTrue(actual.ElementAt(3).StartsWith("take:"));
        }

    }
}