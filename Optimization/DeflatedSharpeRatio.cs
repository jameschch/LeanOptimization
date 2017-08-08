using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using System.Runtime.CompilerServices;

namespace Optimization
{
    public class DeflatedSharpeRatio
    {


        protected Dictionary<string, double> SharpeData { get; set; }
        protected Dictionary<string, double> ReturnsData { get; set; }
        protected double N; //number of trials
        protected double V; //variance of results
        protected double T; //sample length.
        protected double Skewness;
        protected double Kurtosis;
        protected double SharpeRatio;

        public virtual void Initialize(IOptimizerConfiguration config)
        {
            var fullResults = OptimizerAppDomainManager.GetResults(AppDomain.CurrentDomain);
            SharpeData = fullResults.ToDictionary(k => k.Key, v => (double)v.Value["SharpeRatio"]);
            ReturnsData = fullResults.ToDictionary(k => k.Key, v => (double)v.Value["CompoundingAnnualReturn"]);


            N = SharpeData.Where(d => d.Value != 0).Count();
            var statistics = new DescriptiveStatistics(ReturnsData.Select(d => d.Value));
            V = statistics.Variance;
            T = (config.EndDate - config.StartDate).Value.TotalDays;
            Skewness = statistics.Skewness;
            Kurtosis = statistics.Kurtosis;
            SharpeRatio = SharpeData.Max(d => d.Value);
        }

        //cumulative standard normal distribution
        private double Z(double x)
        {
            return new Normal(0, 1).CumulativeDistribution(x);
        }

        //cumulative standard normal distribution inverse
        private double ZInverse(double x)
        {
            return new Normal(0, 1).InverseCumulativeDistribution(Z(x));
        }

        public double CalculateExpectedMaximum()
        {
            var result = Math.Sqrt(1 / V) * ((1 - Constants.EulerMascheroni) * ZInverse(1 - 1 / N) + Constants.EulerMascheroni * ZInverse(1 - 1 / (N * Constants.E)));
            return result;
        }

        public double CalculateDeflatedSharpeRatio(double sharpeRatioZero)
        {
            var nonAnnualized = (SharpeRatio / Math.Sqrt(250));
            var top = (nonAnnualized - sharpeRatioZero) * Math.Sqrt(T - 1);
            var bottom = Math.Sqrt(1 - (Skewness) * nonAnnualized + ((Kurtosis - 1) / 4) * Math.Pow(nonAnnualized, 2));

            return Z(top / bottom);
        }

        public double Calculate(IOptimizerConfiguration config)
        {
            Initialize(config);
            return CalculateDeflatedSharpeRatio(CalculateExpectedMaximum());
        }

    }
}
