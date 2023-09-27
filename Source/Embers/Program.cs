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
a = [1, 2]
p [a[0], a[1]]

5.times do |n|
    puts n
end

=begin
puts 0x5050f
puts 509654806985379835687935687356078250650268724807257507846092785637856098
puts 509654806985379835687935687356078250650268724807257507846092785637856098.7653735686496859785095780
puts 509654806985379835687935687356078250650268724807257507846092785637856098 + 1
p (509654806985379835687935687356078250650268724807257507846092785637856098 / 2.4).to_s
=end

p '1000 + 9223372036854775801'
p 1000 + 9223372036854775801
p 1000.0 + 9223372036854775801.0
p 1000.0 + 9223372036854775801.0 - 9223372036854775801.0

puts [3, 6, 2, 4, 6].min
puts [3, 6, 2, 4, 6].max

puts (1...3).count

c = 'Cat'

puts 'Parallel.times(5):'
Parallel.times 5 do |n|
    puts n
end
puts 'Parallel.times(5..7):'
Parallel.times 5..7 do |n|
    puts n
end

puts Float::INFINITY + 922337203685477580100000

$lol = 'hi'

p local_variables, global_variables
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