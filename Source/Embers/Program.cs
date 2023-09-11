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
begin
    raise StandardError.new('hi')
rescue Exception => ex
    puts ""rescued #{ex.class}""
else
    puts 'Runs if not rescued'
ensure
    puts 'Runs regardless'
end

begin
    
rescue Exception => ex
    puts ""rescued #{ex.class}""
else
    puts 'Runs if not rescued'
ensure
    puts 'Runs regardless'
end
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