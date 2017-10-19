using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Optimization.Toolbox.OptimizerLogToJson
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("***********************************************************************************************************************");
            Console.WriteLine("Converts the last generation alpha chromosome from supplied optimizer.log to json or C# dictionary.\nUsage: OptimizerLogToJson path separator");
            Console.WriteLine("***********************************************************************************************************************");

            if (args == null || !args.Any())
            {
                args = new[] { Path.Combine(System.Environment.GetEnvironmentVariable("USERPROFILE"), @"Source\Repos\LeanOptimization\Optimization\bin\Debug\optimizer.txt"), "," };
            }

            string path = args[0];

            var lines = File.ReadAllLines(path);
            string line = null;
            for (var i = lines.Length - 1; i > 0; i--)
            {
                line = lines.ElementAt(i);

                if (line.Contains(", Generation: "))
                {
                    break;
                }
            }

            var split = line.Split(',');

            var reformatted = split.Skip(4).Where(i => !i.StartsWith(" Id")).Select(s => Replace(s, args[1]));

            string output = "";
            foreach (var item in reformatted)
            {
                Console.Write(item);
            }
            
            Console.ReadKey();
        }

        private static string Replace(string entry, string separator)
        {
            entry = entry.Trim().Replace(":", $"\"{separator}");
            entry = "{\"" + entry + "},\n";

            return entry;
        }


    }
}
