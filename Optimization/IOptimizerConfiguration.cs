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
    }
}