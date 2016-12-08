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

        private static Random random = new Random();


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
            return random.Next(minValue, maxValue);
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

            var value = random.NextDouble() * ((double)maxValue - (double)minValue) + (double)minValue;
            return System.Math.Round(value, precision.Value);
        }

        public class Chromosome : ChromosomeBase
        {

            GeneConfiguration[] _config;
            bool _isActual;

            public Chromosome(bool isActual, GeneConfiguration[] config) : base(config.Length)
            {
                _isActual = isActual;
                _config = config;

                for (int i = 0; i < _config.Length; i++)
                {
                    ReplaceGene(i, GenerateGene(i));
                }
            }

            public override Gene GenerateGene(int geneIndex)
            {

                var item = _config[geneIndex];

                if (_isActual && item.ActualInt.HasValue)
                {
                    return new Gene(new KeyValuePair<string, object>(item.Key, item.ActualInt));
                }
                else if (_isActual && item.ActualDecimal.HasValue)
                {
                    return new Gene(new KeyValuePair<string, object>(item.Key, item.ActualDecimal));
                }
                else if (item.MinDecimal.HasValue && item.MaxDecimal.HasValue)
                {
                    return new Gene(new KeyValuePair<string, object>(item.Key, RandomBetween(item.MinDecimal.Value, item.MaxDecimal.Value, item.Precision)));
                }

                return new Gene(new KeyValuePair<string, object>(item.Key, RandomBetween(item.MinInt.Value, item.MaxInt.Value)));
            }

            public override IChromosome CreateNew()
            {
                var config = Load();
                return new Chromosome(false, config);
            }

            public override IChromosome Clone()
            {
                var clone = base.Clone() as Chromosome;

                return clone;
            }
        }


    }
}
