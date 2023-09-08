using System.Diagnostics;
using static Embers.Script;

namespace Embers
{
    internal class Program
    {
        static void Main() {
            // Test
            {
                Interpreter Interpret = new();
                Script Script = new(Interpret);
                Benchmark(() => 
                    Script.Evaluate(@"
5.times do |n = 'bruh', m = 'lol'|
    puts n, m
end

puts '---'

[4, 2, 5].each do |n, m|
    print n, ' ', m, ""\n""
end

puts '---'

arr = [1, 2, 3, 4, 5]
puts arr.map {|a| 2*a}

puts '---'

# Find three numbers that multiply to make 1230.
Num = 1230
loop do
    num1 = rand(Num)
    num2 = rand(Num)
    num3 = rand(Num)
    if num1 * num2 * num3 == Num
        puts ""The numbers #{num1}, #{num2} and #{num3} work.""
        break
    end
end
                    ")
                );
                Console.ReadLine();
            }
            // Benchmark
            {
                Interpreter Interpret = new();
                Script Script = new(Interpret);
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