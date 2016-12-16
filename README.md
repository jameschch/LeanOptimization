# LeanOptimization
Genetic optimization using LEAN

This fork retains some of the original execution model but is ported to the GeneticSharp library. The configuration has also been overhauled.

You must edit the file "optimization.json" to define genes and settings. The gene values are fed into the LEAN config and can be accessed in an algorithm using the QuantConnect.Configuration.Config methods.
