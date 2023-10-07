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
puts 1.object_id
puts 1.object_id
puts 2.3.object_id
puts 2.3.object_id
puts (1 + 1).object_id
puts (1 + 1).object_id
puts :hi.object_id
puts :hi.object_id

# puts Net::HTTP.get('example.com/index.html')

p :hi
p 5.class.class
p 5.class.name
p 5.class.class.name

p Integer.methods

a = ['a']
puts [a[0] + 0.5.to_s]
puts -1

class Thing
    
end
class SpecialThing < Thing
    
end

key = SpecialThing.new

puts Thing === key
puts Integer === 5
puts 5 === Integer
puts 2..3 === 2
puts key.is_a? Thing
puts key.instance_of? Thing

puts Class === Class

puts :hi === :hi
puts :he === :hi
puts :hi === :he

case 5
when 2..3
    puts '2..3'
when 6, 5, 4
    puts '6, 5 or 4'
end

p constants
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