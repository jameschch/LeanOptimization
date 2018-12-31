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

        public virtual string ScoreKey { get; set; } = "SharpeRatio";
        public override string Name { get; set; } = "Sharpe";
        public IChromosome Best { get; set; }

        public SharpeMaximizer(IOptimizerConfiguration config, IFitnessFilter filter) : base(config, filter)
        {
        }

        public override double Evaluate(IChromosome chromosome)
        {
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
                        optimizer = new ParticleSwarmOptimizer(parameters, maxIterations: Config.Generations, numberOfParticles: Config.PopulationSize,
                            seed: 42);
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

                        var score = GetScore(list);
                        var fitness = CalculateFitness(score);

                        output.AppendFormat("{0}: {1}", Name, fitness.Value.ToString("0.##"));
                        Program.Logger.Info(output);

                        return new OptimizerResult(p, fitness.Fitness);

                    }
                    catch (Exception ex)
                    {
                        Program.Logger.Error($"Id: {id}, Iteration failed.");

                        return new OptimizerResult(p, 1.01);
                    }
                };

                // run optimizer
                var result = optimizer.OptimizeBest(minimize);

                Best = ToChromosome(result.ParameterSet, chromosome);
                Best.Fitness = result.Error;

                if (result == null)
                {
                    return 0;
                }

                return result.Error;
            }
            catch (Exception ex)
            {
                Program.Logger.Error(ex);
                return 0;
            }
        }

        protected virtual Dictionary<string, decimal> GetScore(Dictionary<string, object> list)
        {
            return OptimizerAppDomainManager.RunAlgorithm(list, Config);
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

        protected override FitnessResult CalculateFitness(Dictionary<string, decimal> result)
        {
            var ratio = result[ScoreKey];

            if (Filter != null && !Filter.IsSuccess(result, this))
            {
                ratio = ErrorRatio;
            }

            return new FitnessResult
            {
                Value = ratio,
                Fitness = 1 - ((double)ratio / 1000)
            };
        }

        public override double GetValueFromFitness(double? fitness)
        {
            return ((fitness ?? 1.01) - 1) * 1000 * -1;
        }


    }
}
