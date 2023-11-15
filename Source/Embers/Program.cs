using System;
using System.Diagnostics;

namespace Embers
{
    internal class Program
    {
        static string ToSnakeCase(string String) {
            if (String.Length == 0) return String;
            else if (String.Length == 1) return String.ToLower();
            else {
                string SnakeString = "";
                foreach (char Chara in String) {
                    SnakeString += char.IsUpper(Chara)
                        ? "_" + char.ToLower(Chara)
                        : Chara;
                }
                return SnakeString.TrimStart('_');
            }
        }

        static void Main() {
            // Test
            {
                Scope Scope = new();
                Benchmark(() =>
                    Scope.Evaluate(@"
def fibonacci(n)
  if n <= 1
    return n
  else
    return fibonacci(n - 1) + fibonacci(n - 2)
  end
end

22.times do |n|
    puts fibonacci(n)
end

puts ""Started.""
t = Time.now.to_f; i = 0; for i in 1..1_000_000 do i += 1 end; puts Time.now.to_f - t
                    ")
                );
                Console.ReadLine();
            }
            // Benchmark
            {
                Scope Scope = new();
                Benchmark(() =>
                    Scope.Evaluate("1_000_000.times do end")
                );
                Console.ReadLine();
            }
        }
        static void Benchmark(Action Code, int Times = 1) {
            Stopwatch Stopwatch = new();
            Stopwatch.Start();
            for (int i = 0; i < Times; i++)
                Code();
            Stopwatch.Stop();
            Console.WriteLine($"Took {Stopwatch.ElapsedMilliseconds / 1000d} seconds");
        }
    }
}