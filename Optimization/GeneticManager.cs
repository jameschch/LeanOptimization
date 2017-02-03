using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Reinsertions;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Infrastructure.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class GeneticManager
    {

        IOptimizerConfiguration _config;
        SmartThreadPoolTaskExecutor _executor;
        Population _population;
        OptimizerFitness _fitness;
        ILogManager _logManager;

        public GeneticManager(IOptimizerConfiguration config, OptimizerFitness fitness, ILogManager logManager)
        {
            _config = config;
            _fitness = fitness;
            _logManager = logManager;
        }

        public void Start()
        {
            _executor = new SmartThreadPoolTaskExecutor() { MinThreads = 1 };
            _executor.MaxThreads = _config.MaxThreads > 0 ? _config.MaxThreads : 8;

            //create the population
            IList<IChromosome> list = new List<IChromosome>();
            var geneConfig = GeneFactory.Load();
            for (int i = 0; i < _config.PopulationSize; i++)
            {
                list.Add(new Chromosome(true, geneConfig));
            }

            int max = _config.PopulationSizeMaximum < _config.PopulationSize ? _config.PopulationSize * 2 : _config.PopulationSizeMaximum;
            _population = new PreloadPopulation(_config.PopulationSize, max, list);
            _population.GenerationStrategy = new PerformanceGenerationStrategy();

            //create the GA itself 
            var ga = new GeneticAlgorithm(_population, _fitness, new TournamentSelection(),
                _config.OnePointCrossover ? new OnePointCrossover() : new TwoPointCrossover(), new UniformMutation(true));

            //subscribe to events
            ga.GenerationRan += GenerationRan;
            ga.TerminationReached += TerminationReached;
            ga.TaskExecutor = _executor;
            ga.Termination = new OrTermination(new FitnessStagnationTermination(_config.StagnationGenerations), new GenerationNumberTermination(_config.Generations));
            ga.Reinsertion = new ElitistReinsertion();

            //run the GA 
            ga.Start();
        }

        void TerminationReached(object sender, EventArgs e)
        {
            _logManager.Output("Termination reached.");
            string output = "";

            var fittest = _population.BestChromosome;

            foreach (var item in ((Chromosome)fittest).ToDictionary())
            {
                output += item.Key + ": " + item.Value.ToString() + ", ";
            }

            output += string.Format("{0}: {1}", _fitness.Name, _fitness.GetValueFromFitness(fittest.Fitness));
            _logManager.Output(output);
        }

        void GenerationRan(object sender, EventArgs e)
        {
            var fittest = _population.BestChromosome;
            _logManager.Output("Algorithm: {0}, Generation: {1}, Fitness: {2}, {3}: {4}", _config.AlgorithmTypeName, _population.GenerationsNumber, fittest.Fitness,
                _fitness.Name, _fitness.GetValueFromFitness(fittest.Fitness));
        }

    }
}
