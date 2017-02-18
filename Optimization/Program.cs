using GeneticSharp.Domain.Fitnesses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantConnect.Util;
using System;
using System.IO;
using System.Reflection;

namespace Optimization
{

    public class Program
    {

        #region Declarations
        static StreamWriter _writer;
        static OptimizerConfiguration _config;
        static object _writerLock;
        #endregion

        public static void Main(string[] args)
        {
            _config = LoadConfig(args);
            string path = _config.ConfigPath;
            System.IO.File.Copy(path, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"), true);

            path = _config.AlgorithmLocation;
            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.Copy(path, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(path)), true);
                string pdb = path.Replace(Path.GetExtension(path), ".pdb");
                if (File.Exists(pdb))
                {
                    System.IO.File.Copy(pdb, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(pdb)), true);
                }
            }

            _writerLock = new object();

            using (_writer = System.IO.File.AppendText("optimizer.txt"))
            {
                AppDomainManager.Initialize(_config);

                OptimizerFitness instance = (OptimizerFitness)System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(_config.FitnessTypeName, false, BindingFlags.Default, null,
                    new[] { _config }, null, null);

                var manager = new GeneticManager(_config, instance, new LogManager());
                manager.Start();
            }

            Console.ReadKey();
        }

        public static void Output(string line, params object[] format)
        {
            Output(string.Format(line, format));
        }

        public static void Output(string line)
        {
            lock (_writerLock)
            {
                _writer.Write(DateTime.Now.ToString("u"));
                _writer.Write(" ");
                _writer.Write(line);
                _writer.Write(_writer.NewLine);
            }
            _writer.Flush();
            Console.WriteLine(line);
        }

        private static OptimizerConfiguration LoadConfig(string[] args)
        {
            string path = "optimization.json";
            if (args != null && args.Length > 0 && !string.IsNullOrEmpty(args[0]))
            {
                path = args[0];
            }

            using (StreamReader file = File.OpenText(path))
            {
                var document = (JObject)JsonConvert.DeserializeObject(file.ReadToEnd());
                return JsonConvert.DeserializeObject<OptimizerConfiguration>(document["optimizer"].ToString());
            }
        }

    }
}