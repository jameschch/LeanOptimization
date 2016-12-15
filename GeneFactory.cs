using GeneticSharp.Domain.Chromosomes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{

    public class GeneFactory
    {

        private static Random _random = new Random();


        public static GeneConfiguration[] Load()
        {

            var v = new Dictionary<string, object>();

            using (StreamReader file = File.OpenText("optimization.json"))
            {
                var document = (JObject)JsonConvert.DeserializeObject(file.ReadToEnd());

                return JsonConvert.DeserializeObject<GeneConfiguration[]>(document["genes"].ToString());
            }

        }

        public static int RandomBetween(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }

        public static double RandomBetween(decimal minValue, decimal maxValue, int? precision = null)
        {
            if (!precision.HasValue)
            {
                precision = BitConverter.GetBytes(decimal.GetBits(minValue)[3])[2];

                if (precision <= 0)
                {
                    precision = 3;
                }
            }

            var value = _random.NextDouble() * ((double)maxValue - (double)minValue) + (double)minValue;
            return System.Math.Round(value, precision.Value);
        }

        public static Gene Generate(GeneConfiguration config, bool isActual)
        {
            if (isActual && config.ActualInt.HasValue)
            {
                return new Gene(new KeyValuePair<string, object>(config.Key, config.ActualInt));
            }
            else if (isActual && config.ActualDecimal.HasValue)
            {
                return new Gene(new KeyValuePair<string, object>(config.Key, config.ActualDecimal));
            }
            else if (config.MinDecimal.HasValue && config.MaxDecimal.HasValue)
            {
                return new Gene(new KeyValuePair<string, object>(config.Key, GeneFactory.RandomBetween(config.MinDecimal.Value, config.MaxDecimal.Value, config.Precision)));
            }

            return new Gene(new KeyValuePair<string, object>(config.Key, GeneFactory.RandomBetween(config.MinInt.Value, config.MaxInt.Value)));
        }

    }
}
