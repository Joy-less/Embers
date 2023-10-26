using System;
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

p __LINE__
p __FILE__
ttt = Time.now.to_f
for i in 1..1000

end
p Time.now.to_f - ttt

avg = []
for i in 1..50
    t = Time.now.to_f
    i = 0
    y = 1.0 / 60.0
    while Time.now.to_f - t < y
        i += 1
    end
    avg << i
    sleep 0.01
end
p avg.sum / avg.count
                    ")
                );
                Console.ReadLine();
            }
            // Benchmark
            {
                Interpreter Interpreter = new();
                Script Script = new(Interpreter);
                Benchmark(() =>
                    Script.Evaluate("1_000_000.times do end")
                );
                Console.ReadLine();
            }
        }
        static void Benchmark(Action Code, int Times = 1) {
            Stopwatch Stopwatch = new();
            Stopwatch.Start();
            for (int i = 0; i < Times; i++)
                Code();
            Stopwatch.Stop();
            Console.WriteLine($"Took {Stopwatch.ElapsedMilliseconds / 1000d} seconds");
        }
    }
}