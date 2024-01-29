using System;
using System.Diagnostics;
using IronRuby;
using Microsoft.Scripting.Hosting;

namespace Embers.IronRubyBenchmark {
    internal class Program {
        static void Main() {
            Console.WriteLine("IronRuby (100_000 iterations)");
            Benchmark(() => {
                ScriptEngine Engine = Ruby.CreateEngine();
                ScriptScope Scope = Engine.CreateScope();
                for (int i = 0; i < 100_000; i++) {
                    Engine.Execute("a = 0; a += 1", Scope);
                }
            });
            Console.WriteLine("IronRuby (1_000_000.times block)");
            Benchmark(() => {
                ScriptEngine Engine = Ruby.CreateEngine();
                ScriptScope Scope = Engine.CreateScope();
                Engine.Execute("1_000_000.times {a = 0; a += 1}", Scope);
            });
            Console.WriteLine("IronRuby (fibonacci to 25)");
            Benchmark(() => {
                ScriptEngine Engine = Ruby.CreateEngine();
                ScriptScope Scope = Engine.CreateScope();
                Engine.Execute(@"
def fibonacci(n)
  if n <= 1
    return n
  else
    return fibonacci(n - 1) + fibonacci(n - 2)
  end
end

25.times do |n|
    fibonacci(n)
end
                ", Scope);
            });
            Console.ReadLine();
        }

        static void Benchmark(Action Action) {
            Stopwatch Stopwatch = Stopwatch.StartNew();
            Action();
            Stopwatch.Stop();
            Console.WriteLine($"{Stopwatch.Elapsed.TotalSeconds} seconds elapsed");
        }
    }
}
