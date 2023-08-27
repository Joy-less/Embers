using System.Diagnostics;

namespace Embers
{
    internal class Program
    {
        static void Main() {
            string Code = @"
#class A
#    def a
#        puts 'hi'
#    end
#end
#
#A.a do
#    
#end

#def my_yielding_method
#    yield ""lol""
#    yield
#    yield 2, 3
#end
#
#my_yielding_method do
#    puts ""Yield!""
#end

5.times do
    puts 'hi'
end
";
            Benchmark(() => new Interpreter().Evaluate(Code));

            Console.ReadLine();

            Interpreter Interpreter = new();
            Benchmark(() => {
                Interpreter.Evaluate(@"
250000000.times do
    
end
");
            });
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