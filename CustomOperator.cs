using GAF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class CustomOperator : IGeneticOperator
    {

        private int _invoked = 0;
        private static Random rand = new Random();

        public CustomOperator()
        {
            Enabled = true;
        }

        public void Invoke(Population currentPopulation, ref Population newPopulation, FitnessFunction fitnesFunctionDelegate)
        {
            //take top 3 
            var num = 3;
            var min = System.Math.Min(num, currentPopulation.Solutions.Count);

            var best = currentPopulation.GetTop(min);
            var cutoff = best[min - 1].Fitness;
            var genecount = best[0].Genes.Count;
            try
            {

                var config_vars = (Variables)best[rand.Next(0, min - 1)].Genes[rand.Next(0, genecount - 1)].ObjectValue;
                var index = rand.Next(0, config_vars.Items.Count - 1);
                var key = config_vars.Items.ElementAt(index).Key;
                newPopulation.Solutions.Clear();
                foreach (var chromosome in currentPopulation.Solutions)
                {
                    if (chromosome.Fitness < cutoff)
                    {
                        foreach (var gene in chromosome.Genes)
                        {
                            var target_config_vars = (Variables)gene.ObjectValue;
                            target_config_vars.Items[key] = config_vars.Items[key];
                        }
                    }
                    newPopulation.Solutions.Add(chromosome);
                }

                _invoked++;
            }
            catch (Exception e)
            {
                Console.WriteLine("OOPS! " + e.Message + " " + e.StackTrace);
            }
        }

        public int GetOperatorInvokedEvaluations()
        {
            return _invoked;
        }

        public bool Enabled { get; set; }
    }
}
