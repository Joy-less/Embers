using System.Diagnostics;
using static Embers.Interpreter;
using static Embers.Phase2;

namespace Embers
{
    internal class Program
    {
        static void Main() {
            {
                string Code = @"
puts 'Hello, world!'
";
                Benchmark(() => new Interpreter().Evaluate(Code));
                Console.ReadLine();
            }

            // Benchmark
            {
                Interpreter Interpret = new();
                Benchmark(() => {
                    // Interpret.Evaluate("250000000.times do \n end");
                    Interpret.Evaluate("100000.times do \n a = 3 + 2 / 4 ** 2 \n end");
                });
                Console.ReadLine();
            }
        }
        static void Benchmark(Action Code, int Times = 1) {
            Stopwatch Stopwatch = new();
            Stopwatch.Start();
            if (Times == 1)
                Code();
            else
                for (int i = 0; i < Times; i++)
                    Code();
            Stopwatch.Stop();
            Console.WriteLine($"Took {Stopwatch.ElapsedMilliseconds / 1000d} seconds");
        }
    }
}