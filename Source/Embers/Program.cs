using System.Diagnostics;
using static Embers.Script;

namespace Embers
{
    internal class Program
    {
        static void Main() {
            /*// Test
            {
                Interpreter Interpreter = new();
                Script Script = new(Interpreter);
                Benchmark(() => {
                    Script.Evaluate(@"
=begin
Parallel.each [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] do |n|
    print n.to_s + ' '
end
getc
puts ""\n---""
[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].each do |n|
    Thread.new {
        print n.to_s + ' '
    }.start
end
getc
puts ""\n---""
=end

a = false ? 1 : -1
puts a

puts '---'

class B
    def a
        puts 'original'
    end
end
class A < B
    def a
        super
    end
end
A.new.a

puts Integer.is_a? Integer
puts 5.is_a? Integer
puts 5.instance_of? Integer

puts 5 + ""Hello World"" rescue puts ""You can't add numbers and strings""

a = 'Hi'
p defined? a
p defined? b
p defined? A.new.a
p defined? A.new.b

begin
    alias puts_alias puts
end
puts_alias 'Aliased!'
                    ");
                });
                Script.WaitForThreads();
                Console.WriteLine("Done.");
                Console.ReadLine();
            }*/
            // Test 2
            {
                Interpreter Interpreter = new();
                Script Script = new(Interpreter);
                Benchmark(() => {
                    Script.Evaluate(@"
=begin
puts 5 === (2..7)
puts (2..7) === 5
puts (2..7) === (2..7)
puts (2..7) === 5.5

case 500
when 1..8
    puts 'Between 1 and 8'
when 499..501
    puts 'Between 499 and 501'
else
    puts 'Who knows'
end
=end

class A
    def puts
        super 'Hi'
    end
end
A.new.puts
puts 'heya'
                    ");
                });
                Script.WaitForThreads();
                Console.WriteLine("Done.");
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