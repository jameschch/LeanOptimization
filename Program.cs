using System;
using QuantConnect;
using QuantConnect.Lean.Engine;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Packets;
using GAF;
using GAF.Extensions;
using GAF.Operators;
using QuantConnect.Configuration;
using System.Threading;
using QuantConnect.Util;

namespace Optimization
{
	public class RunClass
	{
		private Engine _engine;
		private static Thread _leanEngineThread;
		private IResultHandler _resultsHandler;
		private string algorithmPath = "";
		private AlgorithmNodePacket job = null;
		LeanEngineSystemHandlers systemHandlers = null;
		public RunClass()
		{
			
		}
		public decimal Run(int val)
		{
			Config.Set ("EMA_VAR1", val.ToString ());
			LaunchLean ();
			_resultsHandler = _engine.AlgorithmHandlers.Results;
			if (_resultsHandler != null) {
				DesktopResultHandler dsktophandler = (DesktopResultHandler)_resultsHandler;
				var sharpe_ratio = 0.0m;
				var ratio = dsktophandler.FinalStatistics ["Sharpe Ratio"];
				Decimal.TryParse(ratio,out sharpe_ratio);
				//_engine = null;
				return sharpe_ratio;
			}
			return -1.0m;
		}
		private void LaunchLean()
		{
			
			if (_engine == null) {
				systemHandlers = LeanEngineSystemHandlers.FromConfiguration (Composer.Instance);
				var algorithmHandlers = LeanEngineAlgorithmHandlers.FromConfiguration (Composer.Instance);
				_engine = new Engine (systemHandlers, algorithmHandlers, Config.GetBool ("live-mode"));
				//if (job == null)
			}
			job = systemHandlers.JobQueue.NextJob(out algorithmPath);
			_engine.Run(job, algorithmPath);

		}

	}
	class MainClass
	{
		private static RunClass rc;
		public static void Main (string[] args)
		{
			string algorithm = "EMATest";
		
			Console.WriteLine("Running " + algorithm + "...");

			Config.Set("algorithm-type-name", algorithm);
			Config.Set("live-mode", "false");
			Config.Set("messaging-handler", "QuantConnect.Messaging.Messaging");
			Config.Set("job-queue-handler", "QuantConnect.Queues.JobQueue");
			Config.Set("api-handler", "QuantConnect.Api.Api");
			Config.Set("result-handler", "QuantConnect.Lean.Engine.Results.DesktopResultHandler");
			Config.Set ("environment", "desktop");
			Config.Set ("EMA_VAR1", "10");
		

			rc = new RunClass();
			const double crossoverProbability = 0.65;
			const double mutationProbability = 0.08;
			const int elitismPercentage = 5;

			//create the population
			//var population = new Population(100, 44, false, false);

			var population = new Population();

			//create the chromosomes
			for (var p = 0; p < 100; p++)
			{

				var chromosome = new Chromosome();
				for (int i = 0; i < 100; i++)
					chromosome.Genes.Add (new Gene (i));
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
			ga.Operators.Add(mutation);

			//run the GA 
			ga.Run(Terminate);
		}
		static void ga_OnRunComplete(object sender, GaEventArgs e)
		{
			var fittest = e.Population.GetTop(1)[0];
			foreach (var gene in fittest.Genes)
			{
				Console.WriteLine((int)gene.RealValue);
			}
		}

		private static void ga_OnGenerationComplete(object sender, GaEventArgs e)
		{
			var fittest = e.Population.GetTop(1)[0];
			var sharpe = RunAlgorithm(fittest);
			Console.WriteLine("Generation: {0}, Fitness: {1},Distance: {2}", e.Generation, fittest.Fitness, sharpe);                
		}

		public static double CalculateFitness(Chromosome chromosome)
		{
			var sharpe = RunAlgorithm(chromosome);
			return sharpe;
		}

		private static double RunAlgorithm(Chromosome chromosome)
		{

			var sum_sharpe = 0.0;
			foreach (var gene in chromosome.Genes)
			{
				var val = (int)gene.ObjectValue;
				sum_sharpe += (double)rc.Run (val);

			}

			return sum_sharpe;
		}

		public static bool Terminate(Population population, 
			int currentGeneration, long currentEvaluation)
		{
			return currentGeneration > 400;
		}


	}
}

