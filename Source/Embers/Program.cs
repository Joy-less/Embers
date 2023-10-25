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
#for i in 1..1_000_000
    
#end

class A
    def inspect
        'lol'
    end
end
class B
end
p A.new
p B.new

p 'Hello there  lol'.split
p 'Hello there  lol'.split 'l'
p 'Hello there  lol'.split [' ', 'l']
p 'Hello there  lol'.split nil, false
p 'Hello there  lol'.split 'l', false
p 'Hello there  lol'.split [' ', 'l'], false

a = [1, 2, 3]
p a.prepend 0
p a.append 4
p a.pop
p a
p [].prepend 0
p [].pop

p [1, 'hi', true, ['z', ['a', 'b'], 'y']].join ', '
p [1, 'hi', true, ['z', ['a', 'b'], 'y']].join
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