using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{

    //todo: add actual starting value
    [JsonConverter(typeof(GeneConverter))]
    public class GeneConfiguration
    {

        public string Key { get; set; }

        public decimal? MinDecimal { get; set; }

        public decimal? MaxDecimal { get; set; }

        public int? MinInt { get; set; }

        public int? MaxInt { get; set; }

        public int? Precision { get; set; }

    }
}
