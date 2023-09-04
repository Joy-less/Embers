using System.Diagnostics;
using static Embers.Interpreter;

namespace Embers
{
    internal class Program
    {
        static void Main() {
            {
                string Code = @"
puts 2
def say
    yield
end
# puts say { |k| puts k }
# say {; puts 'k'; }
# puts say do; puts 'k'; end
# say do; puts 'hi'; end
say do
    puts 'hiya'
end

5.times do
    puts 'Countdown'
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