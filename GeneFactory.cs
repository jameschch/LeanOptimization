using GAF;
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

        public static Chromosome Spawn()
        {
            var chromosome = new Chromosome();
            var list = new Dictionary<string, object>();

            foreach (var item in Load())
            {
                if (item.MinDecimal.HasValue && item.MaxDecimal.HasValue)
                {
                    list.Add(item.Key, RandomBetween(item.MinDecimal.Value, item.MaxDecimal.Value));
                }
                else
                {
                    list.Add(item.Key, RandomBetween(item.MinInt.Value, item.MaxInt.Value));
                }

            }

            foreach(var item in list)
            {
                chromosome.Genes.Add(new Gene(item));
            }

            return chromosome;
        }

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
            var rnd = GAF.Threading.RandomProvider.GetThreadRandom();
            return rnd.Next(minValue, maxValue);
        }

        public static double RandomBetween(decimal minValue, decimal maxValue, int? rounding = null)
        {
            if (!rounding.HasValue)
            {
                rounding = BitConverter.GetBytes(decimal.GetBits(minValue)[3])[2];

                if (rounding <= 0)
                {
                    rounding = 3;
                }
            }

            var rnd = GAF.Threading.RandomProvider.GetThreadRandom();
            var value = rnd.NextDouble() * ((double)maxValue - (double)minValue) + (double)minValue;
            return System.Math.Round(value, rounding.Value);
        }


    }
}
