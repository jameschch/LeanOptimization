using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Fitnesses;
using Newtonsoft.Json;
using System.Reflection;
using System.IO.Abstractions;

namespace Optimization
{
    public class OptimizerInitializer
    {

        private readonly IFileSystem _file;
        private readonly IGeneManager _manager;
        OptimizerConfiguration _config;

        public OptimizerInitializer(IFileSystem file, IGeneManager manager)
        {
            _file = file;
            _manager = manager;
        }

        public OptimizerInitializer() : this(new FileSystem(), new GeneManager())
        {
        }

        public void Initialize(string[] args)
        {
            _config = LoadConfig(args);
            _file.File.Copy(_config.ConfigPath, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"), true);

            string path = _config.AlgorithmLocation;
            if (!string.IsNullOrEmpty(path))
            {
                _file.File.Copy(path, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.IO.Path.GetFileName(path)), true);
                string pdb = path.Replace(System.IO.Path.GetExtension(path), ".pdb");

                //due to locking issues, need to manually clean to update pdb
                if (!_file.File.Exists(pdb))
                {
                    _file.File.Copy(pdb, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.IO.Path.GetFileName(pdb)), true);
                }
            }

            OptimizerAppDomainManager.Initialize(_config);

            OptimizerFitness fitness = (OptimizerFitness)Assembly.GetExecutingAssembly().CreateInstance(_config.FitnessTypeName, false, BindingFlags.Default, null,
                new[] { _config }, null, null);

            _manager.Initialize(_config, fitness);
            _manager.Start();

        }

        private OptimizerConfiguration LoadConfig(string[] args)
        {
            string path = "optimization.json";
            if (args != null && args.Length > 0 && !string.IsNullOrEmpty(args[0]))
            {
                path = args[0];
            }

            return JsonConvert.DeserializeObject<OptimizerConfiguration>(_file.File.ReadAllText(path));
        }

    }

}
