﻿using System.Diagnostics;
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
            /*// Test 2
            {
                Interpreter Interpreter = new();
                Script Script = new(Interpreter);
                Benchmark(() => {
                    Script.Evaluate(@"
puts 5.243567.round
puts 5.243567.round 1
puts 5.243567.round 0
puts 5.243567.round 20
puts 565464.243567.round -1
puts 565464.243567.round -20
puts 123456.7.round 1

puts 5.in? [2, 5, 7]
puts 5.in? [2, 6, 7]

puts 5.ceil

p 123.to_s.to_a.map {|n| n.to_i}.reverse

puts Float::INFINITY
puts -Float::INFINITY

class MyClass
    def a
        puts 'a'
    end
    def self.b
        puts 'b'
    end
end

obj1 = MyClass.new
obj2 = MyClass.new

puts obj1.inspect
puts obj2.inspect

MyClass.new.a
MyClass.b
puts Math.method :sqrt
puts 5.is_a? Integer
puts Integer.is_a? Integer
p Integer.methods

for i, v in {:a => :b}
    p i, v
end

c = (a, b = 1, 2)
p a, b
p c

a, b = [5, 6]
p a, b
                    ");
                });
                Script.WaitForThreads();
                Console.WriteLine("Done.");
                Console.ReadLine();
            }*/
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