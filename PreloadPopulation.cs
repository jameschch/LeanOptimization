using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class PreloadPopulation : Population
    {
        private IList<IChromosome> preloadChromosome;

        public PreloadPopulation(int minSize, int maxSize, IList<IChromosome> preloadChromosomes)
            : base(minSize, maxSize, preloadChromosomes.FirstOrDefault())
        {
            preloadChromosome = preloadChromosomes;
        }

        public override void CreateInitialGeneration()
        {
            Generations = new List<Generation>();
            GenerationsNumber = 0;

            foreach (var c in preloadChromosome)
            {
                c.ValidateGenes();
            }

            CreateNewGeneration(preloadChromosome);
        }
    }
}
