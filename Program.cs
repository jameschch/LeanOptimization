using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using QuantConnect;
using QuantConnect.Lean.Engine;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Packets;
using QuantConnect.Configuration;
using QuantConnect.Util;
using QuantConnect.Interfaces;
using QuantConnect.Queues;
using QuantConnect.Messaging;
using QuantConnect.Api;
using QuantConnect.Logging;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Lean.Engine.Setup;
using QuantConnect.Lean.Engine.RealTime;
using QuantConnect.Lean.Engine.TransactionHandlers;
using QuantConnect.Lean.Engine.HistoricalData;
using GAF;
using GAF.Extensions;
using GAF.Operators;

namespace Optimization
{

	public class ConfigVarsOperator : IGeneticOperator
	{
		
		private int _invoked = 0;
		private static Random rand = new Random ();

		public ConfigVarsOperator()
		{
			Enabled = true;
		}
		public void Invoke(Population currentPopulation, 
			ref Population newPopulation, FitnessFunction fitnesFunctionDelegate)
		{
			//take top 3 
			var num = 3;
			var min = System.Math.Min (num, currentPopulation.Solutions.Count);

			var best = currentPopulation.GetTop (min);
			var cutoff = best [min-1].Fitness;
			var genecount = best [0].Genes.Count;
			try
			{
				
				var config_vars = (ConfigVars)best[rand.Next(0,min-1)].Genes [rand.Next(0,genecount-1)].ObjectValue;
				var index = rand.Next(0, config_vars.vars.Count-1);
				var key = config_vars.vars. ElementAt(index).Key;
				newPopulation.Solutions.Clear();
				foreach (var chromosome in currentPopulation.Solutions) {
					if (chromosome.Fitness < cutoff) {
						foreach (var gene in chromosome.Genes) {
							var target_config_vars = (ConfigVars)gene.ObjectValue;
							target_config_vars.vars [key] = config_vars.vars [key];
						}
					}
					newPopulation.Solutions.Add (chromosome);
				}

				_invoked++;
			}
			catch(Exception e) {
				Console.WriteLine ("OOPS! " + e.Message + " " + e.StackTrace);
			}
		}

		public int GetOperatorInvokedEvaluations()
		{
			return _invoked;
		}

		public bool Enabled { get; set; }
	}

	[Serializable]
	public class ConfigVars
	{
		public Dictionary<string,object> vars = new  Dictionary<string, object> ();
		public override bool Equals(object obj) 
		{ 
			var item = obj as ConfigVars; 
			return Equals(item); 
		} 

		protected bool Equals(ConfigVars other) 
		{ 
			foreach (KeyValuePair<string,object> kvp in vars) {
				if (kvp.Value.ToString () != other.vars [kvp.Key].ToString ())
					return false;
			}
			return true;
		} 

		public override int GetHashCode() 
		{ 
			unchecked 
			{ 
				int hashCode = 0;
				foreach (KeyValuePair<string,object> kvp in vars)
					hashCode = hashCode * kvp.Value.GetHashCode ();
				return hashCode; 
			} 
		} 
	}
	public class RunClass: MarshalByRefObject
	{
		private Api _api;
		private Messaging _notify;
		private JobQueue _jobQueue;
		private IResultHandler _resultshandler;

		private FileSystemDataFeed _dataFeed;
		private ConsoleSetupHandler _setup;
		private BacktestingRealTimeHandler _realTime;
		private ITransactionHandler _transactions;
		private IHistoryProvider _historyProvider;

		private readonly Engine _engine;

