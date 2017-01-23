using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class AppDomainManager
    {

        static AppDomainSetup _ads;
        static string _exeAssembly;
        static Dictionary<string, decimal> _results;
        static object _resultsLocker;
        static OptimizerConfiguration _config;

        public static void Initialize(OptimizerConfiguration config)
        {
            _config = config;
            _results = new Dictionary<string, decimal>();
            _ads = SetupAppDomain();
            _resultsLocker = new object();
        }

        static AppDomainSetup SetupAppDomain()
        {
            // Get and display the full name of the EXE assembly.
            _exeAssembly = Assembly.GetEntryAssembly().FullName;

            // Construct and initialize settings for a second AppDomain.
            AppDomainSetup ads = new AppDomainSetup();
            ads.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;

            ads.DisallowBindingRedirects = false;
            ads.DisallowCodeDownload = true;
            ads.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            return ads;
        }

        static Runner CreateRunClassInAppDomain(ref AppDomain ad)
        {
            // Create the second AppDomain.
            var name = Guid.NewGuid().ToString("x");
            ad = AppDomain.CreateDomain(name, null, _ads);

            // Create an instance of MarshalbyRefType in the second AppDomain. 
            // A proxy to the object is returned.
            Runner rc = (Runner)ad.CreateInstanceAndUnwrap(_exeAssembly, typeof(Runner).FullName);

            ad.SetData("Results", _results);
            ad.SetData("AlgorithmTypeName", _config.AlgorithmTypeName);
            if (!string.IsNullOrEmpty(_config.AlgorithmLocation))
            {
                ad.SetData("AlgorithmLocation", Path.GetFileName(_config.AlgorithmLocation));
            }

            return rc;
        }

        public static double RunAlgorithm(Dictionary<string, object> list)
        {

            AppDomain ad = null;
            Runner rc = CreateRunClassInAppDomain(ref ad);
            string output = "";

            foreach (var item in list)
            {
                output += item.Key + ": " + item.Value.ToString() + ", ";
            }

            var sharpe = (double)rc.Run(list);
            output += string.Format("sharpe: {0}", sharpe);
            Program.Output(output);

            lock (_resultsLocker)
            {
                foreach (var item in (Dictionary<string, decimal>)ad.GetData("Results"))
                {
                    if (!_results.ContainsKey(item.Key))
                    {
                        _results.Add(item.Key, item.Value);
                    }
                }
            }

            AppDomain.Unload(ad);

            return sharpe;
        }

    }
}
