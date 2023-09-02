using System.Diagnostics;
using static Embers.Interpreter;

namespace Embers
{
    internal class Program
    {
        static void Main() {
            {
                string Code = @"
class A

end

if 2.5 == 3.2
    puts 'a'
elsif 3.0 == 3
    puts 'b'
else
    puts 'c'
end
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