using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Optimization
{

    public class GeneConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var json = JObject.Load(reader);

            GeneConfiguration gene = new GeneConfiguration
            {
                Key = json["key"].Value<string>(),
                MinDecimal = IsFractional(json, "min") ? json["min"].Value<decimal?>() : null,
                MaxDecimal = IsFractional(json, "max") ? json["max"].Value<decimal?>() : null,
                MinInt = IsFractional(json, "min") ? null : json["min"].Value<int?>(),
                MaxInt = IsFractional(json, "max") ? null: json["max"].Value<int?>(),
                Precision = json["precision"]?.Value<int?>()
            };

            return gene;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private bool IsFractional(JObject json, string name)
        {
            return json[name].Value<string>().Contains(".");
        }


    }


}