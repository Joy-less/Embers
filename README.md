![Noko](Assets/Noko.png)

# Embers (Embeddable Ruby Scripts)

An embeddable Ruby interpreter written entirely in C#.

Its minimalistic design is intended for use in game engines, game mods, and C# applications.

## Embers vs IronRuby vs Ruby
### Embers
- Embeddable
- Serviceable performance
- Features from Ruby 1-3, but not tied to a Ruby version
- Fully async (useful for UI and game engines)
- Serialise code ahead of time
- Minimalistic built-in APIs
- Cute mascot

### IronRuby
- Embeddable
- Good performance
- Fully compatible with Ruby 1.9.1 and Ruby gems
- Verbose interop
- Abandoned since 2011
- Locked to .NET 4x

### Ruby
- Not embeddable
- Very good performance

## Usage

### Basic Example
```cs
using Embers;

// ...

Scope Scope = new();
Scope.Evaluate("puts 'hi!'"); // hi!
```

### Async
```cs
await Scope.EvaluateAsync("sleep(2)");
```

### Stop Execution
```cs
Scope.Stop(); // Stops the scope just before the next expression is interpreted.
                 // Also stops all running threads in the scope.
```

### Interop
The easy way:
```cs
Scope["my_number"] = 3;
Console.WriteLine(Scope.Evaluate("my_number + 2")); // 5
```
```cs
int AddNumbers(int Num1, int Num2) { return Num1 + Num2; }
Scope["add_numbers"] = AddNumbers;
Scope.Evaluate("puts add_numbers.call(4, 7)"); // 11
```
If you need more flexibility:
```cs
Scope.CurrentModule.InstanceMethods.Add("add_numbers", Scope.CreateMethod(async Input => {
    return new IntegerInstance(Input.Api.Integer, Input.Arguments[0].Integer + Input.Arguments[1].Integer);
}, 2));
Scope.Evaluate("puts add_numbers(4, 7)"); // 11
```
```cs
Scope.Api.Integer.InstanceMethods.Add("double_number", Scope.CreateMethod(async Input => {
    return new IntegerInstance(Input.Api.Integer, Input.Instance.Integer * 2);
}, 0));
Scope.Evaluate("puts 3.double_number"); // 6
```

### Custom classes
The easy way:
```cs
Scope.Evaluate(@"
class Vector2
    def initialize(x, y)
        @x = x
        @y = y
    end
    def x
        @x
    end
    def y
        @y
    end
end
pos = Vector2.new(1, 2); p [pos.x, pos.y] # [1, 2]
");
```
If you need more flexibility:
```cs
Class Vector2 = Scope.CreateClass("Vector2");
Vector2.InstanceMethods["initialize"] = Scope.CreateMethod(async Input => {
    Input.Instance.InstanceVariables["x"] = Input.Arguments[0];
    Input.Instance.InstanceVariables["y"] = Input.Arguments[1];
    return Input.Api.Nil;
}, 2);
Vector2.InstanceMethods["x"] = Scope.CreateMethod(async Input => {
    return Input.Instance.InstanceVariables["x"];
}, 0);
Vector2.InstanceMethods["y"] = Scope.CreateMethod(async Input => {
    return Input.Instance.InstanceVariables["y"];
}, 0);
Scope.Evaluate("pos = Vector2.new(1, 2); p [pos.x, pos.y]"); // [1, 2]
```

### Parallel Processing
You can run code on multiple threads and even cores. You can do this in C# by creating a scope for each thread, or in Ruby by using built-in methods.
```cs
Scope.Evaluate(@"
# Parallel
Parallel.each [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] do |n|
    print n.to_s + ' '
end
# Thread
[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].each do |n|
    Thread.new do
        print n.to_s + ' '
    end
end
");
```

### Sandboxing
If you don't trust the Ruby code that will be run, you can remove access to dangerous methods by passing `false` when creating a scope:
```cs
Scope Scope = new(null, AllowUnsafeApi: false);
```
You can see which APIs can still be accessed in [`Api.cs`](Source/Embers/Api.cs).

### Serialisation
If you don't want to parse your code every time it's run, or don't want it stored in memory, you can serialise it ahead of time.
```cs
Console.WriteLine(Interpreter.Serialise("puts 'Hello there!'"));
Console.ReadLine();
```
This will output some C# code, which you can then run directly by wrapping it in `Scope.Interpret(...);`:
```cs
Scope.Interpret(new System.Collections.Generic.List<Embers.Phase2.Expression>() {new Embers.Phase2.MethodCallExpression(new Embers.Phase2.ObjectTokenExpression(new Embers.Phase2.Phase2Token(new DebugLocation(1, 0), Embers.Phase2.Phase2TokenType.LocalVariableOrMethod, "puts", new Embers.Phase1.Phase1Token(new DebugLocation(1, 0), Embers.Phase1.Phase1TokenType.Identifier, "puts", false, false, false))), new System.Collections.Generic.List<Embers.Phase2.Expression>() {new Embers.Phase2.ObjectTokenExpression(new Embers.Phase2.Phase2Token(new DebugLocation(1, 5), Embers.Phase2.Phase2TokenType.String, "Hello there!", new Embers.Phase1.Phase1Token(new DebugLocation(1, 5), Embers.Phase1.Phase1TokenType.String, "Hello there!", true, false, false)))}, null)});
```
Please note that serialised code is not necessarily compatible between different versions of Embers. It should be done just before building your project.

## Game engine support
Embers is fully compatible with Godot, Unity, and other C# game engines. However, certain methods such as `puts` reference the `Console`, which is hidden in Godot and Unity by default, so you will need to make some changes.

For example:

#### Godot
```cs
// In Api.cs
Console.WriteLine(Message.LightInspect()); // -> Godot.GD.Print(Message.LightInspect());
Console.WriteLine(); // -> Godot.GD.Print("");
```

#### Unity
```cs
// In Api.cs
Console.WriteLine(Message.LightInspect()); // -> UnityEngine.Debug.Log(Message.LightInspect());
Console.WriteLine(); // -> UnityEngine.Debug.Log("");
```

## About Noko
Noko is Embers' mascot who you can see at the top.

She comes from a society living in the Earth's core, nearly 3000km below the surface. She has fiery powers but finds the surface a bit cold.

Noko is short for "Nokoribi" meaning "embers" in Japanese.

You can use the images in the Assets folder under the [CC BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0) license.

#### What does she think of this project?
She hates it.

![Made with Embers](Assets/Made%20with%20Embers%20Mini.png)