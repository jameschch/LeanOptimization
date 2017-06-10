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

        private readonly IFileSystem _file;
        private readonly IProcessWrapper _process;
        private readonly ILogWrapper _logWrapper;
        private readonly IGeneManagerFactory _managerFactory;
        const string _configFilename = "optimization_dynasty.json";
        private FixedSizeQueue<string> _queue;
        private DynastyConfiguration _config;
        OptimizerConfiguration _current;
        private static Dynasty _instance;

        public Dynasty(IFileSystem file, ILogWrapper logWrapper, IGeneManagerFactory managerFactory)
        {
            _file = file;
            _logWrapper = logWrapper;
            _managerFactory = managerFactory;
            _queue = new FixedSizeQueue<string>(2);
            _current = null;
            _instance = this;
        }

        public Dynasty() : this(new FileSystem(), new LogWrapper(), new GeneManagerFactory())
        {
        }

        public void Optimize()
        {
            _config = JsonConvert.DeserializeObject<DynastyConfiguration>(_file.File.ReadAllText("dynasty.json"));

            if (_config.DurationDays == 0 && _config.DurationHours == 0)
            {
                throw new ArgumentException("Duration must be specified");
            }

            var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

            for (var i = _config.StartDate; i <= _config.EndDate; i = i.AddDays(_config.DurationDays).AddHours(_config.DurationHours))
            {
                if (_current == null)
                {
                    _current = JsonConvert.DeserializeObject<OptimizerConfiguration>(_file.File.ReadAllText(_configFilename));
                }

                _current.StartDate = i;
                _current.EndDate = i.AddDays(_config.DurationDays).AddHours(_config.DurationHours);

                string json = JsonConvert.SerializeObject(_current, settings);

                _file.File.WriteAllText(_configFilename, json);

                _logWrapper.Result($"For period: {_current.StartDate} {_current.EndDate}");

                var initializer = new Optimization.OptimizerInitializer(_file, _managerFactory.Create());
                initializer.Initialize(new[] { _configFilename });
            }
        }

        public void Watch(string message)
        {
            _queue.Enqueue(message);
            //Console.WriteLine(output);

            if (_queue.First() == GeneManager.Termination)
            {
                _logWrapper.Result(_queue.Dequeue());
                _logWrapper.Result(_queue.Dequeue());
                string optimal = _queue.Dequeue();
                _logWrapper.Result(optimal);

                if (_config.WalkForward)
                {
                    var split = optimal.Split(',');

                    for (int ii = 0; ii < split.Length; ii++)
                    {
                        string[] pair = split[ii].Split(':');
                        var gene = _current.Genes.SingleOrDefault(g => g.Key == pair[0].Trim());

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
                            throw new Exception($"Unable to parse optimal gene from range {_current.StartDate} {_current.EndDate}");
                        }
                    }
                }
            }

        }

        public static void LogOutput(string message)
        {
            _instance.Watch(message);
        }

    }
}
