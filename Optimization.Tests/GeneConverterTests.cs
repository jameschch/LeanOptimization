using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
    public class GeneConverterTests
    {
        [Test()]
        public void ReadWriteJsonTest()
        {
            string expected = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.json"));
            var config = JsonConvert.DeserializeObject<OptimizerConfiguration>(expected);
            expected = expected.Replace("\n", "").Replace(" ", "").Replace("\r", "");

            var actual = JsonConvert.SerializeObject(config, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            Assert.AreEqual(expected, actual);


        }
    }
}