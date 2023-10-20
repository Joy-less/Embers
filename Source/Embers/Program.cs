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
=begin
class A
    def method_missing(method, *args)
        p method, args
    end
end

A.new.a 'hi'


class Test
  def initialize
  end
  def test_test
    5
  end
end

b = WeakRef.new(Test.new)
z = ['a', 'b', 'c']
while true
    z *= 2
    sleep 0.1
    puts b.weakref_alive?
end
=end

=begin
puts (true ? 5 : 2) + 6

res = Net::HTTP.get('httpbin.org/')
puts res.class
puts 'body length: ' + res.body.length.to_s
puts 'code: ' + res.code.to_s

puts 5.abs
puts -5.abs
puts Math.abs -100
puts Math.abs 100
puts 5.0.abs
puts -5.0.abs
puts Math.abs -100.0
puts Math.abs 100.0

class Z
def y
6
end
end
puts -Z.new.y
=end

a = [2, 4, 6, 7, 3, 5, 1, 9, 0, 8, 4.5]
a.sort!
p a
p a.shuffle
p a.shuffle.sort

start_time = Time.now.to_f
2_000.times do
    a = [2, 4, 6, 7, 3, 5, 1, 9, 0, 8, 4.5]
    a.sort!
end
puts (Time.now.to_f - start_time).to_s + ' seconds'

start_time = Time.now.to_f
10.times do
    a = ((0..10).to_a * 10).shuffle
    a.sort!
end
puts (Time.now.to_f - start_time).to_s + ' seconds'

z = [1, 2, 4, 2]
z.sort
p z
z.sort!
p z
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