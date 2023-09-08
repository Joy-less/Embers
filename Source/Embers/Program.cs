﻿using System.Diagnostics;
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
unless false then
    puts 'Yes'
else
    puts 'No'
end

_52 = 5_0_4.2_6
puts _52
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