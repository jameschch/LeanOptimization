# LeanOptimization
Parameter optimization for LEAN

This toolset allows you to execute multiple parallel backtests using a local Lean clone. It is possible to configure several different optimization methods to fit your trading algorithm to an array of different success measures. 

You must edit the config file [optimization.json](https://github.com/jameschch/LeanOptimization/blob/master/Optimization/optimization.json) to define parameters and other settings. The gene values are fed into the Lean config and can be accessed in an algorithm using the QuantConnect.Configuration.Config methods.

An example algorithm is provided here: [ParameterizedAlgorithm](https://github.com/jameschch/LeanOptimization/blob/master/Optimization.Example/ParameterizedAlgorithm.cs)

## Quickstart
1. Clone local Lean.
2. Clone the Optimizer so that it shares the same parent folder as the Lean clone.
3. Edit the config file and enter the location of your trading algorithm dll in "algorithmLocation".
4. Now enter the class name of your algorithm in "algorithmTypeName".
5. Enter the location of your trade and quote bar data in the "dataFolder" setting.
6. Configure the maxThreads to define the number of parallel backtests.

## Configuration

Full documentation is provided in comments: [OptimizerConfiguration](https://github.com/jameschch/LeanOptimization/blob/master/Optimization/OptimizerConfiguration.cs)

The most important options:

### fitnessTypeName

#### Genetic
The default OptimizerFitness is simple Sharpe Ratio maximization. There is also CompoundingAnnualReturnFitness to maximize raw returns. It is possible to optimize any Lean statistic using ConfiguredFitness.

#### General Maximizer
Specify the SharpeMaximizer fitness allows access to all of the optimization methods provided by the SharpLearning library. These include Random Search. Grid Search, Particle Swarm, Smac and several others.

#### Specialist
The simple SharpeMaximizer has been extended in NFoldCrossSharpeMaximizer so that the succes measure is averaged over n-fold periods. This will prevent overfitting to a single in-sample period but is not guaranteed to eliminate overfitting entirely. 
There are several other fitness measures that operate over mutliple periods.

