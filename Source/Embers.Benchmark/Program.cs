using System.Diagnostics;

namespace Embers.Benchmark {
    internal class Program {
        static void Main() {
            Console.WriteLine("Embers (100_000 iterations)");
            Benchmark(() => {
                Scope Scope = new();
                for (int i = 0; i < 100_000; i++) {
                    Scope.Evaluate("a = 0; a += 1");
                }
            });
            Console.WriteLine("Embers (1_000_000 iterations pre-parsed)");
            Benchmark(() => {
                Scope Scope = new();
                Expression[] Expressions = Scope.Parse("a = 0; a += 1");
                for (int i = 0; i < 1_000_000; i++) {
                    Scope.Interpret(Expressions);
                }
            });
            Console.WriteLine("Embers (1_000_000.times block)");
            Benchmark(() => {
                Scope Scope = new();
                Scope.Evaluate(@"1_000_000.times do; a = 0; a += 1; end");
            });
            Console.WriteLine("Embers (empty 10_000_000.times block)");
            Benchmark(() => {
                Scope Scope = new();
                Scope.Evaluate(@"10_000_000.times do; end");
            });
            Console.WriteLine("Embers (fibonacci to 25)");
            Benchmark(() => {
                Scope Scope = new();
                Scope.Evaluate(@"
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
                ");
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
