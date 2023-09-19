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
puts -1, 2
puts 4 -1, 2

def a b
    3
end

puts a -1
puts 1 -1
puts 1 - 1

a = -1
b = - 2
c = (- 3.4)
puts a, b, c

z = 'c'
if true
    u = 'b'
end
p defined? z
p defined? u
=end

def abc
    2.times do
        puts '1'
        2.times do
            puts '2'
            yield
        end
    end
end
abc {puts '3'}

puts Time.new
puts Time.new 2023, 09, 19, 16, 38
puts Time.new 2023, 09, 19, 16, 38, 5, +5
puts Time.now
puts Time.now.to_i
puts Time.now.to_f
puts Time.at 140293586
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