using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Accord.MachineLearning;

namespace Optimization
{

    public class SingleAppDomainManager : OptimizerAppDomainManager
    {

        static AppDomainSetup _ads;
        private static AppDomain _ad;

        public new static void Initialize()
        {
            _ads = SetupAppDomain();
            _ad = AppDomain.CreateDomain(Guid.NewGuid().ToString("x"), null, _ads);
            SetResults(_ad, new Dictionary<string, Dictionary<string, decimal>>());
        }

        static SingleRunner CreateRunnerInAppDomain()
        {
            var rc = (SingleRunner)_ad.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(SingleRunner).FullName);

            return rc;
        }

        public new static Dictionary<string, decimal> RunAlgorithm(Dictionary<string, object> list, IOptimizerConfiguration config)
        {
            var rc = CreateRunnerInAppDomain();

            var result = rc.Run(list, config);

            return result;
        }

        public new static Dictionary<string, Dictionary<string, decimal>> GetResults()
        {
            return SingleAppDomainManager.GetData<Dictionary<string, Dictionary<string, decimal>>>(_ad, "Results");
        }

    }

}
