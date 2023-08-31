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
def a b, c
    return b.to_f + c.to_f
end
puts a '75', '23' if true

puts(a '23', a('65', '65').to_s) if true

File.write('something.txt', 'Hi!')
puts File.read 'something.txt'
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