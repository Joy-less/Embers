using System.Diagnostics;

namespace Embers
{
    internal class Program
    {
        static void Main() {
            string Code = @"
def a (b, c = 'ok')
    puts b, c
end
a 'first'
";
            Benchmark(() => new Interpreter().Evaluate(Code));

            Console.ReadLine();

            Interpreter Interpreter = new();
            Benchmark(() => {
                Interpreter.Evaluate("2 + 3");
            }, 1_000_000);
            Console.ReadLine();
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