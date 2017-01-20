using GeneticSharp.Domain.Mutations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using Accord.MachineLearning.DecisionTrees;
using Accord.Statistics.Filters;
using Accord.MachineLearning.Bayes;
using Accord.Statistics.Distributions.Univariate;
using Accord.Statistics.Distributions.Fitting;
using GeneticSharp.Domain.Reinsertions;
using GeneticSharp.Domain.Populations;

namespace Optimization
{
    class BayesReinsertion : ElitistReinsertion
    {


        protected override IList<IChromosome> PerformSelectChromosomes(IPopulation population, IList<IChromosome> offspring, IList<IChromosome> parents)
        {

            //standard elite insertion as base
            var inserting = base.PerformSelectChromosomes(population, offspring, parents);

            var outputs = Program.Results.Select(r => (int)(Math.Round(r.Value, 1) * 10)).ToArray();
            var inputs = Program.Results.Select(r => r.Key.Split(',').Select(k => double.Parse(k)).ToArray()).ToArray();
            var classes = outputs.Distinct().ToArray();
            var config = GeneFactory.Load();
            
            Parallel.ForEach(inserting, item =>
            {
                try
                {
                    //do not modify fittest
                    if (item != population.BestChromosome)
                    {
                        var learning = new NaiveBayesLearning<NormalDistribution>();
                        learning.Options.InnerOption = new NormalOptions
                        {
                            Regularization = 1e-1
                        };
                        //learn entire evolution
                        var bayes = learning.Learn(inputs, outputs);

                        //arbitrary number of attempts
                        Parallel.For(0, 10000, (i, state) =>
                        {
                            var created = new Chromosome(false, config);

                            //only alpha produces offspring
                            created.ReplaceGenes(0, population.BestChromosome.GetGenes());

                            //attempt mutations of fittest
                            new UniformMutation(true).Mutate(created, 1);
                            var genes = created.GetGenes().Select(r => double.Parse((((KeyValuePair<string, object>)r.Value).Value.ToString()))).ToArray();

                            //estimate fitness of mutant
                            var predicted = bayes.Decide(genes);

                            //keep mutation if predicted top 50% fittest
                            if (classes[predicted] >= classes.Max()/2)
                            {
                                item = created;
                                state.Stop();
                            }
                        }); 
                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            });

            return inserting;

        }

    }
}
