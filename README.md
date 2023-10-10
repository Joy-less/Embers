![Noko](Assets/Noko.png)

# Embers (Embeddable Ruby Scripts)

An embeddable Ruby interpreter written entirely in C#.

Its minimalistic design should make Ruby suitable for use in game engines, modding scenarios, or other C# applications.

## Advantages
- Easy to embed, sandbox, and control in your C# application or game.
- Source code is easy to understand, with everything in one place.
- Each interpreter can have multiple scripts which can each run on their own thread and communicate.
- Full compatibility with Godot and Unity.
- Some obsolete functionality, such as numbers starting with 0 being octal integers, is omitted.
- A great mascot.

## Drawbacks
- Less performant than Ruby. Benchmarks suggest it is several times slower.
- Does not have 100% compatibility with Ruby syntax and functionality.

## Note
Ruby is a very flexible language that is sometimes likened to a set of sharp knives. One such feature is the ability to patch core classes. Please keep this in mind when using or contributing to this repository.

## Usage
### Basic example
```csharp
using Embers;

// ...

Interpreter MyInterpreter = new();
Script MyScript = new(MyInterpreter);
MyScript.Evaluate("puts 'hi!'");
```
### Returning values
```csharp
using static Embers.Script;

// ...

Instance Result = MyScript.Evaluate("3 + 2");
Console.WriteLine(Result.Integer); // 5
```
### Asynchronous operation
```csharp
await MyScript.EvaluateAsync("sleep(2)");
```

#### Several scripts example
```csharp
Interpreter Interpreter = new();
Script ScriptA = new(Interpreter);
Script ScriptB = new(Interpreter);

Task.Run(async () => await ScriptA.EvaluateAsync("sleep(2); puts $my_global"));
Thread.Sleep(1000);
Task.Run(async () => await ScriptB.EvaluateAsync("$my_global = 3"));

Thread.Sleep(2000);
Console.WriteLine("Done");
Console.ReadLine();
```

### Stopping scripts
```csharp
MyScript.Stop(); // Stops the script just before the next expression is interpreted.
                 // Also stops all running threads in the script.
```

### Multithreading / Parallelisation
You can run code on multiple threads and even cores. You can do this in C# by creating a script for each thread, or in Ruby by using built-in methods:

```csharp
Script.Evaluate(@"
Parallel.each [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] do |n|
    print n.to_s + ' '
end
getc
puts '\n---'
[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].each do |n|
    Thread.new do
        print n.to_s + ' '
    end
end
getc
puts '\n---'
");
```
Output:
```
2 3 4 5 6 7 8 9 1 10
---
1 3 4 7 2 5 6 8 9 10
---
```

Note that code running on a single thread will be faster if they are regularly accessing the same variables due to locking mechanisms.

### Custom methods
```csharp
MyInterpreter.Integer.InstanceMethods.Add("double_number", MyScript.CreateMethod(async Input => {
    return Input.Interpreter.GetInteger(Input.Instance.Integer * 2);
}, 0));
MyScript.Evaluate("puts 3.double_number"); // 6
```
```csharp
MyInterpreter.Integer.InstanceMethods.Add("catify", MyScript.CreateMethod(async Input => {
    // Get target number
    Instance OnNumber = Input.Instance;
    Instance OnString = await OnNumber.InstanceMethods["to_s"].Call(Input.Script, OnNumber);
    // Catify
    long CatifyFactor = (long)Input.Arguments[0].Integer;
    string CatifiedString = OnString.String;
    for (long i = 0; i < CatifyFactor; i++) {
        CatifiedString += " ~nya";
    }
    // Return result
    return new StringInstance(Input.Interpreter.String, CatifiedString);
}, 1));
MyScript.Evaluate("puts 3.catify 2"); // 3 ~nya ~nya
```
### Custom classes
```csharp
Class Vector2 = MyScript.CreateClass("Vector2");
Vector2.InstanceMethods["initialize"] = MyScript.CreateMethod(async Input => {
    Input.Instance.InstanceVariables["X"] = Input.Arguments[0];
    Input.Instance.InstanceVariables["Y"] = Input.Arguments[1];
    return Input.Interpreter.Nil;
}, 2);
Vector2.InstanceMethods["x"] = MyScript.CreateMethod(async Input => {
    return Input.Instance.InstanceVariables["X"];
}, 0);
Vector2.InstanceMethods["y"] = MyScript.CreateMethod(async Input => {
    return Input.Instance.InstanceVariables["Y"];
}, 0);
MyScript.Evaluate("pos = Vector2.new(1, 2); p [pos.x, pos.y]"); // [1, 2]
```
which is the same as the following:
```csharp
MyScript.Evaluate(@"
class Vector2
    def initialize(x, y)
        @X = x
        @Y = y
    end
    def x
        @X
    end
    def y
        @Y
    end
end
pos = Vector2.new(1, 2); p [pos.x, pos.y] # [1, 2]
");
```
### Sandboxing
If you don't trust the Ruby code that will be run, you can remove access to dangerous methods by passing `false` when creating a script:
```csharp
Script MyScript = new(MyInterpreter, AllowUnsafeApi: false);
```
You can see which APIs can still be accessed in [`Api.cs`](Source/Embers/Api.cs).

### Serialisation
If you don't want to parse your code every time it's run, or don't want it accessible in memory, you can serialise it ahead of time.
```csharp
Console.WriteLine(Interpreter.Serialise("puts 'Hello there!'"));
Console.ReadLine();
```
This will output some C# code, which you can then run directly by wrapping it in `MyScript.Interpret(...);`:
```csharp
MyScript.Interpret(new System.Collections.Generic.List<Embers.Phase2.Expression>() {new Embers.Phase2.MethodCallExpression(new Embers.Phase2.ObjectTokenExpression(new Embers.Phase2.Phase2Token(new DebugLocation(1, 0), Embers.Phase2.Phase2TokenType.LocalVariableOrMethod, "puts", new Embers.Phase1.Phase1Token(new DebugLocation(1, 0), Embers.Phase1.Phase1TokenType.Identifier, "puts", false, false, false))), new System.Collections.Generic.List<Embers.Phase2.Expression>() {new Embers.Phase2.ObjectTokenExpression(new Embers.Phase2.Phase2Token(new DebugLocation(1, 5), Embers.Phase2.Phase2TokenType.String, "Hello there!", new Embers.Phase1.Phase1Token(new DebugLocation(1, 5), Embers.Phase1.Phase1TokenType.String, "Hello there!", true, false, false)))}, null)});
```
Please note that pre-parsed code will not be compatible between different versions of Embers. It should be done just before building your project.

## Game engine support
Embers is fully compatible with Unity, Godot, and other C# game engines. However, certain methods such as `puts` reference `Console`, which is hidden in Godot and Unity, so you will need to make some changes.

For example:

Godot
```csharp
// In Api.cs
Console.WriteLine(Message.LightInspect()); // -> Godot.GD.Print(Message.LightInspect());
Console.WriteLine(); // -> Godot.GD.Print("");
```

Unity
```csharp
// In Api.cs
Console.WriteLine(Message.LightInspect()); // -> UnityEngine.Debug.Log(Message.LightInspect());
Console.WriteLine(); // -> UnityEngine.Debug.Log("");
```

## About Noko
Noko is Embers' mascot who you can see at the top.

She comes from a society living in the Earth's core, nearly 3000km below the surface. She has fiery powers but finds the surface a bit cold.

Noko is short for "Nokoribi" meaning "embers" in Japanese.

You can use the images in the Assets folder under the [CC BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0) license.

![Made with Embers](Assets/Made%20with%20Embers%20Mini.png)