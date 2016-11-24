using GAF;
using GAF.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class ReplaceBottomOperator : IGeneticOperator
    {

        private int _invoked = 0;
        private int _items = 1;


        public ReplaceBottomOperator(int items)
        {
            Enabled = true;
            _items = items;
        }

        public void Invoke(Population currentPopulation, ref Population newPopulation, FitnessFunction fitnesFunctionDelegate)
        {
            //replace bottom with random
            var min = System.Math.Min(_items, currentPopulation.Solutions.Count);

            var bottom = currentPopulation.GetBottom(min);
            try
            {

                foreach (var chromosome in currentPopulation.Solutions)
                {
                    var replacing = chromosome;
                    if (bottom.Contains(chromosome))
                    {
                        replacing = new Chromosome();

                        var spawn = Variables.SpawnRandom();
                        replacing.Genes.Add(new Gene(spawn.Items["p1"]));
                        replacing.Genes.Add(new Gene(spawn.Items["p2"]));
                        replacing.Genes.Add(new Gene(spawn.Items["p3"]));
                        replacing.Genes.Add(new Gene(spawn.Items["p4"]));
                        replacing.Genes.Add(new Gene(spawn.Items["stop"]));
                        replacing.Genes.Add(new Gene(spawn.Items["take"]));

                    }
                    //replacing.Genes.ShuffleFast();
                    replacing.ClearFitness();
                    newPopulation.Solutions.Add(replacing);
                }

                _invoked++;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error encountered: " + e.Message + " " + e.StackTrace);
            }
        }

        public int GetOperatorInvokedEvaluations()
        {
            return _invoked;
        }

        public bool Enabled { get; set; }
    }
}
