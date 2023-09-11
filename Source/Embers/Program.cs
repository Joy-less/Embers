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
class A
    def a=(value)
        @a = value
    end
    attr_accessor :a
end
b = A.new
b.a = 'Some'
b.a += 'thing'
puts b.a

class Z
    def +(value)
        'Zzz' + value
    end
    def <=(value)
        'No idea'
    end
end
y = Z.new
puts y + '...'
puts y <= 5

puts 'Less than or equal to result: ' + (2 <= 3.15).to_s
puts 'Spaceship result: ' + (2 <=> 3.15).to_s
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