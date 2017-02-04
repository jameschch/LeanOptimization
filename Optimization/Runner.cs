using QuantConnect.Configuration;
using QuantConnect.Lean.Engine;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Logging;
using QuantConnect.Packets;
using QuantConnect.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace Optimization
{

    public class Runner : MarshalByRefObject
    {

        private BacktestingResultHandler _resultsHandler;
        IOptimizerConfiguration _config;

        public Dictionary<string, string> Run(Dictionary<string, object> items)
        {
            string plain = string.Join(",", items.Select(s => s.Value));

            Dictionary<string, Dictionary<string, string>> results = AppDomainManager.GetResults(AppDomain.CurrentDomain);
            _config = AppDomainManager.GetConfig(AppDomain.CurrentDomain);

            if (results.ContainsKey(plain))
            {
                return results[plain];
            }

            foreach (var pair in items)
            {
                Config.Set(pair.Key, pair.Value.ToString());
            }

            LaunchLean();

            results.Add(plain, _resultsHandler.FinalStatistics);
            AppDomainManager.SetResults(AppDomain.CurrentDomain, results);

            return _resultsHandler.FinalStatistics;
        }

        private void LaunchLean()
        {
            Config.Set("environment", "backtesting");
            string algorithm = _config.AlgorithmTypeName;
            string path = _config.AlgorithmLocation;

            Config.Set("algorithm-type-name", algorithm);
            if (!string.IsNullOrEmpty(path))
            {
                Config.Set("algorithm-location", Path.GetFileName(path));
            }
            var systemHandlers = LeanEngineSystemHandlers.FromConfiguration(Composer.Instance);
            systemHandlers.Initialize();

            Log.LogHandler = Composer.Instance.GetExportedValueByTypeName<ILogHandler>(Config.Get("log-handler", "CompositeLogHandler"));
            //Log.DebuggingEnabled = false;
            //Log.DebuggingLevel = 1;

            LeanEngineAlgorithmHandlers leanEngineAlgorithmHandlers;
            try
            {
                leanEngineAlgorithmHandlers = LeanEngineAlgorithmHandlers.FromConfiguration(Composer.Instance);
                _resultsHandler = (BacktestingResultHandler)leanEngineAlgorithmHandlers.Results;
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
                Log.Trace("Engine.Main(): Packet removed from queue: " + job.AlgorithmId);

                // clean up resources
                systemHandlers.Dispose();
                leanEngineAlgorithmHandlers.Dispose();
                Log.LogHandler.Dispose();
            }
        }

    }
}
