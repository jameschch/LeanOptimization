using GeneticSharp.Domain.Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using SharpLearning.Optimization;

namespace Optimization
{

    public class SharpeMaximizer : OptimizerFitness
    {

        public IChromosome Best { get; set; }

        public SharpeMaximizer(IOptimizerConfiguration config, IFitnessFilter filter) : base(config, filter)
        {
        }

        public override double Evaluate(IChromosome chromosome)
        {
            Name = "Sharpe";

            try
            {
                var parameters = Config.Genes.Select(s =>
                    new MinMaxParameterSpec(min: (double)(s.MinDecimal ?? s.MinInt.Value), max: (double)(s.MaxDecimal ?? s.MaxInt.Value),
                        transform: Transform.Linear, parameterType: s.Precision > 0 ? ParameterType.Continuous : ParameterType.Discrete)
                ).ToArray();


                IOptimizer optimizer = null;
                if (Config.Fitness == null || Config.Fitness.OptimizerTypeName == Enums.OptimizerTypeOptions.RandomSearch.ToString())
                {
                    optimizer = new RandomSearchOptimizer(parameters, iterations: Config.Generations, runParallel: true);
                }
                else if (Config.Fitness != null)
                {
                    if (Config.Fitness.OptimizerTypeName == Enums.OptimizerTypeOptions.ParticleSwarm.ToString())
                    {
                        optimizer = new ParticleSwarmOptimizer(parameters, maxIterations: Config.Generations, numberOfParticles: Config.PopulationSizeMaximum, seed: 42);
                    }
                    else if (Config.Fitness.OptimizerTypeName == Enums.OptimizerTypeOptions.Genetic.ToString())
                    {
                        throw new Exception("Genetic optimizer cannot be used with Sharpe Maximizer");
                    }
                }

                //todo:
                // GridSearchOptimizer
                // GlobalizedBoundedNelderMeadOptimizer
                // BayesianOptimizer

                Func<double[], OptimizerResult> minimize = p =>
                {
                    var id = Guid.NewGuid().ToString("N");
                    try
                    {
                        StringBuilder output = new StringBuilder();
                        var list = ((Chromosome)chromosome).ToDictionary();

                        ((Chromosome)chromosome).Id = id;

                        list.Add("Id", ((Chromosome)chromosome).Id);
                        output.Append("Id: " + list["Id"] + ", ");

                        for (int i = 0; i < Config.Genes.Count(); i++)
                        {
                            var key = Config.Genes.ElementAt(i).Key;
                            var precision = Config.Genes.ElementAt(i).Precision ?? 0;
                            var value = Math.Round(p[i], precision);
                            list[key] = value;

                            output.Append(key + ": " + value.ToString() + ", ");
                        }

                        if (Config.StartDate.HasValue && Config.EndDate.HasValue)
                        {
                            output.AppendFormat("Start: {0}, End: {1}, ", Config.StartDate, Config.EndDate);
                        }

                        var score = OptimizerAppDomainManager.RunAlgorithm(list, Config);

                        var fitness = CalculateFitness(score);

                        output.AppendFormat("{0}: {1}", Name, fitness.Value.ToString("0.##"));
                        Program.Logger.Info(output);

                        return new OptimizerResult(p, (double)fitness.Value * -1);

                    }
                    catch (Exception ex)
                    {
                        Program.Logger.Info($"Id: {id}, Iteration failed with error: {ex.ToString()}");

                        return new OptimizerResult(p, 10d);
                    }
                };

                // run optimizer
                var result = optimizer.OptimizeBest(minimize);

                Best = ToChromosome(result.ParameterSet, chromosome);
                Best.Fitness = result.Error * -1;

                if (result == null)
                {
                    return 0;
                }

                return result.Error * -1;
            }
            catch (Exception ex)
            {
                Program.Logger.Error(ex);
                return 0;
            }
        }

        private IChromosome ToChromosome(double[] parameters, IChromosome configChromosome)
        {
            var list = ((Chromosome)configChromosome).ToDictionary();
            for (int i = 0; i < Config.Genes.Count(); i++)
            {
                var pair = (KeyValuePair<string, object>)configChromosome.GetGene(i).Value;
                configChromosome.ReplaceGene(i, new Gene(new KeyValuePair<string, object>(pair.Key, parameters[i])));
            }

            return configChromosome;
        }

    }
}
