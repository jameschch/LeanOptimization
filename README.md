# LeanOptimization
Genetic optimization using LEAN

This fork retains some of the original execution model but is ported to the GeneticSharp library and now supports parallel backtests. The configuration has also been overhauled.

You must edit the file "optimization.json" to define genes and settings. The gene values are fed into the LEAN config and can be accessed in an algorithm using the QuantConnect.Configuration.Config methods.

It is also possible to execute a sequence of optimizations using the Optimization.Batcher tool. When configured correctly, this will allow for multiple period and walk forward optimization. 
