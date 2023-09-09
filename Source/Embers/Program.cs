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
p ' hiii '.strip
p ' hiii '.lstrip
p ' hiii '.rstrip
p ' hiii '.squeeze
p ' hiii '.chop
p ' hiii '.chr
p 'hIII'.capitalize
p 'hIII'.upcase
p 'hIII'.downcase
p 'cat cat'.sub('at', 'orkscrew')
p 'cat cat'.gsub('at', 'orkscrew')
p 'cat cat'.to_a

p eval('puts \'Evaluated\'; 5')

hash = {1 => 'one', 2 => 'two'}
p hash.invert
p hash.keys
p hash.values
p hash.to_a
p hash.to_hash
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