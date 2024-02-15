using System;
using System.Diagnostics;
using IronRuby;
using Microsoft.Scripting.Hosting;

namespace Embers.IronRubyBenchmark {
    internal class Program {
        static void Main() {
            BenchmarkDotNet.Running.BenchmarkRunner.Run<RubyBenchmark>();
        }
    }
}
