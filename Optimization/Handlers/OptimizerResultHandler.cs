using QuantConnect;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Lean.Engine.Setup;
using QuantConnect.Lean.Engine.TransactionHandlers;
using QuantConnect.Orders;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Statistics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class OptimizerResultHandler : IResultHandler
    {
        protected IAlgorithm Algorithm { get; set; }

        private IResultHandler _shadow;

        #region Properties
        public Dictionary<string, decimal> FullResults { get; set; }

        public ConcurrentQueue<Packet> Messages
        {
            get
            {
                return _shadow.Messages;
            }

            set
            {
                _shadow.Messages = value;
            }
        }

        public ConcurrentDictionary<string, Chart> Charts
        {
            get
            {
                return _shadow.Charts;
            }
            set
            {
                _shadow.Charts = value;
            }
        }

        public TimeSpan ResamplePeriod => _shadow.ResamplePeriod;

        public TimeSpan NotificationPeriod => _shadow.NotificationPeriod;

        public bool IsActive => _shadow.IsActive;

        private bool _hasError;
        #endregion

        public OptimizerResultHandler()
        {
            _shadow = new BacktestingResultHandler();
        }

        public OptimizerResultHandler(BacktestingResultHandler handler)
        {
            _shadow = handler;
        }


        public void SendFinalResult()
        {
            if (_hasError)
            {
                FullResults = null;
                return;
            }

            //HACK: calculate statistics but not store result
            try
            {

                var shadowType = typeof(BacktestingResultHandler);

                var flags = BindingFlags.Instance | BindingFlags.NonPublic;

                //_processingFinalPacket = true;
                shadowType.GetField("_processingFinalPacket", flags).SetValue(_shadow, true);


                var charts = new Dictionary<string, Chart>(_shadow.Charts);
                //var orders = new Dictionary<int, Order>(_shadow.TransactionHandler.Orders);
                var transactionHandler = (ITransactionHandler)shadowType.GetField("TransactionHandler", flags).GetValue(_shadow);
                var orders = new Dictionary<int, Order>(transactionHandler.Orders);

                var profitLoss = new SortedDictionary<DateTime, decimal>(Algorithm.Transactions.TransactionRecord);

                //var runtime = GetAlgorithmRuntimeStatistics();               
                var runtime = (Dictionary<string, string>)shadowType.InvokeMember("GetAlgorithmRuntimeStatistics", flags | BindingFlags.InvokeMethod, Type.DefaultBinder, _shadow, 
                    new object[]{ new  Dictionary<string, string>(), false });


                //var statisticsResults = GenerateStatisticsResults(charts, profitLoss);
                var statisticsResults = (StatisticsResults)shadowType.InvokeMember("GenerateStatisticsResults", flags | BindingFlags.InvokeMethod, null, _shadow,
                   new object[] { charts, profitLoss });

                FullResults = StatisticsAdapter.Transform(statisticsResults.TotalPerformance, statisticsResults.Summary);

                //FinalStatistics = statisticsResults.Summary;
                shadowType.GetProperty("FinalStatistics").SetValue(_shadow, statisticsResults.Summary, flags, null, null, null);

                foreach (var ap in statisticsResults.RollingPerformances.Values)
                {
                    ap.ClosedTrades.Clear();
                }

                //var result = new BacktestResultPacket(_job,
                //    new BacktestResult(charts, orders, profitLoss, statisticsResults.Summary, runtime, statisticsResults.RollingPerformances, statisticsResults.TotalPerformance)
                //        { AlphaRuntimeStatistics = AlphaRuntimeStatistics })
                //{
                //    ProcessingTime = (DateTime.UtcNow - StartTime).TotalSeconds,
                //    DateFinished = DateTime.Now,
                //    Progress = 1
                //};
                var job = (BacktestNodePacket)shadowType.GetField("_job", flags).GetValue(_shadow);
                var startTime = (DateTime)shadowType.GetProperty("StartTime", flags).GetValue(_shadow);
                var alphaRuntimeStatistics = (AlphaRuntimeStatistics)shadowType.GetProperty("AlphaRuntimeStatistics", flags).GetValue(_shadow);

                var result = new BacktestResultPacket(job,
                    new BacktestResult(charts, orders, profitLoss, statisticsResults.Summary, runtime, statisticsResults.RollingPerformances, statisticsResults.TotalPerformance)
                    { AlphaRuntimeStatistics = alphaRuntimeStatistics })
                {
                    ProcessingTime = (DateTime.UtcNow - startTime).TotalSeconds,
                    DateFinished = DateTime.Now,
                    Progress = 1
                };

                //StoreResult(result);
                //do not store result

                //MessagingHandler.Send(result);
                var messagingHandler = (IMessagingHandler)shadowType.GetField("MessagingHandler", flags).GetValue(_shadow);
                messagingHandler.Send(result);

            }
            catch (Exception ex)
            {
                Program.ErrorLogger.Error(ex);
            }

        }

        #region Shadow Methods
        public void Initialize(AlgorithmNodePacket job, IMessagingHandler messagingHandler, IApi api, IDataFeed dataFeed, ITransactionHandler transactionHandler)
        {
            _shadow.Initialize(job, messagingHandler, api, transactionHandler);
        }

        public void Run()
        {
            _hasError = false;
            _shadow.Run();
        }

        public void DebugMessage(string message)
        {
            _shadow.DebugMessage(message);
        }

        public void SystemDebugMessage(string message)
        {
            _shadow.SystemDebugMessage(message);
        }

        public void SecurityType(List<SecurityType> types)
        {
            _shadow.SecurityType(types);
        }

        public void LogMessage(string message)
        {
            _shadow.LogMessage(message);
        }

        public void ErrorMessage(string error, string stacktrace = "")
        {
            _shadow.ErrorMessage(error, stacktrace);
        }

        public void RuntimeError(string message, string stacktrace = "")
        {
            _shadow.ErrorMessage(message, stacktrace);
            Program.ErrorLogger.Error(new Exception($"{Algorithm.AlgorithmId}:{ message }:{stacktrace}"));
            _hasError = true;
        }

        public void Sample(string chartName, string seriesName, int seriesIndex, SeriesType seriesType, DateTime time, decimal value, string unit = "$")
        {
            _shadow.Sample(chartName, seriesName, seriesIndex, seriesType, time, value, unit);
        }

        public void SampleEquity(DateTime time, decimal value)
        {
            _shadow.SampleEquity(time, value);
        }

        public void SamplePerformance(DateTime time, decimal value)
        {
            _shadow.SamplePerformance(time, value);
        }

        public void SampleBenchmark(DateTime time, decimal value)
        {
            _shadow.SampleBenchmark(time, value);
        }

        public void SampleAssetPrices(Symbol symbol, DateTime time, decimal value)
        {
            _shadow.SampleAssetPrices(symbol, time, value);
        }

        public void SampleRange(List<Chart> samples)
        {
            _shadow.SampleRange(samples);
        }

        public void SetAlgorithm(IAlgorithm algorithm, decimal startingPortfolioValue)
        {
            Algorithm = algorithm;
            _shadow.SetAlgorithm(algorithm, startingPortfolioValue);

        }

        public void StoreResult(Packet packet, bool async = false)
        {
            //do not save rounded results to disk
            //_shadow.StoreResult(packet, async);
        }

        public void SendStatusUpdate(AlgorithmStatus status, string message = "")
        {
            _shadow.SendStatusUpdate(status, message);
        }

        public void SetChartSubscription(string symbol)
        {
            _shadow.SetChartSubscription(symbol);
        }

        public void RuntimeStatistic(string key, string value)
        {
            _shadow.RuntimeStatistic(key, value);
        }

        public void OrderEvent(OrderEvent newEvent)
        {
            _shadow.OrderEvent(newEvent);
        }

        public void Exit()
        {
            _shadow.Exit();
        }

        public void PurgeQueue()
        {
            _shadow.PurgeQueue();
        }

        public void ProcessSynchronousEvents(bool forceProcess = false)
        {
            _shadow.ProcessSynchronousEvents(forceProcess);
        }

        public string SaveLogs(string id, IEnumerable<string> logs)
        {
            return _shadow.SaveLogs(id, logs);
        }

        public void SaveResults(string name, Result result)
        {
            //do not save rounded results to disk
            //_shadow.SaveResults(name, result);
        }

        public void SetAlphaRuntimeStatistics(AlphaRuntimeStatistics statistics)
        {
            _shadow.SetAlphaRuntimeStatistics(statistics);
        }

        public void Initialize(AlgorithmNodePacket job, IMessagingHandler messagingHandler, IApi api, ITransactionHandler transactionHandler)
        {
            _shadow.Initialize(job, messagingHandler, api, transactionHandler);
        }

        public void SetDataManager(IDataFeedSubscriptionManager dataManager)
        {
            _shadow.SetDataManager(dataManager);
        }
        #endregion
    }
}
