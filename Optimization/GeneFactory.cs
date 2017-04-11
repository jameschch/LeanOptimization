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
        public static GeneConfiguration[] Config { get; private set; }

        public static void Initialize(GeneConfiguration[] config)
        {
            Config = config;
        }

        public static int RandomBetween(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }

        public static decimal RandomBetween(decimal minValue, decimal maxValue, int? precision = null)
        {
            if (!precision.HasValue)
            {
                precision = GetPrecision(minValue);
            }

            var value = _random.NextDouble() * ((double)maxValue - (double)minValue) + (double)minValue;
            return (decimal)System.Math.Round(value, precision.Value);
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

        public static int? GetPrecision(decimal value)
        {
            return BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
        }

    }
}
