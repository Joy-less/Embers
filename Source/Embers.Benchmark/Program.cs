﻿namespace Embers.Benchmark {
    public class Program {
        public static void Main() {
            BenchmarkDotNet.Running.BenchmarkRunner.Run<RubyBenchmark>();
            Console.WriteLine("Benchmark complete.");
            Console.ReadLine();
        }
    }
}