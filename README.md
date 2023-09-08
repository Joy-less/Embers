![Noko](Assets/Noko.png)

# Embers (Embeddable Ruby Scripts)

An embeddable Ruby interpreter written entirely in C#. The aim is to make Ruby usable in the Unity game engine or other C# applications.

Its minimalistic design should be suitable for use in game engines or modding scenarios.

## Advantages
- Easy to embed, sandbox, and control in your C# application or game.
- Source code is easy to understand, with everything in one place.
- Each interpreter can have multiple scripts which can each run on their own thread and communicate.
- Full compatibility with Unity.
- Obsolete functionality, such as numbers starting with 0 being octal integers, is not included.
- A great mascot.

## Drawbacks
- Likely to be considerably less optimised than Ruby. Benchmarks show it is several times slower.
- Does not have 100% compatibility with Ruby syntax and functionality.

## Note
Ruby is a very flexible language that is sometimes likened to a set of sharp knives. One such feature is the ability to patch core classes. Please keep this in mind when using or contributing to this repository.

## Usage
### Basic example
```csharp
Interpreter MyInterpreter = new();
Script MyScript = new(MyInterpreter);
MyScript.Evaluate("puts 'hi!'");
```
### Returning values
```csharp
using static Embers.Script;

// ...

Instances Result = MyScript.Evaluate("3 + 2");
Console.WriteLine(Result[0].Integer); // 5
```
### Asynchronous operation
```csharp
await MyScript.EvaluateAsync("sleep(2)");
```
#### Several scripts example
```csharp
string CodeA = @"
sleep(2)
puts $my_global
";
string CodeB = @"
$my_global = 3
";
Interpreter Interpreter = new();
Script ScriptA = new(Interpreter);
Script ScriptB = new(Interpreter);

Task.Run(async () => await ScriptA.EvaluateAsync(CodeA));
Thread.Sleep(1000);
Task.Run(async () => await ScriptB.EvaluateAsync(CodeB));

Thread.Sleep(2000);
Console.WriteLine("Done");
Console.ReadLine();
```
### Parallelisation
You can also run code on multiple cores.

Note that code running on a single thread will be faster if they are accessing the same variables.

<details><summary>Benchmark</summary>

```csharp
const string BenchmarkCode = @"
$i = 0
while $i < 550000
    # Random equations
    r1 = rand 20
    r2 = rand 20
    r1 - (r2 % r1 + r1) * r2 - (r1 ** r2)
    r2 *= r1 - r2
    r1 = r2 + r2 + 2 * (r1 - r2)
    
    # Increment counter
    $i += 1
end
";
{
    Console.WriteLine("Single thread benchmark:");

    Interpreter SingleThreadInterpreter = new();
    Script SingleThreadScript = new(SingleThreadInterpreter);

    Benchmark(() => SingleThreadScript.Evaluate(BenchmarkCode));
}

{
    Console.WriteLine("Multi-threading benchmark:");

    Interpreter MultiThreadInterpreter = new();
    Script MultiThreadScriptA = new(MultiThreadInterpreter);
    Script MultiThreadScriptB = new(MultiThreadInterpreter);
    Script MultiThreadScriptC = new(MultiThreadInterpreter);
    Script MultiThreadScriptD = new(MultiThreadInterpreter);

    Task.WaitAll(
        Task.Run(() => Benchmark(() => MultiThreadScriptA.Evaluate(BenchmarkCode))),
        Task.Run(() => Benchmark(() => MultiThreadScriptB.Evaluate(BenchmarkCode))),
        Task.Run(() => Benchmark(() => MultiThreadScriptC.Evaluate(BenchmarkCode))),
        Task.Run(() => Benchmark(() => MultiThreadScriptD.Evaluate(BenchmarkCode)))
    );
}

{
    Console.WriteLine("Parallel benchmark:");

    Interpreter ParallelInterpreter = new();
    Script ParallelScriptA = new(ParallelInterpreter);
    Script ParallelScriptB = new(ParallelInterpreter);
    Script ParallelScriptC = new(ParallelInterpreter);
    Script ParallelScriptD = new(ParallelInterpreter);

    Parallel.Invoke(
        () => Benchmark(() => ParallelScriptA.Evaluate(BenchmarkCode)),
        () => Benchmark(() => ParallelScriptB.Evaluate(BenchmarkCode)),
        () => Benchmark(() => ParallelScriptC.Evaluate(BenchmarkCode)),
        () => Benchmark(() => ParallelScriptD.Evaluate(BenchmarkCode))
    );
}
```
```
Single thread benchmark:
Took 16.356 seconds
Multi-threading benchmark:
Took 10.334 seconds
Took 10.335 seconds
Took 10.335 seconds
Took 10.335 seconds
Parallel benchmark:
Took 10.398 seconds
Took 10.398 seconds
Took 10.398 seconds
Took 10.398 seconds
```
</details>

