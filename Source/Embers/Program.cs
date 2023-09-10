using System.Diagnostics;
using static Embers.Script;

namespace Embers
{
    internal class Program
    {
        static void Main() {
            // Test
            {
                Interpreter Interpreter = new();
                Script Script = new(Interpreter);
                Benchmark(() => 
                    Script.Evaluate(@"
puts Math::PI
puts Math::E
p Math.frexp(1234) # [0.6025390625, 11]
p Math.ldexp(0.6025390625, 11) # 1234.0
p Math.frexp(0) # [0.0, 0]
p Math.erf(2.1) # ~0.997020533343667
p Math.gamma(5) # ~24.0
p Math.lgamma(2.5) # [~0.2846828704729205, 1]
p Math.hypot(5, 12) # 13.0
                    ")
                );
                Console.ReadLine();
            }
            // Benchmark
            {
                Interpreter Interpreter = new();
                Script Script = new(Interpreter);
                Benchmark(() => 
                    Script.Evaluate("250000000.times do \n end")
                );
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