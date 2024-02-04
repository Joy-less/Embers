﻿namespace Embers.Testing {
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
puts 'I <3 Ruby!'

pizza.bite
pizza.bite 5

p pizza.methods
";
            Scope Scope = new();
            Scope.SetVariable("pizza", typeof(Pizza));
            Console.WriteLine($"Result: {Scope.Evaluate(Code).Inspect()}");
        }
    }
}