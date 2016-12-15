using GeneticSharp.Domain.Chromosomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{

    public class Chromosome : ChromosomeBase
    {

        GeneConfiguration[] _config;
        bool _isActual;

        public Chromosome(bool isActual, GeneConfiguration[] config) : base(config.Length)
        {
            _isActual = isActual;
            _config = config;

            for (int i = 0; i < _config.Length; i++)
            {
                ReplaceGene(i, GenerateGene(i));
            }
        }

        public override Gene GenerateGene(int geneIndex)
        {
            var item = _config[geneIndex];
            return GeneFactory.Generate(item, _isActual);
        }

        public override IChromosome CreateNew()
        {
            var config = GeneFactory.Load();
            return new Chromosome(false, config);
        }

        public override IChromosome Clone()
        {
            var clone = base.Clone() as Chromosome;
            return clone;
        }

    }

}
