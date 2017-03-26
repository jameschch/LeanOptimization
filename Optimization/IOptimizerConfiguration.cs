namespace Optimization
{
    public interface IOptimizerConfiguration
    {

        string AlgorithmLocation { get; set; }
        string AlgorithmTypeName { get; set; }
        string ConfigPath { get; set; }
        int Generations { get; set; }
        int MaxThreads { get; set; }
        bool OnePointCrossover { get; set; }
        int PopulationSize { get; set; }
        int PopulationSizeMaximum { get; set; }
        int StagnationGenerations { get; set; }
        bool IncludeNegativeReturn { get; set; }
        string FitnessTypeName { get; set; }
        string DataFolder { get; set; }
        FitnessConfiguration Fitness { get; set; }
    }

    public interface IFitnessConfiguration
    {
        string Name { get; set; }
        string ResultKey { get; set; }
        double? Scale { get; set; }
        double? Modifier { get; set; }
    }

}