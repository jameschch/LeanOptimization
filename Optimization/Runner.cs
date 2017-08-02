using QuantConnect.Configuration;
using QuantConnect.Lean.Engine;
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

        private OptimizerResultHandler _resultsHandler;
        IOptimizerConfiguration _config;

        public Dictionary<string, decimal> Run(Dictionary<string, object> items)
        {
            Dictionary<string, Dictionary<string, decimal>> results = OptimizerAppDomainManager.GetResults(AppDomain.CurrentDomain);
            _config = OptimizerAppDomainManager.GetConfig(AppDomain.CurrentDomain);

            if (_config.StartDate.HasValue && _config.EndDate.HasValue)
            {
                if (!items.ContainsKey("startDate")) { items.Add("startDate", _config.StartDate); }
                if (!items.ContainsKey("endDate")) { items.Add("endDate", _config.EndDate); }
            }

            string plain = string.Join(",", items.Select(s => s.Value));

            if (results.ContainsKey(plain))
            {
                return results[plain];
            }

            foreach (var pair in items)
            {
                if (pair.Value is DateTime?)
                {
                    var cast = ((DateTime?)pair.Value);
                    if (cast.HasValue)
                    {
                        Config.Set(pair.Key, cast.Value.ToString("O"));
                    }
                }
                else
                {
                    Config.Set(pair.Key, pair.Value.ToString());
                }
            }

            LaunchLean();

            results.Add(plain, _resultsHandler.FullResults);
            OptimizerAppDomainManager.SetResults(AppDomain.CurrentDomain, results);
 
            return _resultsHandler.FullResults;
        }

        private void LaunchLean()
        {
            Config.Set("environment", "backtesting");

            if (!string.IsNullOrEmpty(_config.AlgorithmTypeName))
            {
                Config.Set("algorithm-type-name", _config.AlgorithmTypeName);
            }

            if (!string.IsNullOrEmpty(_config.AlgorithmLocation))
            {
                Config.Set("algorithm-location", Path.GetFileName(_config.AlgorithmLocation));
            }

            if (!string.IsNullOrEmpty(_config.DataFolder))
            {
                Config.Set("data-folder", _config.DataFolder);
            }

            var systemHandlers = LeanEngineSystemHandlers.FromConfiguration(Composer.Instance);
            systemHandlers.Initialize();

            //separate log now uniquely named
            var logFileName = "log" + DateTime.Now.ToString("yyyyMMddssfffffff");
            if (File.Exists(logFileName + ".txt"))
            {
                logFileName += "_" + Guid.NewGuid().ToString();
            }
            logFileName += ".txt";

            var logHandlers = new ILogHandler[] { new FileLogHandler(logFileName, true) };
            Log.Trace("Initializing log.");

            using (Log.LogHandler = new CompositeLogHandler(logHandlers))
            {
                LeanEngineAlgorithmHandlers leanEngineAlgorithmHandlers;
                try
                {
                    //override config to use custom result handler
                    Config.Set("backtesting.result-handler", nameof(OptimizerResultHandler));
                    leanEngineAlgorithmHandlers = LeanEngineAlgorithmHandlers.FromConfiguration(Composer.Instance);                   
                    _resultsHandler = (OptimizerResultHandler)leanEngineAlgorithmHandlers.Results;
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
                }
            }
        }

    }
}
