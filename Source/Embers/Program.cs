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
puts ""Hello World"".hash
puts ""Hello World"".hash
puts ""Hello World 2"".hash
puts ""Hello World 2"".hash
puts ""Hello World"".object_id
puts ""Hello World"".object_id

class A
    def ==(value)
        true
    end
end
class B
    
end

z = A.new
b = {z => 1, B.new => 2}

p b[B.new]
p b[z]
p ({'a' => 2})['a']

p [1.0.eql?(1.0), 1.0.eql? 1]

puts '---'
p [0, 1, 2].hash
p [0, 1, 2].hash
p [0, 1, 2].hash == [0, 1, 2].hash # => true
p [0, 1, 2].hash == [0, 1, 3].hash # => false

puts '---'
p ({:foo => 0, :bar => 1, :baz => 2}).hash
p ({:foo => 0, :bar => 1, :baz => 2}).hash
p ({:foo => 0, :bar => 1, :baz => 2}).hash == {:foo => 0, :bar => 1, :baz => 2}.hash # => true
p ({:foo => 0, :bar => 1, :baz => 2}).hash == {:baz => 2, :bar => 1, :foo => 0}.hash # => true
p ({:foo => 0, :bar => 1, :baz => 2}).hash == {:baz => 2, :bar => 1}.hash # => false
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