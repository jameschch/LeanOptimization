using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Reinsertions;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Infrastructure.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantConnect.Api;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Lean.Engine.HistoricalData;
using QuantConnect.Lean.Engine.RealTime;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Lean.Engine.Setup;
using QuantConnect.Lean.Engine.TransactionHandlers;
using QuantConnect.Logging;
using QuantConnect.Messaging;
using QuantConnect.Packets;
using QuantConnect.Queues;
using QuantConnect.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
using static Optimization.GeneFactory;

namespace Optimization
{

    public class Program
    {

        #region Declarations
        static StreamWriter _writer;
        static OptimizerConfiguration _config;
        static Population _population;
        private static AppDomainSetup _ads;
        private static string _exeAssembly;
        static Dictionary<string, decimal> _results;
        static readonly SmartThreadPoolTaskExecutor _executor = new SmartThreadPoolTaskExecutor() { MinThreads = 1, MaxThreads = 8 };
        #endregion

        public static void Main(string[] args)
        {
            _config = LoadConfig();
            string path = _config.ConfigPath;
            System.IO.File.Copy(path, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"), true);

            _results = new Dictionary<string, decimal>();
            _ads = SetupAppDomain();
            _writer = System.IO.File.AppendText("optimizer.txt");
            _executor.MaxThreads = _config.MaxThreads > 0 ? _config.MaxThreads : 8;

            IList<IChromosome> list = new List<IChromosome>();
            //create the population
            var geneConfig = GeneFactory.Load();
            for (int i = 0; i < _config.PopulationSize; i++)
            {
                list.Add(new Chromosome(true, geneConfig));
            }

            _population = new PreloadPopulation(_config.PopulationSize, _config.PopulationSize * 2, list);
            _population.GenerationStrategy = new PerformanceGenerationStrategy();

            //create the GA itself 
            var ga = new GeneticAlgorithm(_population, new Fitness(), new TournamentSelection(), 
                _config.OnePointCrossover ? new OnePointCrossover() : new TwoPointCrossover(), new UniformMutation(true));

            //subscribe to the GAs Generation Complete event 
            ga.GenerationRan += ga_OnGenerationComplete;
            ga.TerminationReached += ga_OnRunComplete;
            ga.TaskExecutor = _executor;
            ga.Termination = new OrTermination(new FitnessStagnationTermination(_config.StagnationGenerations), new GenerationNumberTermination(_config.Generations));
            ga.Reinsertion = new ElitistReinsertion();
            //run the GA 
            ga.Start();

            _writer.Close();

            Console.ReadKey();
        }


        static void ga_OnRunComplete(object sender, EventArgs e)
        {
            var fittest = _population.BestChromosome;
            foreach (var gene in fittest.GetGenes())
            {
                var pair = (KeyValuePair<string, object>)gene.Value;
                Output("{0}: value {1}", pair.Key, pair.Value);
            }
        }

        static void ga_OnGenerationComplete(object sender, EventArgs e)
        {

            var fittest = _population.BestChromosome;
            Output("Algorithm: {0}, Generation: {1}, Fitness: {2}, Sharpe: {3}", _config.AlgorithmTypeName, _population.GenerationsNumber, fittest.Fitness, 
                (fittest.Fitness * 200) - 10);
        }

        public static void Output(string line, params object[] format)
        {
            Output(string.Format(line, format));
        }

        public static void Output(string line)
        {
            _writer.Write(DateTime.Now.ToString("u"));
            _writer.Write(" ");
            _writer.Write(line);
            _writer.Write(_writer.NewLine);
            _writer.Flush();
            Console.WriteLine(line);
        }

        static AppDomainSetup SetupAppDomain()
        {
            // Get and display the full name of the EXE assembly.
            _exeAssembly = Assembly.GetEntryAssembly().FullName;

            // Construct and initialize settings for a second AppDomain.
            AppDomainSetup ads = new AppDomainSetup();
            ads.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;

            ads.DisallowBindingRedirects = false;
            ads.DisallowCodeDownload = true;
            ads.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            return ads;
        }

        public static Runner CreateRunClassInAppDomain(ref AppDomain ad)
        {
            // Create the second AppDomain.
            var name = Guid.NewGuid().ToString("x");
            ad = AppDomain.CreateDomain(name, null, _ads);

            // Create an instance of MarshalbyRefType in the second AppDomain. 
            // A proxy to the object is returned.
            Runner rc = (Runner)ad.CreateInstanceAndUnwrap(_exeAssembly, typeof(Runner).FullName);

            ad.SetData("Results", _results);
            ad.SetData("AlgorithmTypeName", _config.AlgorithmTypeName);

            return rc;
        }

        public static double RunAlgorithm(IChromosome chromosome)
        {

            AppDomain ad = null;
            Runner rc = CreateRunClassInAppDomain(ref ad);
            string output = "";

            foreach (var item in chromosome.GetGenes())
            {
                var pair = (KeyValuePair<string, object>)item.Value;
                output += pair.Key + " " + pair.Value.ToString() + " ";
            }

            output = output.TrimEnd(' ');

            var sharpe = (double)rc.Run(chromosome.GetGenes().ToDictionary(d => ((KeyValuePair<string, object>)d.Value).Key, d => ((KeyValuePair<string, object>)d.Value).Value));

            _results = (Dictionary<string, decimal>)ad.GetData("Results");

            AppDomain.Unload(ad);
            output += string.Format(" sharpe {0}", sharpe);

            Program.Output(output);

            return sharpe;
        }

        private static OptimizerConfiguration LoadConfig()
        {
            using (StreamReader file = File.OpenText("optimization.json"))
            {
                var document = (JObject)JsonConvert.DeserializeObject(file.ReadToEnd());
                return JsonConvert.DeserializeObject<OptimizerConfiguration>(document["optimizer"].ToString());
            }

        }

    }
}