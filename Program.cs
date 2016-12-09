using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Reinsertions;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Infrastructure.Threading;
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
        private static readonly Random random = new Random();
        static StreamWriter writer;
        static OptimizerConfiguration config;
        static Population population;
        private static AppDomainSetup _ads;
        private static string _exeAssembly;
        private static string _callingDomainName;
        public static Dictionary<string, decimal> Results;
        static readonly SmartThreadPoolTaskExecutor Executor = new SmartThreadPoolTaskExecutor() { MinThreads = 2, MaxThreads = 8};
        #endregion

        public static void Main(string[] args)
        {
            string path = System.Configuration.ConfigurationManager.AppSettings["ConfigPath"];
            System.IO.File.Copy(path, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"), true);
            //todo: map from config
            config = new OptimizerConfiguration();
            Results = new Dictionary<string, decimal>();
            _ads = SetupAppDomain();

            writer = System.IO.File.AppendText("optimizer.txt");


            IList<IChromosome> list = new List<IChromosome>();
            //create the population
            var geneConfig = GeneFactory.Load();
            for (int i = 0; i < config.PopulationSize; i++)
            {
               list.Add(new Chromosome(true, geneConfig));
            }

            population = new PreloadPopulation(config.PopulationSize, config.PopulationSize*2, list);
            population.GenerationStrategy = new PerformanceGenerationStrategy();

            //create the GA itself 
            var ga = new GeneticAlgorithm(population, new Fitness(), new TournamentSelection(), new TwoPointCrossover(), new UniformMutation(true));

            //subscribe to the GAs Generation Complete event 
            ga.GenerationRan += ga_OnGenerationComplete;
            ga.TerminationReached += ga_OnRunComplete;
            ga.TaskExecutor = Executor;
            ga.Termination = new GenerationNumberTermination(1000);
            ga.Reinsertion = new ElitistReinsertion();
            //run the GA 
            ga.Start();

            writer.Close();

            Console.ReadKey();
        }


        static void ga_OnRunComplete(object sender, EventArgs e)
        {
            var fittest = population.BestChromosome;
            foreach (var gene in fittest.GetGenes())
            {
                var pair = (KeyValuePair<string, object>)gene.Value;
                Output("{0}: value {1}", pair.Key, pair.Value);
            }
        }

        static void ga_OnGenerationComplete(object sender, EventArgs e)
        {

            var fittest = population.BestChromosome;
            Output("Generation: {0}, Fitness: {1}, Sharpe: {2}", population.GenerationsNumber, fittest.Fitness, (fittest.Fitness * 200) - 10);
        }     

        public static void Output(string line, params object[] format)
        {
            Output(string.Format(line, format));
        }

        public static void Output(string line)
        {
            writer.Write(DateTime.Now.ToString("u"));
            writer.Write(line);
            writer.Write(writer.NewLine);
            writer.Flush();
            Console.WriteLine(line);
        }


        static AppDomainSetup SetupAppDomain()
        {
            _callingDomainName = Thread.GetDomain().FriendlyName;

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

            ad.SetData("Results", Results);

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
                output += " " + pair.Key + " " + pair.Value.ToString();
            }

            var sharpe = (double)rc.Run(chromosome.GetGenes().ToDictionary(d => ((KeyValuePair<string, object>)d.Value).Key, d => ((KeyValuePair<string, object>)d.Value).Value));

            Results = (Dictionary<string, decimal>)ad.GetData("Results");

            AppDomain.Unload(ad);
            output += string.Format(" Sharpe:{0}", sharpe);

            Program.Output(output);

            return sharpe;
        }

    }
}

