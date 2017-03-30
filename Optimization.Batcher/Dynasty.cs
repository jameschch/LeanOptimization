using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Optimization;
using Newtonsoft.Json.Serialization;

namespace Optimization.Batcher
{
    public class Dynasty
    {

        readonly IFileSystem _file;
        readonly IProcessWrapper _process;
        const string configFilename = "optimization.json";

        public Dynasty(IFileSystem file, IProcessWrapper process)
        {
           _file = file;
            _process = process;
        }

        public Dynasty() : this(new FileSystem(), new ProcessWrapper())
        {
        }

        public void Optimize()
        {
            var config = JsonConvert.DeserializeObject<DynastyConfiguration>(_file.File.ReadAllText("DynastyConfiguration.json"));

            OptimizerConfiguration current = null;
            var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

            for (var i = config.StartDate; i <= config.EndDate; i = i.AddDays(config.DurationDays).AddHours(config.DurationHours))
            {

                if (current == null)
                {
                    current = JsonConvert.DeserializeObject<OptimizerConfiguration>(_file.File.ReadAllText(configFilename));
                }

                current.StartDate = i;
                current.EndDate = i.AddDays(config.DurationDays).AddHours(config.DurationHours);

                string json = JsonConvert.SerializeObject(current, settings);

                _file.File.WriteAllText(configFilename, json);

                var info = new ProcessStartInfo("Optimization.exe", configFilename)
                {
                    RedirectStandardOutput = true
                };

                _process.Start(info);
                string output;
                Queue<string> queue = new Queue<string>(2);
                while ((output = _process.StandardOutput.ReadLine()) != null)
                {
                    queue.Enqueue(output);
                }
                if (queue.First() == GeneticManager.Termination)
                {
                    if (config.WalkForward)
                    {
                        var split = queue.Last().Split(',');

                        foreach (var item in split)
                        {
                            string[] pair = item.Split(':');

                            var gene = current.Genes.SingleOrDefault(g => g.Key == pair[0]);

                            decimal parsedDecimal;
                            int parsedInt;
                            if (decimal.TryParse(pair[1], out parsedDecimal))
                            {
                                gene.ActualDecimal = parsedDecimal;
                            }
                            else if (int.TryParse(pair[1], out parsedInt))
                            {
                                gene.ActualInt = parsedInt;
                            }
                            else
                            {
                                throw new Exception($"Unable to parse optimal gene from range {current.StartDate} {current.EndDate}");
                            }

                        } 
                    }
                }
                _process.Kill();
            }
        }

    }
}
