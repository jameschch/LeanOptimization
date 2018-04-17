using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Optimization.Toolbox.OptimizerLogToCsv
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("***********************************************************************************************************************");
            Console.WriteLine("Extracts all generations alpha chromosomes into csv.\nUsage: OptimizerLogToCsv logPath csvPath");
            Console.WriteLine("***********************************************************************************************************************");

            if (args == null || !args.Any())
            {
                args = new[] {
                    Path.Combine(System.Environment.GetEnvironmentVariable("USERPROFILE"), @"Source\Repos\LeanOptimization\Optimization\bin\Debug\optimizer.txt"),
                    Path.Combine(System.Environment.GetEnvironmentVariable("USERPROFILE"), @"Source\Repos\LeanOptimization\Optimization\bin\Debug\optimization.csv") };
            }

            string path = args[0];
            string headerLine = "";
            bool hasHeader = false;

            using (StreamWriter writer = new StreamWriter(args[1]))
            {
                foreach (var line in File.ReadAllLines(path))
                {
                    string csvLine = "";
                    if (line.Contains(", Generation: "))
                    {
                        var split = line.Split(',').Skip(1);
                        foreach (var field in split)
                        {
                            var fieldSplit = field.Split(":");
                            headerLine += fieldSplit[0] + ",";
                            csvLine += fieldSplit[1] + ",";
                        }
                        if (!hasHeader)
                        {
                            writer.Write(headerLine + "\n");
                            hasHeader = true;
                        }
                        writer.Write(csvLine + "\n");
                    }
                }
            }

            Console.ReadKey();
        }
    }
}
