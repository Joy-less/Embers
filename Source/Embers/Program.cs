using System.Diagnostics;
using static Embers.Interpreter;
using static Embers.Phase2;

namespace Embers
{
    internal class Program
    {
        static void Main() {
            {
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

3.times do
    puts 'hi'
end

def my_method
    return ""Ok""
end
puts my_method

class A
    def a
        
    end
end
# A.a
";
                Benchmark(() => new Interpreter().Evaluate(Code));
                Console.ReadLine();
            }

            // Benchmark (pre-baked)
            {
                // Console.WriteLine(Interpreter.Serialise("100000.times do \n a = 3 + 2 / 4 ** 2 \n end"));
                Interpreter Interpret = new();
                Benchmark(() => {
                    var CompiledBenchmark = new List<Statement>() {new ExpressionStatement(new MethodCallExpression(new PathExpression(new ObjectTokenExpression(new Phase2Token(Phase2TokenType.Integer, "100000", false)), new Phase2Token(Phase2TokenType.LocalVariableOrMethod, "times", false)), new List<Expression>() {}, new MethodExpression(new List<Statement>() {new AssignmentStatement(new ObjectTokenExpression(new Phase2Token(Phase2TokenType.LocalVariableOrMethod, "a", true)), "=", new MethodCallExpression(new PathExpression(new ObjectTokenExpression(new Phase2Token(Phase2TokenType.Integer, "3", true)), new Phase2Token(Phase2TokenType.LocalVariableOrMethod, "+", false)), new List<Expression>() {new MethodCallExpression(new PathExpression(new ObjectTokenExpression(new Phase2Token(Phase2TokenType.Integer, "2", true)), new Phase2Token(Phase2TokenType.LocalVariableOrMethod, "+", false)), new List<Expression>() {new MethodCallExpression(new PathExpression(new ObjectTokenExpression(new Phase2Token(Phase2TokenType.Integer, "4", true)), new Phase2Token(Phase2TokenType.LocalVariableOrMethod, "+", false)), new List<Expression>() {new ObjectTokenExpression(new Phase2Token(Phase2TokenType.Integer, "2", true))}, null)}, null)}, null))}, new IntRange(null, null), new List<MethodArgumentExpression>() {})))};
                    Interpret.Interpret(CompiledBenchmark);
                });
                Console.ReadLine();
            }

            // Benchmark
            {
                Interpreter Interpret = new();
                Benchmark(() => {
                    // Interpret.Evaluate("250000000.times do \n end");
                    Interpret.Evaluate("100000.times do \n a = 3 + 2 / 4 ** 2 \n end");
                });
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