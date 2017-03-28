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

            var unit = new Dynasty(file.Object, process.Object);

            unit.Optimize();

        }


    }
}