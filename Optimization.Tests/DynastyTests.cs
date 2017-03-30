using Moq;
using NUnit.Framework;
using Optimization.Batcher;
using System;
using System.Collections.Generic;
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

            var unit = new Dynasty(file.Object, process.Object);

            unit.Optimize();

        }


    }
}