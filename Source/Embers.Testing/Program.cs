namespace Embers.Testing {
    public class Program {
        static void Main() {
            const string Code = @"
puts 'I <3 Ruby!'
";
            Scope Scope = new();
            Console.WriteLine($"Result: {Scope.Evaluate(Code).Inspect()}");
        }
    }
}