using GAF;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QuantConnect.Api;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Lean.Engine.HistoricalData;
using QuantConnect.Lean.Engine.RealTime;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Lean.Engine.Setup;
using QuantConnect.Lean.Engine.TransactionHandlers;
using QuantConnect.Logging;
using QuantConnect.Messaging;
using QuantConnect.Packets;
using QuantConnect.Queues;
using QuantConnect.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class Runner : MarshalByRefObject
    {

        private Api _api;
        private Messaging _notify;
        private JobQueue _jobQueue;
        private IResultHandler _resultshandler;
        private FileSystemDataFeed _dataFeed;
        private ConsoleSetupHandler _setup;
        private BacktestingRealTimeHandler _realTime;
        private ITransactionHandler _transactions;
        private IHistoryProvider _historyProvider;
        Dictionary<string, decimal> results = new Dictionary<string, decimal>();

        public decimal Run(IEnumerable<Gene> items)
        {
            string hash = JsonConvert.SerializeObject(items, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { ContractResolver = new GeneContractResolver() });

            if (results.ContainsKey(hash))
            {
                return results[hash];
            }


            foreach (var item in items)
            {
                var pair = (KeyValuePair<string, object>)item.ObjectValue;
                Config.Set(pair.Key, pair.Value.ToString());
            }

            LaunchLean();
            BacktestingResultHandler resultshandler = (BacktestingResultHandler)_resultshandler;
            var sharpe_ratio = -10m;
            var ratio = resultshandler.FinalStatistics["Sharpe Ratio"];
            Decimal.TryParse(ratio, out sharpe_ratio);

            sharpe_ratio = System.Math.Max(sharpe_ratio == 0 ? -10 : sharpe_ratio, -10);

            results.Add(hash, sharpe_ratio);

            return sharpe_ratio;
        }

        private void LaunchLean()
        {
            Config.Set("environment", "backtesting");
            string algorithm = ConfigurationManager.AppSettings["algorithmTypeName"];

            Config.Set("algorithm-type-name", algorithm);

            _jobQueue = new JobQueue();
            _notify = new Messaging();
            _api = new Api();
            _resultshandler = new DesktopResultHandler();
            _dataFeed = new FileSystemDataFeed();
            _setup = new ConsoleSetupHandler();
            _realTime = new BacktestingRealTimeHandler();
            _historyProvider = new SubscriptionDataReaderHistoryProvider();
            _transactions = new BacktestingTransactionHandler();
            var systemHandlers = new LeanEngineSystemHandlers(_jobQueue, _api, _notify);
            systemHandlers.Initialize();

            //			var algorithmHandlers = new LeanEngineAlgorithmHandlers (_resultshandler, _setup, _dataFeed, _transactions, _realTime, _historyProvider);
            Log.LogHandler = Composer.Instance.GetExportedValueByTypeName<ILogHandler>(Config.Get("log-handler", "CompositeLogHandler"));

            LeanEngineAlgorithmHandlers leanEngineAlgorithmHandlers;
            try
            {
                leanEngineAlgorithmHandlers = LeanEngineAlgorithmHandlers.FromConfiguration(Composer.Instance);
                _resultshandler = leanEngineAlgorithmHandlers.Results;
            }
            catch (CompositionException compositionException)
            {
                Log.Error("Engine.Main(): Failed to load library: " + compositionException);
                throw;
            }
            string algorithmPath;
            AlgorithmNodePacket job = systemHandlers.JobQueue.NextJob(out algorithmPath);
            try
            {
                var _engine = new Engine(systemHandlers, leanEngineAlgorithmHandlers, Config.GetBool("live-mode"));
                _engine.Run(job, algorithmPath);
            }
            finally
            {
                //Delete the message from the job queue:
                //systemHandlers.JobQueue.AcknowledgeJob(job);
                Log.Trace("Engine.Main(): Packet removed from queue: " + job.AlgorithmId);

                // clean up resources
                systemHandlers.Dispose();
                leanEngineAlgorithmHandlers.Dispose();
                Log.LogHandler.Dispose();
            }
        }

        public class GeneContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);
                if (property.PropertyType == typeof(Guid))
                {
                    property.Ignored = true;
                }
                return property;
            }
        }

    }
}
