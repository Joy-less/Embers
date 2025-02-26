![Noko](https://raw.githubusercontent.com/Joy-less/Embers/main/Assets/Noko%20Mini.png)

# Embers

[![NuGet](https://img.shields.io/nuget/v/Embers.svg)](https://www.nuget.org/packages/Embers)

An embeddable Ruby interpreter written entirely in C#.

Its powerful, lightweight design is intended for use in game engines, game mods, and C# applications.

> [!WARNING]  
> This project has been inactive since 2024/03/19. Please do not use it anymore. It has unsolvable bugs, poor performance, and differences to Ruby.
>
> As an alternative, please consider one of the following:
> ```cs
> // IronRuby.Portable + IronRuby.Libraries
> // Ruby interpreter for Ruby 1.9 (inactive since 2011).
> 
> Microsoft.Scripting.Hosting.ScriptEngine RubyEngine = IronRuby.Ruby.CreateEngine();
> RubyEngine.Execute("""
>     puts("Hello from IronRuby!")
>     """);
> 
> // NLua
> // .NET-bridged native Lua bindings for Lua 5.4 (active development).
> 
> using NLua.Lua NLuaEngine = new();
> NLuaEngine.DoString("""
>     print("Hello from NLua!")
>     """);
> 
> // KeraLua
> // Raw native Lua bindings for Lua 5.4 (active development).
> 
> using KeraLua.Lua KeraLuaEngine = new();
> KeraLuaEngine.DoString("""
>     print("Hello from KeraLua!")
>     """);
> ```
>
> My official statement for abandoning Embers:
> > I loved using RGSS in RPG Maker and it was my motivation to create Embers. Ruby is neat.
> > If you'd like to know why I abandoned the project, it's because of the difficulty and mess. Because I was new to creating programming languages, Embers doesn't actually use its own call stack; it uses C#, which creates problems. The main one being that `return` and `break` don't work correctly in blocks. I had already remade Embers twice before and was very burnt-out from working on the project. I have other projects so I don't really have time to work on Embers anymore.
>
> Thank you for your time, and good luck.

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