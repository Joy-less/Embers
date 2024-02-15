using System;

namespace Embers.IronRubyBenchmark {
    public class Program {
        public static void Main() {
            BenchmarkDotNet.Running.BenchmarkRunner.Run<RubyBenchmark>();
            Console.WriteLine("Benchmark complete.");
            Console.ReadLine();
        }
    }
}
