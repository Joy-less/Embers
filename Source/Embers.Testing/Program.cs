namespace Embers.Testing {
    public class Program {
        class Pizza {
            public static void Bite() {
                Console.WriteLine("bitten");
            }
            public static void Bite(int Times) {
                Console.WriteLine("bitten " + Times.ToString());
            }
        }
        static void Main() {
            const string Code = @"
=begin
puts 'I <3 Ruby!'

pizza.bite
pizza.bite 5

p pizza.methods

p pizza.new.class.bite 10

p pizza.method :equals
p 5.send :puts, 'hi'

printer = -> a, b {
    puts ""printing #{a}, #{b}""
}
printer.call 1, 2
=end

def fibonacci(n)
    if n <= 1
        return n
    else
        return fibonacci(n - 1) + fibonacci(n - 2)
    end
end

x = Time.now
28.times do |n|
    p ""#{n}. #{fibonacci(n)}""
end
p Time.now.to_f - x.to_f
gets
";
            Scope Scope = new();
            Scope.SetVariable("pizza", typeof(Pizza));
            Console.WriteLine($"Result: {Scope.Evaluate(Code).Inspect()}");
        }
    }
}