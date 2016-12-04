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
        private double _percent = 1;


        public ReplaceBottomOperator(double percent)
        {
            Enabled = true;
            _percent = percent;
        }

        public void Invoke(Population currentPopulation, ref Population newPopulation, FitnessFunction fitnesFunctionDelegate)
        {
            //replace bottom with random

            int min = (int)System.Math.Round(currentPopulation.Solutions.Count * _percent);

            min = System.Math.Min(min, currentPopulation.Solutions.Count);

            var bottom = currentPopulation.GetBottom(min);
            try
            {

                foreach (var chromosome in currentPopulation.Solutions)
                {
                    var replacing = chromosome;
                    if (bottom.Contains(chromosome))
                    {
                        replacing = GeneFactory.SpawnRandom();
                    }
                    //replacing.Genes.ShuffleFast();
                    //replacing.ClearFitness();
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
