using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{

    [JsonConverter(typeof(GeneConverter))]
    public class GeneConfiguration
    {

        public string Key { get; set; }

        public double? MinDouble { get; set; }

        public double? MaxDouble { get; set; }

        public int? MinInt { get; set; }

        public int? MaxInt { get; set; }

    }
}
