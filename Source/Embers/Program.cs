using System.Diagnostics;
using static Embers.Script;

namespace Embers
{
    internal class Program
    {
        static void Main() {
            // Test
            {
                Interpreter Interpret = new();
                Script Script = new(Interpret);
                Benchmark(() => 
                    Script.Evaluate(@"
a = [4, 7, 3]
p a[1] # 7
p a.count # 3
p a.length # 3
p a.count 3 # 1
p a.first # 4
p a.last # 3
p a.sample # ?
puts '---'
b = [3, 4, 5]
p b.insert 1, 'Hi', 'There'
p b
                    ")
                );
                Console.ReadLine();
            }
            // Benchmark
            {
                Interpreter Interpret = new();
                Script Script = new(Interpret);
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