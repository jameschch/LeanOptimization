using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Optimization
{

    public class GeneConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(GeneConfiguration);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var json = JObject.Load(reader);

            var precision = json["precision"]?.Value<int?>();

            GeneConfiguration gene = new GeneConfiguration
            {
                Key = json["key"].Value<string>(),
                MinDecimal = precision > 0 ? json["min"].Value<decimal?>() : null,
                MaxDecimal = precision > 0 ? json["max"].Value<decimal?>() : null,
                MinInt = precision > 0 ? null : json["min"].Value<int?>(),
                MaxInt = precision > 0 ? null: json["max"].Value<int?>(),
                Precision = precision
            };
            if (json["actual"] != null)
            {

                int parsed;
                string raw = json["actual"].Value<string>();
                if (int.TryParse(raw, out parsed))
                {
                    gene.ActualInt = parsed;
                }

                if (!gene.ActualInt.HasValue)
                {
                    decimal decimalParsed;
                    if (decimal.TryParse(raw, out decimalParsed))
                    {
                        gene.ActualDecimal = decimalParsed;
                    }
                }
            }

            return gene;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

    }


}