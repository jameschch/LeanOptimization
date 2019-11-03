using System;
using NLog;

namespace Optimization
{

    public class Program
    {

        public static Logger OptimizerLogger = LogManager.GetLogger("optimizer");
        public static Logger GenerationsLogger = LogManager.GetLogger("generations");
        public static Logger ErrorLogger = LogManager.GetLogger("error");

        public static void Main(string[] args)
        {
            try
            {
                new OptimizerInitializer().Initialize(args);

            }
            catch (Exception ex)
            {
                ErrorLogger.Error(ex);
                throw new Exception("Unhandled Exception", ex);
            }
            Console.ReadKey();
        }

    }
}