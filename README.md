# Embers (Embeddable Ruby Scripts)
An embeddable Ruby interpreter written entirely in C#. The aim is to make Ruby usable in the Unity game engine or other C# applications.

Its minimalistic design should be suitable for use in game engines or modding scenarios.

## Advantages
- Easy to embed, sandbox, and control in your C# application or game.
- Source code is much easier to understand, with everything in one place.
- Fully compatible with Unity.
- Obsolete functionality, such as interpreting numbers starting with 0 as octal, is removed.

## Drawbacks
- Likely to be considerably less optimised than Ruby. Benchmarks show it is several times slower.
- Does not have 100% compatibility with Ruby syntax or functionality.

## Note
Ruby is a very flexible language that is often likened to a set of sharp knives. One such feature is allowing the programmer to patch core classes. Please keep this in mind when using or contributing to this repository.

## Usage
### Basic example:
```csharp
using static Embers.Interpreter;

// ...

Interpreter MyInterpreter = new();
MyInterpreter.Evaluate("puts 'hi!'");
```
### Returning values:
```csharp
Instances Result = MyInterpreter.Evaluate("3 + 2");
Console.WriteLine(Result[0].Integer); // 5
```
### Asynchronously:
```csharp
await MyInterpreter.EvaluateAsync("sleep(2)");
```
### Custom methods:
```csharp
MyInterpreter.Integer.InstanceMethods.Add("double_number", new Method(async Input => {
    return new IntegerInstance(Input.Interpreter.Integer, Input.Instance.Integer * 2);
}, 0));
MyInterpreter.Evaluate("puts 3.double_number"); // 6
```
```csharp
MyInterpreter.Integer.InstanceMethods.Add("catify", new Method(async Input => {
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
MyInterpreter.Evaluate("puts 3.catify 2"); // 3 ~nya ~nya
```
### Custom classes:
```csharp
Class Vector2 = MyInterpreter.CreateClass("Vector2");
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
MyInterpreter.Evaluate("pos = Vector2.new 1, 2; puts(\"{#{pos.x}, #{pos.y}}\")"); // {1, 2}
```
which is the same as the following:
```csharp
MyInterpreter.Evaluate(@"
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