using System.Diagnostics;
using static Embers.Interpreter;

namespace Embers
{
    internal class Program
    {
        static void Main() {
            {
                string Code = @"
p :'hi '
p :hi
p 'Hi'.to_sym
p :hey.to_s
p '5'.to_sym
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