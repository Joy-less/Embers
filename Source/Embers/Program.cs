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

puts (true ? 5 : 2) + 6
=end

res = Net::HTTP.get('httpbin.org/')
puts res.class
puts 'body length: ' + res.body.length.to_s
puts 'code: ' + res.code.to_s
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