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
        public void CanConvertTest()
        {
        }

        [Test()]
        public void ReadJsonTest()
        {
        }

        [Test()]
        public void WriteJsonTest()
        {
            string expected = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.json"));
            var config = JsonConvert.DeserializeObject<OptimizerConfiguration>(expected);

            var actual = JsonConvert.SerializeObject(config, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            expected = expected.Replace("\n", "").Replace(" ", "").Replace("\r", "");


            Assert.AreEqual(expected, actual);


        }
    }
}