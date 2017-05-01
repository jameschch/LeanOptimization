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
    public class Dynasty : IDisposable
    {

        private readonly IFileSystem _file;
        private readonly IProcessWrapper _process;
        private readonly ILogWrapper _logWrapper;
        const string _configFilename = "optimization_dynasty.json";

        public Dynasty(IFileSystem file, IProcessWrapper process, ILogWrapper logWrapper)
        {
            _file = file;
            _process = process;
            _logWrapper = logWrapper;
        }

        public Dynasty() : this(new FileSystem(), new ProcessWrapper(), new LogWrapper())
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
                    current = JsonConvert.DeserializeObject<OptimizerConfiguration>(_file.File.ReadAllText(_configFilename));
                }

                current.StartDate = i;
                current.EndDate = i.AddDays(config.DurationDays).AddHours(config.DurationHours);

                string json = JsonConvert.SerializeObject(current, settings);

                _file.File.WriteAllText(_configFilename, json);

                var info = new ProcessStartInfo("Optimization.exe", _configFilename)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

                _process.Start(info);
                string output;
                FixedSizeQueue<string> queue = new FixedSizeQueue<string>(2);
                while ((output = _process.ReadLine()) != null)
                {
                    queue.Enqueue(output);
                    //Console.WriteLine(output);

                    if (queue.First() == GeneManager.Termination)
                    {
                        _logWrapper.Info($"For period: {current.StartDate} {current.EndDate}");
                        _logWrapper.Info(queue.Dequeue());
                        _logWrapper.Info(queue.Dequeue());
                        string optimal = queue.Dequeue();
                        _logWrapper.Info(optimal);

                        if (config.WalkForward)
                        {
                            var split = optimal.Split(',');

                            for (int ii = 0; ii < split.Length; ii++)
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

        public void Dispose()
        {
            if (_process != null)
            {
                _process.Kill();
            }
        }

        private class FixedSizeQueue<T> : Queue<T>
        {

            public FixedSizeQueue(int limit)
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
