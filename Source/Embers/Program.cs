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
Parallel.each [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] do |n|
    print n.to_s + ' '
end
getc
puts ""\n---""
[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].each do |n|
    Thread.new {
        print n.to_s + ' '
    }.start
end
getc
puts ""\n---""
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