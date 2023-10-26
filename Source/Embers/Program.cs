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
def fibonacci(n)
  if n <= 1
    return n
  else
    return fibonacci(n - 1) + fibonacci(n - 2)
  end
end

p __FILE__

# Calculate the Fibonacci sequence
n = 22
puts ""Fibonacci sequence for n = #{n}:""

p 0..n
p (0..n).to_a

puts ""Processor count: #{Parallel.processor_count}""

puts 'PARALLEL'
t = Time.now.to_f
Parallel.each((0..n).to_a) { |i| puts ""#{i}. #{fibonacci(i)}"" }
p Time.now.to_f - t
puts 'SEQUENCE'
t = Time.now.to_f
(0..n).each { |i| puts ""#{i}. #{fibonacci(i)}"" }
p Time.now.to_f - t
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