		public RunClass()
		{
			
		}
		public decimal Run(ConfigVars vars)
		{
			foreach(KeyValuePair<string,object> kvp in vars.vars)
				Config.Set (kvp.Key, kvp.Value.ToString ());

			LaunchLean ();
			ConsoleResultHandler resultshandler = (ConsoleResultHandler)_resultshandler;
			var sharpe_ratio = 0.0m;
			var ratio = resultshandler.FinalStatistics ["Sharpe Ratio"];
			Decimal.TryParse(ratio,out sharpe_ratio);
			return sharpe_ratio;
		}
		private void LaunchLean()
		{
			
			Config.Set ("environment", "backtesting");
			string algorithm = "EMATest";

			Config.Set("algorithm-type-name", algorithm);

			_jobQueue = new JobQueue (); 
			_notify = new Messaging ();
			_api = new Api();
			_resultshandler = new DesktopResultHandler ();
			_dataFeed = new FileSystemDataFeed ();
			_setup = new ConsoleSetupHandler ();
			_realTime = new BacktestingRealTimeHandler ();
			_historyProvider = new SubscriptionDataReaderHistoryProvider ();
			_transactions = new BacktestingTransactionHandler ();
			var systemHandlers = new LeanEngineSystemHandlers (_jobQueue, _api, _notify);
			systemHandlers.Initialize ();

//			var algorithmHandlers = new LeanEngineAlgorithmHandlers (_resultshandler, _setup, _dataFeed, _transactions, _realTime, _historyProvider);
			Log.LogHandler = Composer.Instance.GetExportedValueByTypeName<ILogHandler>(Config.Get("log-handler", "CompositeLogHandler"));

			LeanEngineAlgorithmHandlers leanEngineAlgorithmHandlers;
			try
			{
				leanEngineAlgorithmHandlers = LeanEngineAlgorithmHandlers.FromConfiguration(Composer.Instance);
				_resultshandler = leanEngineAlgorithmHandlers.Results;
			}
			catch (CompositionException compositionException)
			{
				Log.Error("Engine.Main(): Failed to load library: " + compositionException);
				throw;
			}
			string algorithmPath;
			AlgorithmNodePacket job = systemHandlers.JobQueue.NextJob(out algorithmPath);
			try
			{
				var _engine = new Engine(systemHandlers, leanEngineAlgorithmHandlers, Config.GetBool("live-mode"));
				_engine.Run(job, algorithmPath);
			}
			finally
			{
				//Delete the message from the job queue:
				//systemHandlers.JobQueue.AcknowledgeJob(job);
				Log.Trace("Engine.Main(): Packet removed from queue: " + job.AlgorithmId);

				// clean up resources
				systemHandlers.Dispose();
				leanEngineAlgorithmHandlers.Dispose();
				Log.LogHandler.Dispose();
			}
		}

	}

	class MainClass
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
		private static int RandomNumberBetweenInt(int minValue, int maxValue)
		{
			return random.Next (minValue,maxValue);

		}

		public static void Main (string[] args)
		{

		
			_ads = SetupAppDomain ();


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
				for (int i = 0; i < 2; i++) {
					ConfigVars v = new ConfigVars ();
					v.vars ["EMA_VAR1"] = RandomNumberBetweenInt (0, 20);
					v.vars ["EMA_VAR2"] = RandomNumberBetweenInt (0, 100);
					//v.vars ["LTD3"] = RandomNumberBetweenInt (0, 100);
					//v.vars ["LTD4"] = RandomNumberBetweenInt (2, 200);

					chromosome.Genes.Add (new Gene (v));
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
//			ga.Operators.Add(elite);
//			ga.Operators.Add(crossover);
//			ga.Operators.Add(mutation);

			var cv_operator = new ConfigVarsOperator ();
			ga.Operators.Add (cv_operator);

			//run the GA 
			ga.Run(Terminate);
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
			ads.ConfigurationFile = 
				AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			return ads;
		}

		static RunClass CreateRunClassInAppDomain(ref AppDomain ad)
		{
			
			// Create the second AppDomain.
			var name = Guid.NewGuid().ToString("x");
			ad = AppDomain.CreateDomain(name, null, _ads);

			// Create an instance of MarshalbyRefType in the second AppDomain. 
			// A proxy to the object is returned.
			RunClass rc = 
				(RunClass) ad.CreateInstanceAndUnwrap(
					_exeAssembly, 
					typeof(RunClass).FullName
				);

			return rc;
		}

		static void ga_OnRunComplete(object sender, GaEventArgs e)
		{
			var fittest = e.Population.GetTop(1)[0];
			foreach (var gene in fittest.Genes)
			{
				ConfigVars v = (ConfigVars)gene.ObjectValue;
				foreach(KeyValuePair<string,object> kvp in v.vars)
					Console.WriteLine("Variable {0}:, value {1}",kvp.Key,kvp.Value.ToString());
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
			return sharpe;
		}

		private static double RunAlgorithm(Chromosome chromosome)
		{

			var sum_sharpe = 0.0;
			var i = 0;
			foreach (var gene in chromosome.Genes)
			{
				Console.WriteLine ("Running gene number {0}", i);
				var val = (ConfigVars)gene.ObjectValue;
				AppDomain ad = null;
				RunClass rc = CreateRunClassInAppDomain (ref ad);
				foreach(KeyValuePair<string,object> kvp in val.vars)
					Console.WriteLine("Running algorithm with variable {0}:, value {1}",kvp.Key,kvp.Value.ToString());
				
				var res = (double)rc.Run (val);
				Console.WriteLine ("Sharpe ratio: {0}", res);
				sum_sharpe += res;
				AppDomain.Unload (ad);
				Console.WriteLine ("Sum Sharpe ratio: {0}",sum_sharpe);

				i++;
			}

			return sum_sharpe;
		}

		public static bool Terminate(Population population, 
			int currentGeneration, long currentEvaluation)
		{
			return currentGeneration > 2;
		}


	}
}

