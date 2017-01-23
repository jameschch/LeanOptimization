using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
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

        OptimizerConfiguration _config;
        SmartThreadPoolTaskExecutor _executor;
        Population _population;

        public GeneticManager(OptimizerConfiguration config)
        {
            _config = config;
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

            int max = _config.PopulationSizeMaximum < _config.PopulationSize ? _config.PopulationSize : _config.PopulationSizeMaximum;
            _population = new PreloadPopulation(_config.PopulationSize, max, list);
            _population.GenerationStrategy = new PerformanceGenerationStrategy();

            //create the GA itself 
            var ga = new GeneticAlgorithm(_population, new Fitness(), new TournamentSelection(),
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
            Program.Output("Termination reached.");
            string output = "";

            var fittest = _population.BestChromosome;

            foreach (var item in ((Chromosome)fittest).ToDictionary())
            {
                output += item.Key + ": " + item.Value.ToString() + ", ";
            }

            output += string.Format("sharpe: {0}", (fittest.Fitness * 200) - 10); 
            Program.Output(output);
        }

        void GenerationRan(object sender, EventArgs e)
        {
            var fittest = _population.BestChromosome;
            Program.Output("Algorithm: {0}, Generation: {1}, Fitness: {2}, Sharpe: {3}", _config.AlgorithmTypeName, _population.GenerationsNumber, fittest.Fitness,
                (fittest.Fitness * 200) - 10);
        }

    }
}
