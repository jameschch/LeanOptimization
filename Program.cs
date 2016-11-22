using GAF;
using GAF.Extensions;
using GAF.Operators;
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
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Optimization
{

    class Program
    {
        private static readonly Random random = new Random();
        private static AppDomainSetup _ads;
        private static string _callingDomainName;
        private static string _exeAssembly;

        private static double RandomNumberBetween(double minValue, double maxValue)
        {
            var next = random.NextDouble();

            return minValue + (next * (maxValue - minValue));
        }

        private static int RandomBetween(int minValue, int maxValue)
        {
            return random.Next(minValue, maxValue);
        }

        private static double RandomBetween(double minValue, double maxValue)
        {
            var rand = random.NextDouble() * (maxValue - minValue) + minValue;
            return System.Math.Round(rand, 3);
        }

        public static void Main(string[] args)
        {
            _ads = SetupAppDomain();


            const double crossoverProbability = 0.65;
            const double mutationProbability = 0.08;
            const int elitismPercentage = 5;

            //create the population
            //var population = new Population(100, 44, false, false);

            var population = new Population();

            //create the chromosomes
            for (var p = 0; p < 10; p++)
            {

                var chromosome = new Chromosome();
                for (int i = 0; i < 2; i++)
                {
                    Variables v = new Variables();
                    v.Items = new Dictionary<string, object>
                    {
                        { "stop", RandomBetween(0.01, 0.06) },
                        { "take", RandomBetween(0.01, 0.06) },
                        { "stddev",  RandomBetween(2.5, 3.5) },
                        { "period", RandomBetween(4, 48) },
                        { "tickWindow", RandomBetween(1, 4) },
                    };

                    chromosome.Genes.Add(new Gene(v));
                }
                chromosome.Genes.ShuffleFast();
                population.Solutions.Add(chromosome);
            }

            //create the genetic operators 
            var elite = new Elite(elitismPercentage);

            var crossover = new Crossover(crossoverProbability, true)
            {
                CrossoverType = CrossoverType.SinglePoint
            };

            var mutation = new BinaryMutate(mutationProbability, true);

            //create the GA itself 
            var ga = new GeneticAlgorithm(population, CalculateFitness);

            //subscribe to the GAs Generation Complete event 
            ga.OnGenerationComplete += ga_OnGenerationComplete;

            //add the operators to the ga process pipeline 
            ga.Operators.Add(elite);
            ga.Operators.Add(crossover);
            //ga.Operators.Add(mutation);

            //var cv_operator = new CustomOperator();
            //ga.Operators.Add(cv_operator);

            //run the GA 
            ga.Run(Terminate);

            Console.ReadKey();
        }

        static AppDomainSetup SetupAppDomain()
        {
            _callingDomainName = Thread.GetDomain().FriendlyName;
            //Console.WriteLine(callingDomainName);

            // Get and display the full name of the EXE assembly.
            _exeAssembly = Assembly.GetEntryAssembly().FullName;
            //Console.WriteLine(exeAssembly);

            // Construct and initialize settings for a second AppDomain.
            AppDomainSetup ads = new AppDomainSetup();
            ads.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;

            ads.DisallowBindingRedirects = false;
            ads.DisallowCodeDownload = true;
            ads.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            return ads;
        }

        static Runner CreateRunClassInAppDomain(ref AppDomain ad)
        {

            // Create the second AppDomain.
            var name = Guid.NewGuid().ToString("x");
            ad = AppDomain.CreateDomain(name, null, _ads);

            // Create an instance of MarshalbyRefType in the second AppDomain. 
            // A proxy to the object is returned.
            Runner rc =
                (Runner)ad.CreateInstanceAndUnwrap(
                    _exeAssembly,
                    typeof(Runner).FullName
                );

            return rc;
        }

        static void ga_OnRunComplete(object sender, GaEventArgs e)
        {
            var fittest = e.Population.GetTop(1)[0];
            foreach (var gene in fittest.Genes)
            {
                Variables v = (Variables)gene.ObjectValue;
                foreach (KeyValuePair<string, object> kvp in v.Items)
                    Console.WriteLine("Variable {0}:, value {1}", kvp.Key, kvp.Value.ToString());
            }
        }

        private static void ga_OnGenerationComplete(object sender, GaEventArgs e)
        {
            var fittest = e.Population.GetTop(1)[0];
            var sharpe = RunAlgorithm(fittest);
            Console.WriteLine("Generation: {0}, Fitness: {1},sharpe: {2}", e.Generation, fittest.Fitness, sharpe);
        }

        public static double CalculateFitness(Chromosome chromosome)
        {
            var sharpe = RunAlgorithm(chromosome);
            return (sharpe + 10) / 200;
        }

        private static double RunAlgorithm(Chromosome chromosome)
        {

            var sum_sharpe = 0.0;
            var i = 0;
            foreach (var gene in chromosome.Genes)
            {
                Console.WriteLine("Running gene number {0}", i);
                var val = (Variables)gene.ObjectValue;
                AppDomain ad = null;
                Runner rc = CreateRunClassInAppDomain(ref ad);
                foreach (KeyValuePair<string, object> kvp in val.Items)
                    Console.WriteLine("Running algorithm with variable {0}:, value {1}", kvp.Key, kvp.Value.ToString());

                var res = (double)rc.Run(val);
                Console.WriteLine("Sharpe ratio: {0}", res);
                sum_sharpe += res;
                AppDomain.Unload(ad);
                Console.WriteLine("Sum Sharpe ratio: {0}", sum_sharpe);

                i++;
            }

            return sum_sharpe;
        }

        public static bool Terminate(Population population, int currentGeneration, long currentEvaluation)
        {
            return currentGeneration > 2;
        }


    }
}

