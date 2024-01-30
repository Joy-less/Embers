![Noko](Assets/Noko%20Mini.png)

# Embers

An embeddable Ruby interpreter written entirely in C#.

Its minimalistic design is intended for use in game engines, game mods, and C# applications.

## Features

- Run Ruby code in C# .NET
- Adapter to convert between .NET objects and Ruby instances
- Multithreading and thread safety option
- Simplified standard library
- No external dependencies

## Usage

### Basic Example

```cs
using Embers;

Scope Scope = new();
Scope.Evaluate("puts 'Ruby!'"); // Ruby!
```

### Interop

Embers has an adapter which converts between .NET objects and Ruby instances. If an object is not built-in, its methods, fields and properties will be adapted. Methods such as `SetVariable` implicitly use the adapter for you.

```cs
class Pizza {
    public int Portions = 2;
    public string Topping = "Pepperoni";
}
Scope.SetVariable("pizza", typeof(Pizza));
string Topping = Scope.Evaluate("pizza.new.topping").CastString;
Console.WriteLine(Topping); // Pepperoni
```

### Parsing

Expressions can be pre-parsed and interpreted much more quickly.

```cs
Expression[] Expressions = Scope.Parse("puts 'pre-parsed!'");
Expressions.Interpret(new Context(Scope.Location, Scope)); // pre-parsed!
```

### Sandboxing

You can see which methods can still be accessed in [`StandardLibrary.cs`](Source/Embers/Runtime/StandardLibrary.cs).

```cs
Scope Scope = new(new AxisOptions() { Sandbox = true });
Scope.Evaluate("File.write('test.txt', 'text')"); // undefined method 'write' for File:Module
```

## Game engine support
Embers is fully compatible with Godot, Unity, and other C# game engines. However, certain methods such as `puts` reference the `Console`, which is hidden in Godot and Unity by default, so you will need to make some changes.

For example:
```cs
// In StandardLibrary.cs
Console.WriteLine(Message.ToS()); // -> Godot.GD.Print(Message.ToS());
```

## About Noko

Noko is Embers' mascot who you can see at the top.

She comes from a society in the Earth's core, 3000km below the surface. She has fiery powers but finds the surface a bit cold.

Noko is short for "Nokoribi" meaning "Embers" in Japanese.

![Made with Embers](Assets/Powered%20by%20Embers%20Mini.png)