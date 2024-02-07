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
class Pizza
    def f; end
    def self.equals(a, b)
        true
    end
end

p Pizza.new.f
p Pizza.equals 5, 5
";
            Scope Scope = new();
            Scope.SetVariable("pizza", typeof(Pizza));
            Console.WriteLine($"Result: {Scope.Evaluate(Code).Inspect()}");
        }
    }
}