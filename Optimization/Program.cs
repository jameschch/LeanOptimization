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
            new OptimizerInitializer().Initialize(args);

            Console.ReadKey();
        }

    }
}