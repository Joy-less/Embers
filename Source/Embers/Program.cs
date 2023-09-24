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
does = is = {true => 'Yes', false => 'No'}
puts does[10 == 50] # No
puts is[10 > 5] # Yes

year = 1982
puts case year
    when 1970..1979; 'Seventies'
    when 1980..1989; 'Eighties'
    when 1990..1999; 'Nineties'
end

class Thing
    
end
a = 'hi '
b = a.clone
def a.something
    
end
p defined? a.something
p defined? b.something
p defined? 'hey'.something
p a, b
p a.object_id, b.object_id

p Thing.clone
                    ")
                );
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