### Custom methods
```csharp
MyScript.Integer.InstanceMethods.Add("double_number", new Method(async Input => {
    return new IntegerInstance(Input.Interpreter.Integer, Input.Instance.Integer * 2);
}, 0));
MyScript.Evaluate("puts 3.double_number"); // 6
```
```csharp
MyScript.Integer.InstanceMethods.Add("catify", new Method(async Input => {
    // Get target string
    Instance OnNumber = Input.Instance;
    Instance OnString = (await OnNumber.InstanceMethods["to_s"].Call(Input.Interpreter, OnNumber))[0];
    // Catify
    long CatifyFactor = Input.Arguments[0].Integer;
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
Vector2.InstanceMethods["initialize"] = new Method(async Input => {
    Input.Instance.InstanceVariables["X"] = Input.Arguments[0];
    Input.Instance.InstanceVariables["Y"] = Input.Arguments[1];
    return Input.Interpreter.Nil;
}, 2);
Vector2.InstanceMethods["x"] = new Method(async Input => {
    return Input.Instance.InstanceVariables["X"];
}, 0);
Vector2.InstanceMethods["y"] = new Method(async Input => {
    return Input.Instance.InstanceVariables["Y"];
}, 0);
MyScript.Evaluate("pos = Vector2.new 1, 2; puts(\"{#{pos.x}, #{pos.y}}\")"); // {1, 2}
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
pos = Vector2.new 1, 2; puts(""{#{pos.x}, #{pos.y}}"") # {1, 2}
");
```
### Sandboxing
If you don't trust the Ruby code that will be run, you can remove access to dangerous methods by passing `false` when creating the script:
```csharp
Script MyScript = new(MyInterpreter, false);
```
You can see which APIs can still be accessed in [`Api.cs`](Source/Embers/Api.cs).

### Serialisation
If you don't want to parse your code every time it's run, and want it to be obfuscated in memory, you can serialise it ahead of time.
```csharp
Console.WriteLine(Interpreter.Serialise("puts 'Hello there!'"));
Console.ReadLine();
```
This will output some C# code, which you can then run directly by wrapping it in `MyScript.Interpret(...);`:
```csharp
MyScript.Interpret(new List<Embers.Phase2.Expression>() {new Embers.Phase2.MethodCallExpression(new Embers.Phase2.ObjectTokenExpression(new Embers.Phase2.Phase2Token(new DebugLocation(1, 0), Embers.Phase2.Phase2TokenType.LocalVariableOrMethod, "puts", new Embers.Phase1.Phase1Token(new DebugLocation(1, 0), Embers.Phase1.Phase1TokenType.Identifier, "puts", false, false))), new List<Embers.Phase2.Expression>() {new Embers.Phase2.ObjectTokenExpression(new Embers.Phase2.Phase2Token(new DebugLocation(1, 5), Embers.Phase2.Phase2TokenType.String, "Hello there!", new Embers.Phase1.Phase1Token(new DebugLocation(1, 5), Embers.Phase1.Phase1TokenType.String, "Hello there!", true, false)))}, null)});
```
Please note that pre-parsed code will not be compatible between different versions of Embers. It should be done just before building your project.

## About Noko
Noko is Embers' mascot who you can see at the top.

She comes from a society living in the Earth's core, nearly 3000km below the surface. She has fiery powers but finds the surface a bit cold.

Noko is short for "Nokoribi" meaning "embers" in Japanese.

You can use her branding under the [CC BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0) license.

![Made with Embers](Assets/Made%20with%20Embers%20Mini.png)