using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class OptimizerAppDomainManager
    {

        static AppDomainSetup _ads;
        static Dictionary<string, Dictionary<string, string>> _results;
        static object _resultsLocker;
        static IOptimizerConfiguration _config;

        public static void Initialize(IOptimizerConfiguration config)
        {
            _config = config;
            _results = new Dictionary<string, Dictionary<string, string>>();
            _ads = SetupAppDomain();
            _resultsLocker = new object();
        }

        static AppDomainSetup SetupAppDomain()
        {
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
            Runner rc = (Runner)ad.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(Runner).FullName);

            SetResults(ad, _results);
            SetConfig(ad, _config);

            return rc;
        }

        public static Dictionary<string, string> RunAlgorithm(Dictionary<string, object> list)
        {
            AppDomain ad = null;
            Runner rc = CreateRunClassInAppDomain(ref ad);       

            var result = (Dictionary<string, string>)rc.Run(list);
            
            lock (_resultsLocker)
            {
                foreach (var item in GetResults(ad))
                {
                    if (!_results.ContainsKey(item.Key))
                    {
                        _results.Add(item.Key, item.Value);
                    }
                }
            }

            AppDomain.Unload(ad);

            return result;
        }

        public static Dictionary<string, Dictionary<string, string>> GetResults(AppDomain ad)
        {
            return GetData<Dictionary<string, Dictionary<string, string>>>(ad, "Results");
        }

        public static IOptimizerConfiguration GetConfig(AppDomain ad)
        {
            return GetData<IOptimizerConfiguration>(ad, "Config");
        }

        public static T GetData<T>(AppDomain ad, string key)
        {
           return (T)ad.GetData(key);
        }

        public static void SetResults(AppDomain ad, object item)
        {
            SetData(ad, "Results", item);
        }

        public static void SetConfig(AppDomain ad, object item)
        {
            SetData(ad, "Config", item);
        }

        public static void SetData(AppDomain ad, string key, object item)
        {
            ad.SetData(key, item);
        }

    }

}
