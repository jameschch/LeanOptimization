using System;
using NLog;

namespace Optimization
{

    public class Program
    {

        public static void Main(string[] args)
        {
            try
            {
                new OptimizerInitializer().Initialize(args);

            }
            catch (Exception ex)
            {
                LogProvider.ErrorLogger.Error(ex);
                throw new Exception("Unhandled Exception", ex);
            }
            Console.ReadKey();
        }

    }
}