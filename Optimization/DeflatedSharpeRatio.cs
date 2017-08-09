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
    public class DeflatedSharpeRatioFitness
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


            N = SharpeData.Count(d => d.Value != 0);
            var statistics = new DescriptiveStatistics(ReturnsData.Select(d => d.Value));
            V = statistics.Variance;
            T = (config.EndDate - config.StartDate).Value.TotalDays;
            Skewness = statistics.Skewness;
            Kurtosis = statistics.Kurtosis;
            SharpeRatio = SharpeData.Average(d => d.Value);
        }

        //cumulative standard normal distribution
        private double Z(double x)
        {
            return Normal.CDF(0, 1, x);
        }

        //cumulative standard normal distribution inverse
        private double ZInverse(double x)
        {
            return Normal.InvCDF(0, 1, x);
        }

        public double CalculateExpectedMaximum()
        {
            var asd = ZInverse(1 - 1 / N);
            var qwe = ZInverse(1 - 1 / (N * Constants.E));
            var maxZ = (1 - Constants.EulerMascheroni) * ZInverse(1 - 1 / N) + Constants.EulerMascheroni * ZInverse(1 - 1 / (N * Constants.E));
            return SharpeRatio + Math.Sqrt(V) * maxZ;
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
