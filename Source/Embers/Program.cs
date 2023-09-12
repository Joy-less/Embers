using System.Diagnostics;
using static Embers.Script;

namespace Embers
{
    internal class Program
    {
        static void Main() {
            /*// Test
            {
                Interpreter Interpreter = new();
                Script Script = new(Interpreter);
                Benchmark(() => {
                    _ = Script.EvaluateAsync(@"
puts 'a'
sleep(10)
puts 'b'
                    ");
                    Thread.Sleep(3000);
                    Script.Stop();
                });
                Console.ReadLine();
            }
            // Test 2
            {
                Interpreter Interpreter = new();
                Script Script = new(Interpreter);
                Task.Run(async () => await Script.EvaluateAsync(@"
t = Thread.new {
    sleep(1)
    puts 'a'
}
t.join
puts 'b'

t2 = Thread.new {
    sleep(1)
    puts 'a'
}
t2.start
puts 'b'
                "));
                Thread.Sleep(300);
                Script.Stop();
                Console.WriteLine("Stopped!");
                Console.ReadLine();
            }*/
            // Test 3
            {
                Interpreter Interpreter = new();
                Script Script = new(Interpreter);
                Benchmark(() => {
                    Script.Evaluate(@"
t = Thread.new {
    for i in 1..1_000_000
        b = i + 1
    end
}
t.start_parallel
                    ");
                });
                Script.WaitForThreads();
                Console.WriteLine("Done.");
                Console.ReadLine();
            }
            // Benchmark
            {
                Interpreter Interpreter = new();
                Script Script = new(Interpreter);
                Benchmark(() => 
                    Script.Evaluate("250_000_000.times do \n end")
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