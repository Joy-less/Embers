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
puts ""a"" if true
puts ""b"" if 1
puts ""c"" if false
puts ""d"" if nil
puts ""e"" if ""true""
puts ""f"" if 0
";
                Benchmark(() => new Interpreter().Evaluate(Code));
                Console.ReadLine();
            }

            // Benchmark
            {
                Interpreter Interpret = new();
                Benchmark(() => {
                    Interpret.Evaluate("250000000.times do \n end");
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