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
        const string configFilename = "optimization_dynasty.json";

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
            var config = JsonConvert.DeserializeObject<DynastyConfiguration>(_file.File.ReadAllText("dynasty.json"));

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
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

                _process.Start(info);
                string output;
                FixedSizedQueue<string> queue = new FixedSizedQueue<string>(1);
                while ((output = _process.ReadLine()) != null)
                {
                    if (output == null)
                    {
                        break;
                    }
                    //Console.WriteLine(output);
                    queue.Enqueue(output);

                    if (queue.First() == GeneticManager.Termination)
                    {
                        Console.WriteLine($"{current.StartDate} {current.EndDate}");
                        Console.WriteLine(queue.First());
                        Console.WriteLine(queue.Last());

                        if (config.WalkForward)
                        {
                            var split = queue.Last().Split(',');

                            for (int ii = 0; ii < split.Length - 1; ii++)
                            {
                                string[] pair = split[ii].Split(':');
                                var gene = current.Genes.SingleOrDefault(g => g.Key == pair[0].Trim());

                                decimal parsedDecimal;
                                int parsedInt;
                                if (int.TryParse(pair[1].Trim(), out parsedInt))
                                {
                                    gene.ActualInt = parsedInt;
                                }
                                else if (decimal.TryParse(pair[1].Trim(), out parsedDecimal))
                                {
                                    gene.ActualDecimal = parsedDecimal;
                                }
                                else
                                {
                                    throw new Exception($"Unable to parse optimal gene from range {current.StartDate} {current.EndDate}");
                                }
                            }
                        }
                        _process.Kill();
                    }
                }
            }
        }

        private class FixedSizedQueue<T> : Queue<T>
        {

            public FixedSizedQueue(int limit)
            {
                _limit = limit;
            }

            private int _limit { get; set; }
            public new void Enqueue(T obj)
            {
                while (this.Count > _limit)
                {
                    this.Dequeue();
                }
                base.Enqueue(obj);
            }
        }

    }
}
