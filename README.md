![Noko](https://raw.githubusercontent.com/Joy-less/Embers/main/Assets/Noko%20Mini.png)

# Embers

[![NuGet](https://img.shields.io/nuget/v/Embers.svg)](https://www.nuget.org/packages/Embers)

An embeddable Ruby interpreter written entirely in C#.

Its powerful, lightweight design is intended for use in game engines, game mods, and C# applications.

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

### Adapter

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

### Game engine support
Embers is fully compatible with Godot, Unity, and other C# game engines.

Here's a counter example in Godot:
```cs
public partial class CounterScript : Node {
    [Export] RichTextLabel CounterLabel;

    Scope Scope;

    public override void _Ready() {
        Scope = new Scope();
        Scope.Axis.Object.SetInstanceVariable("@counter_label", CounterLabel);
        Scope.Evaluate(@"
@counter = 0
def _process
  @counter += 1
  @counter_label.text = @counter.to_s
end
        ");
    }
    public override void _Process(double Delta) {
        Scope.Axis.Main.CallMethod("_process");
    }
}
```

Note that methods such as `puts` use the `Console`, which is hidden in Godot and Unity by default. This can be changed:
```cs
Scope Scope = new(new AxisOptions() { Logger = new CustomLogger() });
```

## About Noko

Noko is Embers' mascot you can see at the top.

She comes from a society in the Earth's core, 3000km below the surface. She has fiery powers, but finds the surface a bit cold.

Noko is short for "Nokoribi", meaning "Embers" in Japanese.

![Made with Embers](https://raw.githubusercontent.com/Joy-less/Embers/main/Assets/Powered%20by%20Embers%20Mini.png)