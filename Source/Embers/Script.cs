using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using static Embers.Phase2;

#nullable enable
#pragma warning disable CS1998

namespace Embers
{
    public class Script
    {
        public readonly Interpreter Interpreter;
        public readonly bool AllowUnsafeApi;
        public bool Running { get; private set; }
        public bool Stopping { get; private set; }
        public DebugLocation ApproximateLocation { get; private set; } = DebugLocation.Unknown;

        readonly Stack<object> CurrentObject = new();
        Block CurrentBlock => (Block)CurrentObject.First(obj => obj is Block);
        Scope CurrentScope => (Scope)CurrentObject.First(obj => obj is Scope);
        MethodScope CurrentMethodScope => (MethodScope)CurrentObject.First(obj => obj is MethodScope);
        Module CurrentModule => (Module)CurrentObject.First(obj => obj is Module);
        Instance CurrentInstance => (Instance)CurrentObject.First(obj => obj is Instance);

        public AccessModifier CurrentAccessModifier = AccessModifier.Public;

        Method? CurrentOnYield;

        internal readonly ConditionalWeakTable<Exception, ExceptionInstance> ExceptionsTable = new();
        public int ThreadCount { get; private set; }

        public Instance CreateInstanceWithNew(Class Class) {
            if (Class.InheritsFrom(Interpreter.NilClass))
                return new NilInstance(Class);
            else if (Class.InheritsFrom(Interpreter.TrueClass))
                return new TrueInstance(Class);
            else if (Class.InheritsFrom(Interpreter.FalseClass))
                return new FalseInstance(Class);
            else if (Class.InheritsFrom(Interpreter.String))
                return new StringInstance(Class, "");
            else if (Class.InheritsFrom(Interpreter.Symbol))
                return GetSymbol("");
            else if (Class.InheritsFrom(Interpreter.Integer))
                return new IntegerInstance(Class, 0);
            else if (Class.InheritsFrom(Interpreter.Float))
                return new FloatInstance(Class, 0);
            else if (Class.InheritsFrom(Interpreter.Proc))
                throw new RuntimeException($"{ApproximateLocation}: Tried to create Proc instance without a block");
            else if (Class.InheritsFrom(Interpreter.Range))
                throw new RuntimeException($"{ApproximateLocation}: Tried to create Range instance with new");
            else if (Class.InheritsFrom(Interpreter.Array))
                return new ArrayInstance(Class, new List<Instance>());
            else if (Class.InheritsFrom(Interpreter.Hash))
                return new HashInstance(Class, new Dictionary<Instance, Instance>(), Interpreter.Nil);
            else if (Class.InheritsFrom(Interpreter.Exception))
                return new ExceptionInstance(Class, "");
            else if (Class.InheritsFrom(Interpreter.Thread))
                return new ThreadInstance(Class, this);
            else if (Class.InheritsFrom(Interpreter.Time))
                return new TimeInstance(Class, new DateTime());
            else
                return new Instance(Class);
        }

        public class Block {
            public readonly Dictionary<string, Instance> LocalVariables = new();
            public readonly Dictionary<string, Instance> Constants = new();
        }
        public class Scope : Block {
            
        }
        public class MethodScope : Scope {
            public readonly Method? Method;
            public MethodScope(Method method) : base() {
                Method = method;
            }
        }
        public class Module : Block {
            public readonly string Name;
            public readonly ReactiveDictionary<string, Method> Methods = new();
            public readonly ReactiveDictionary<string, Method> InstanceMethods = new();
            public readonly Dictionary<string, Instance> ClassVariables = new();
            public readonly Interpreter Interpreter;
            public readonly Module? SuperModule;
            public Module(string name, Module parent, Module? superModule = null) {
                Name = name;
                Interpreter = parent.Interpreter;
                SuperModule = superModule ?? Interpreter.Object;
                Setup();
            }
            public Module(string name, Interpreter interpreter, Module? superModule = null) {
                Name = name;
                Interpreter = interpreter;
                SuperModule = superModule;
                Setup();
            }
            protected virtual void Setup() {
                // Copy superclass class and instance methods
                if (SuperModule != null) {
                    SuperModule.Methods.CopyTo(Methods);
                    SuperModule.InstanceMethods.CopyTo(InstanceMethods);
                    // Inherit changes later
                    SuperModule.Methods.Set += (string Key, Method NewValue) => {
                        Methods[Key] = NewValue;
                    };
                    SuperModule.InstanceMethods.Set += (string Key, Method NewValue) => {
                        InstanceMethods[Key] = NewValue;
                    };
                    SuperModule.Methods.Removed += (string Key) => {
                        Methods.Remove(Key);
                    };
                    SuperModule.InstanceMethods.Removed += (string Key) => {
                        InstanceMethods.Remove(Key);
                    };
                }
            }
            public bool InheritsFrom(Module? Ancestor) {
                if (Ancestor == null)
                    return false;
                Module? CurrentAncestor = this;
                while (CurrentAncestor != null) {
                    if (CurrentAncestor == Ancestor)
                        return true;
                    CurrentAncestor = CurrentAncestor.SuperModule;
                }
                return false;
            }
        }
        public class Class : Module {
            public Class(string name, Module parent, Module? superClass = null) : base(name, parent, superClass) { }
            public Class(string name, Interpreter interpreter) : base(name, interpreter) { }
            protected override void Setup() {
                // Default method: new
                Methods["new"] = new Method(async Input => {
                    Instance NewInstance = Input.Script.CreateInstanceWithNew((Class)Input.Instance.Module!);
                    if (NewInstance.InstanceMethods.TryGetValue("initialize", out Method? Initialize)) {
                        // Call initialize & ignore result
                        await Initialize.Call(Input.Script, NewInstance, Input.Arguments, Input.OnYield);
                        // Return instance
                        return NewInstance;
                    }
                    else {
                        throw new RuntimeException($"Undefined method 'initialize' for {Name}");
                    }
                }, null);
                // Default method: initialize
                InstanceMethods["initialize"] = new Method(async Input => {
                    return Input.Instance;
                }, 0);
                // Base setup
                base.Setup();
            }
        }
        public class Instance {
            public readonly Module? Module; // Will be null if instance is a pseudoinstance
            public readonly long ObjectId;
            public virtual ReactiveDictionary<string, Instance> InstanceVariables { get; } = new();
            public virtual ReactiveDictionary<string, Method> InstanceMethods { get; } = new();
            public bool IsTruthy => Object is not (null or false);
            public virtual object? Object { get { return null; } }
            public virtual bool Boolean { get { throw new RuntimeException("Instance is not a boolean"); } }
            public virtual string String { get { throw new RuntimeException("Instance is not a string"); } }
            public virtual long Integer { get { throw new RuntimeException("Instance is not an integer"); } }
            public virtual double Float { get { throw new RuntimeException("Instance is not a float"); } }
            public virtual Method Proc { get { throw new RuntimeException("Instance is not a proc"); } }
            public virtual ScriptThread? Thread { get { throw new RuntimeException("Instance is not a thread"); } }
            public virtual LongRange Range { get { throw new RuntimeException("Instance is not a range"); } }
            public virtual List<Instance> Array { get { throw new RuntimeException("Instance is not an array"); } }
            public virtual Dictionary<Instance, Instance> Hash { get { throw new RuntimeException("Instance is not a hash"); } }
            public virtual Exception Exception { get { throw new RuntimeException("Instance is not an exception"); } }
            public virtual DateTimeOffset Time { get { throw new RuntimeException("Instance is not a time"); } }
            public virtual Module ModuleRef { get { throw new ApiException("Instance is not a class/module reference"); } }
            public virtual Method MethodRef { get { throw new ApiException("Instance is not a method reference"); } }
            public virtual string Inspect() {
                return $"#<{Module?.Name}:0x{GetHashCode():x16}>";
            }
            public virtual string LightInspect() {
                return Inspect();
            }
            public static async Task<Instance> CreateFromToken(Script Script, Phase2Token Token) {
                if (Token.ProcessFormatting) {
                    string String = Token.Value!;
                    Stack<int> FormatPositions = new();
                    char? LastChara = null;
                    for (int i = 0; i < String.Length; i++) {
                        char Chara = String[i];

                        if (LastChara == '#' && Chara == '{') {
                            FormatPositions.Push(i - 1);
                        }
                        else if (Chara == '}') {
                            if (FormatPositions.TryPop(out int StartPosition)) {
                                string FirstHalf = String[..StartPosition];
                                string ToFormat = String[(StartPosition + 2)..i];
                                string SecondHalf = String[(i + 1)..];

                                string Formatted = (await Script.InternalEvaluateAsync(ToFormat)).LightInspect();
                                String = FirstHalf + Formatted + SecondHalf;
                                i = FirstHalf.Length - 1;
                            }
                        }
                        LastChara = Chara;
                    }
                    return new StringInstance(Script.Interpreter.String, String);
                }

                return Token.Type switch {
                    Phase2TokenType.Nil => Script.Interpreter.Nil,
                    Phase2TokenType.True => Script.Interpreter.True,
                    Phase2TokenType.False => Script.Interpreter.False,
                    Phase2TokenType.String => new StringInstance(Script.Interpreter.String, Token.Value!),
                    Phase2TokenType.Integer => new IntegerInstance(Script.Interpreter.Integer, Token.ValueAsLong),
                    Phase2TokenType.Float => new FloatInstance(Script.Interpreter.Float, Token.ValueAsDouble),
                    _ => throw new InternalErrorException($"{Token.Location}: Cannot create new object from token type {Token.Type}")
                };
            }
            public Instance(Module fromModule) {
                Module = fromModule;
                ObjectId = fromModule.Interpreter.GenerateObjectId;
                Setup();
            }
            public Instance(Interpreter interpreter) {
                Module = null;
                ObjectId = interpreter.GenerateObjectId;
                Setup();
            }
            void Setup() {
                if (this is not PseudoInstance && Module != null) {
                    // Copy instance methods
                    Module.InstanceMethods.CopyTo(InstanceMethods);
                    // Inherit changes later
                    Module.InstanceMethods.Set += (string Key, Method NewValue) => {
                        InstanceMethods[Key] = NewValue;
                    };
                    Module.InstanceMethods.Removed += (string Key) => {
                        InstanceMethods.Remove(Key);
                    };
                }
            }
            public void AddOrUpdateInstanceMethod(string Name, Method Method) {
                lock (InstanceMethods) lock (Module!.InstanceMethods)
                    InstanceMethods[Name] =
                    Module.InstanceMethods[Name] = Method;
            }
            public async Task<Instance> TryCallInstanceMethod(Script Script, string MethodName, Instances? Arguments = null, Method? OnYield = null) {
                // Found
                if (InstanceMethods.TryGetValue(MethodName, out Method? FindMethod)) {
                    return await Script.CreateTemporaryClassScope(Module!, async () =>
                        await FindMethod.Call(Script, this, Arguments, OnYield)
                    );
                }
                // Error
                else {
                    throw new RuntimeException($"{Script.ApproximateLocation}: Undefined method '{MethodName}' for {Module?.Name}");
                }
            }
        }
        public class NilInstance : Instance {
            public override string Inspect() {
                return "nil";
            }
            public override string LightInspect() {
                return "";
            }
            public NilInstance(Class fromClass) : base(fromClass) { }
        }
        public class TrueInstance : Instance {
            public override object? Object { get { return true; } }
            public override bool Boolean { get { return true; } }
            public override string Inspect() {
                return "true";
            }
            public TrueInstance(Class fromClass) : base(fromClass) { }
        }
        public class FalseInstance : Instance {
            public override object? Object { get { return false; } }
            public override bool Boolean { get { return false; } }
            public override string Inspect() {
                return "false";
            }
            public FalseInstance(Class fromClass) : base(fromClass) { }
        }
        public class StringInstance : Instance {
            string Value;
            public override object? Object { get { return Value; } }
            public override string String { get { return Value; } }
            public override string Inspect() {
                return "\"" + Value.Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
            }
            public override string LightInspect() {
                return Value;
            }
            public StringInstance(Class fromClass, string value) : base(fromClass) {
                Value = value;
            }
            public void SetValue(string value) {
                Value = value;
            }
        }
        public class SymbolInstance : Instance {
            string Value;
            public override object? Object { get { return Value; } }
            public override string String { get { return Value; } }

            bool IsStringSymbol;
            public override string Inspect() {
                if (IsStringSymbol) {
                    return ":\"" + Value.Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
                }
                else {
                    return ":" + Value;
                }
            }
            public override string LightInspect() {
                return Value;
            }
            public SymbolInstance(Class fromClass, string value) : base(fromClass) {
                Value = value;
                SetValue(value);
            }
            public void SetValue(string value) {
                Value = value;
                IsStringSymbol = Value.Any("(){}[]<>=+-*/%!?.,;@#&|~^$_".Contains) || Value.Any(char.IsWhiteSpace) || (Value.Length != 0 && Value[0].IsAsciiDigit());
            }
        }
        public class IntegerInstance : Instance {
            long Value;
            public override object? Object { get { return Value; } }
            public override long Integer { get { return Value; } }
            public override double Float { get { return Value; } }
            public override string Inspect() {
                return Value.ToString();
            }
            public IntegerInstance(Class fromClass, long value) : base(fromClass) {
                Value = value;
            }
            public void SetValue(long value) {
                Value = value;
            }
        }
        public class FloatInstance : Instance {
            double Value;
            public override object? Object { get { return Value; } }
            public override double Float { get { return Value; } }
            public override long Integer { get { return (long)Value; } }
            public override string Inspect() {
                if (double.IsPositiveInfinity(Value))
                    return "Infinity";
                else if (double.IsNegativeInfinity(Value))
                    return "-Infinity";

                string FloatString = Value.ToString();
                if (!FloatString.Contains('.'))
                    FloatString += ".0";
                return FloatString;
            }
            public FloatInstance(Class fromClass, double value) : base(fromClass) {
                Value = value;
            }
            public void SetValue(double value) {
                Value = value;
            }
        }
        public class ProcInstance : Instance {
            Method Value;
            public override object? Object { get { return Value; } }
            public override Method Proc { get { return Value; } }
            public ProcInstance(Class fromClass, Method value) : base(fromClass) {
                Value = value;
            }
            public void SetValue(Method value) {
                Value = value;
            }
        }
        public class ThreadInstance : Instance {
            public readonly ScriptThread ScriptThread;
            public override object? Object { get { return ScriptThread; } }
            public override ScriptThread Thread { get { return ScriptThread; } }
            public ThreadInstance(Class fromClass, Script fromScript) : base(fromClass) {
                ScriptThread = new(fromScript);
            }
            public void SetMethod(Method method) {
                Thread.Method = method;
            }
        }
        public class ScriptThread {
            public ThreadPhase Phase { get; private set; }
            public readonly Script FromScript;
            public readonly Script ThreadScript;
            public Method? Method;
            public ScriptThread(Script fromScript) {
                FromScript = fromScript;
                ThreadScript = new Script(FromScript.Interpreter, FromScript.AllowUnsafeApi);
                Phase = ThreadPhase.Idle;
            }
            public async Task Run(Instances? Arguments = null, Method? OnYield = null) {
                // If already running, wait until it's finished
                if (Phase != ThreadPhase.Idle) {
                    while (Phase != ThreadPhase.Completed)
                        await Task.Delay(10);
                    return;
                }
                // Increase thread counter
                FromScript.ThreadCount++;
                try {
                    // Create a new script
                    FromScript.CurrentObject.CopyTo(ThreadScript.CurrentObject);
                    Phase = ThreadPhase.Running;
                    // Call the method in the script
                    Task CallTask = Method!.Call(ThreadScript, null, Arguments, OnYield);
                    while (!ThreadScript.Stopping && !FromScript.Stopping && !CallTask.IsCompleted) {
                        await Task.Delay(10);
                    }
                    // Stop the script
                    ThreadScript.Stop();
                    Phase = ThreadPhase.Completed;
                }
                finally {
                    // Decrease thread counter
                    FromScript.ThreadCount--;
                }
            }
            public void Stop() {
                ThreadScript.Stop();
            }
            public enum ThreadPhase {
                Idle,
                Running,
                Completed
            }
        }
        public class RangeInstance : Instance {
            public IntegerInstance? Min;
            public IntegerInstance? Max;
            public Instance AppliedMin;
            public Instance AppliedMax;
            public bool IncludesMax;
            public override object? Object { get { return ToLongRange; } }
            public override LongRange Range { get { return ToLongRange; } }
            public override string Inspect() {
                return $"{(Min != null ? Min.Inspect() : "")}{(IncludesMax ? ".." : "...")}{(Max != null ? Max.Inspect() : "")}";
            }
            public RangeInstance(Class fromClass, IntegerInstance? min, IntegerInstance? max, bool includesMax) : base(fromClass) {
                Min = min;
                Max = max;
                IncludesMax = includesMax;
                (AppliedMin, AppliedMax) = Setup();
                Setup();
            }
            public void SetValue(IntegerInstance min, IntegerInstance max, bool includesMax) {
                Min = min;
                Max = max;
                IncludesMax = includesMax;
                Setup();
            }
            (Instance, Instance) Setup() {
                if (Min == null) {
                    AppliedMin = Max!.Module!.Interpreter.Nil;
                    AppliedMax = IncludesMax ? Max : new IntegerInstance((Class)Max.Module!, Max.Integer - 1);
                }
                else if (Max == null) {
                    AppliedMin = Min;
                    AppliedMax = Min!.Module!.Interpreter.Nil;
                }
                else {
                    AppliedMin = Min;
                    AppliedMax = IncludesMax ? Max : new IntegerInstance((Class)Max.Module!, Max.Integer - 1);
                }
                return (AppliedMin, AppliedMax);
            }
            LongRange ToLongRange => new(AppliedMin is IntegerInstance ? AppliedMin.Integer : null, AppliedMax is IntegerInstance ? AppliedMax.Integer : null);
        }
        public class ArrayInstance : Instance {
            List<Instance> Value;
            public override object? Object { get { return Value; } }
            public override List<Instance> Array { get { return Value; } }
            public override string Inspect() {
                return $"[{Value.InspectInstances()}]";
            }
            public override string LightInspect() {
                return Value.LightInspectInstances("\n");
            }
            public ArrayInstance(Class fromClass, List<Instance> value) : base(fromClass) {
                Value = value;
            }
            public void SetValue(List<Instance> value) {
                Value = value;
            }
        }
        public class HashInstance : Instance {
            Dictionary<Instance, Instance> Value;
            public Instance DefaultValue;
            public override object? Object { get { return Value; } }
            public override Dictionary<Instance, Instance> Hash { get { return Value; } }
            public override string Inspect() {
                return $"{{{Value.InspectInstances()}}}";
            }
            public HashInstance(Class fromClass, Dictionary<Instance, Instance> value, Instance defaultValue) : base(fromClass) {
                Value = value;
                DefaultValue = defaultValue;
            }
            public void SetValue(Dictionary<Instance, Instance> value, Instance defaultValue) {
                Value = value;
                DefaultValue = defaultValue;
            }
            public void SetValue(Dictionary<Instance, Instance> value) {
                Value = value;
            }
        }
        public class HashArgumentsInstance : Instance {
            public readonly HashInstance Value;
            public override string Inspect() {
                return $"Hash arguments instance: {{{Value.Inspect()}}}";
            }
            public HashArgumentsInstance(HashInstance value, Interpreter interpreter) : base(interpreter) {
                Value = value;
            }
        }
        public class ExceptionInstance : Instance {
            Exception Value;
            public override object? Object { get { return Value; } }
            public override Exception Exception { get { return Value; } }
            public ExceptionInstance(Class fromClass, string message) : base(fromClass) {
                Value = new Exception(message);
            }
            public void SetValue(string message) {
                Value = new Exception(message);
            }
        }
        public class TimeInstance : Instance {
            DateTimeOffset Value;
            public override object? Object { get { return Value; } }
            public override DateTimeOffset Time { get { return Value; } }
            public override string Inspect() {
                return Value.ToString(System.Globalization.CultureInfo.GetCultureInfo("ja-JP")); // yyyy/mm/dd format
            }
            public TimeInstance(Class fromClass, DateTimeOffset value) : base(fromClass) {
                Value = value;
            }
            public void SetValue(DateTimeOffset value) {
                Value = value;
            }
        }
        public abstract class PseudoInstance : Instance {
            public override ReactiveDictionary<string, Instance> InstanceVariables { get { throw new ApiException($"{GetType().Name} instance does not have instance variables"); } }
            public override ReactiveDictionary<string, Method> InstanceMethods { get { throw new ApiException($"{GetType().Name} instance does not have instance methods"); } }
            public PseudoInstance(Module module) : base(module) { }
            public PseudoInstance(Interpreter interpreter) : base(interpreter) { }
        }
        public class VariableReference : PseudoInstance {
            public Block? Block;
            public Instance? Instance;
            public Phase2Token Token;
            public bool IsLocalReference => Block == null && Instance == null;
            public override string Inspect() {
                return $"{(Block != null ? Block.GetType().Name : (Instance != null ? Instance.Inspect() : Token.Inspect()))} var ref in {Token.Inspect()}";
            }
            public VariableReference(Module module, Phase2Token token) : base(module) {
                Block = module;
                Token = token;
            }
            public VariableReference(Instance instance, Phase2Token token) : base(instance.Module!.Interpreter) {
                Instance = instance;
                Token = token;
            }
            public VariableReference(Phase2Token token, Interpreter interpreter) : base(interpreter) {
                Token = token;
            }
        }
        public class ScopeReference : PseudoInstance {
            public Scope Scope;
            public override string Inspect() {
                return Scope.GetType().Name;
            }
            public ScopeReference(Scope scope, Interpreter interpreter) : base(interpreter) {
                Scope = scope;
            }
        }
        public class ModuleReference : Instance {
            public override object? Object { get { return Module; } }
            public override Module ModuleRef { get { return Module!; } }
            public override string Inspect() {
                return Module!.Name;
            }
            public override string LightInspect() {
                return Module!.Name;
            }
            public ModuleReference(Module module) : base(module) { }
        }
        public class MethodReference : PseudoInstance {
            readonly Method Method;
            public override object? Object { get { return Method; } }
            public override Method MethodRef { get { return Method; } }
            public override string Inspect() {
                return Method.ToString()!;
            }
            public MethodReference(Method method, Interpreter interpreter) : base(interpreter) {
                Method = method;
            }
        }
        public class Method {
            public string? Name;
            public readonly Module? Parent;
            public Func<MethodInput, Task<Instance>> Function {get; private set;}
            public readonly IntRange ArgumentCountRange;
            public readonly List<MethodArgumentExpression> ArgumentNames;
            public readonly bool Unsafe;
            public readonly AccessModifier AccessModifier;
            public Method(Func<MethodInput, Task<Instance>> function, IntRange? argumentCountRange, List<MethodArgumentExpression>? argumentNames = null, bool IsUnsafe = false, AccessModifier accessModifier = AccessModifier.Public, Module? parent = null) {
                Function = function;
                ArgumentCountRange = argumentCountRange ?? new IntRange();
                ArgumentNames = argumentNames ?? new();
                Unsafe = IsUnsafe;
                AccessModifier = accessModifier;
                Parent = parent;
            }
            public Method(Func<MethodInput, Task<Instance>> function, Range argumentCountRange, List<MethodArgumentExpression>? argumentNames = null, bool IsUnsafe = false, AccessModifier accessModifier = AccessModifier.Public, Module? parent = null) {
                Function = function;
                ArgumentCountRange = new IntRange(argumentCountRange);
                ArgumentNames = argumentNames ?? new();
                Unsafe = IsUnsafe;
                AccessModifier = accessModifier;
                Parent = parent;
            }
            public Method(Func<MethodInput, Task<Instance>> function, int argumentCount, List<MethodArgumentExpression>? argumentNames = null, bool IsUnsafe = false, AccessModifier accessModifier = AccessModifier.Public, Module? parent = null) {
                Function = function;
                ArgumentCountRange = new IntRange(argumentCount, argumentCount);
                ArgumentNames = argumentNames ?? new();
                Unsafe = IsUnsafe;
                AccessModifier = accessModifier;
                Parent = parent;
            }
            public async Task<Instance> Call(Script Script, Instance? OnInstance, Instances? Arguments = null, Method? OnYield = null, BreakHandleType BreakHandleType = BreakHandleType.Invalid, bool CatchReturn = true) {
                if (Unsafe && !Script.AllowUnsafeApi)
                    throw new RuntimeException($"{Script.ApproximateLocation}: The method '{Name}' is unavailable since 'AllowUnsafeApi' is disabled for this script.");
                if (AccessModifier == AccessModifier.Private) {
                    if (Parent != null && Parent != Script.Interpreter.RootModule && Script.CurrentModule != Parent)
                        throw new RuntimeException($"{Script.ApproximateLocation}: Private method '{Name}' called {(Parent != null ? $"for {Parent.Name}" : "")}");
                }
                else if (AccessModifier == AccessModifier.Protected) {
                    if (Parent != null && Parent != Script.Interpreter.RootModule && !Script.CurrentModule.InheritsFrom(Parent))
                        throw new RuntimeException($"{Script.ApproximateLocation}: Protected method '{Name}' called {(Parent != null ? $"for {Parent.Name}" : "")}");
                }

                Arguments ??= new Instances();
                if (ArgumentCountRange.IsInRange(Arguments.Count)) {
                    // Create temporary scope
                    if (OnInstance != null) {
                        Script.CurrentObject.Push(OnInstance.Module!);
                        Script.CurrentObject.Push(OnInstance);
                    }
                    else if (Parent != null) {
                        Script.CurrentObject.Push(Parent);
                    }
                    MethodScope MethodScope = new(this);
                    Script.CurrentObject.Push(MethodScope);
                    
                    Instance ReturnValue;
                    try {
                        // Create method input
                        MethodInput Input = new(Script, OnInstance, Arguments, OnYield);
                        // Set argument variables
                        await SetArgumentVariables(MethodScope, Input);
                        // Call method
                        ReturnValue = await Function(Input);
                    }
                    catch (BreakException) {
                        if (BreakHandleType == BreakHandleType.Rethrow)
                            throw;
                        else if (BreakHandleType == BreakHandleType.Destroy)
                            ReturnValue = Script.Interpreter.Nil;
                        else
                            throw new SyntaxErrorException($"{Script.ApproximateLocation}: Invalid break (break must be in a loop)");
                    }
                    catch (ReturnException Ex) when (CatchReturn) {
                        ReturnValue = Ex.Instance;
                    }
                    finally {
                        // Step back a scope
                        Script.CurrentObject.Pop();
                        if (OnInstance != null) {
                            Script.CurrentObject.Pop();
                            Script.CurrentObject.Pop();
                        }
                        else if (Parent != null) {
                            Script.CurrentObject.Pop();
                        }
                    }
                    // Return method return value
                    return ReturnValue;
                }
                else {
                    throw new RuntimeException($"{Script.ApproximateLocation}: Wrong number of arguments for '{Name}' (given {Arguments.Count}, expected {ArgumentCountRange})");
                }
            }
            public void ChangeFunction(Func<MethodInput, Task<Instance>> function) {
                Function = function;
            }
            public async Task SetArgumentVariables(Scope Scope, MethodInput Input) {
                Instances Arguments = Input.Arguments;
                // Set argument variables
                int ArgumentNameIndex = 0;
                int ArgumentIndex = 0;
                while (ArgumentNameIndex < ArgumentNames.Count) {
                    MethodArgumentExpression ArgumentName = ArgumentNames[ArgumentNameIndex];
                    string ArgumentIdentifier = ArgumentName.ArgumentName.Value!;
                    // Declare argument as variable in local scope
                    if (ArgumentIndex < Arguments.Count) {
                        // Splat argument
                        if (ArgumentName.SplatType == SplatType.Single) {
                            // Add splat arguments while there will be enough remaining arguments
                            List<Instance> SplatArguments = new();
                            while (Arguments.Count - ArgumentIndex >= ArgumentNames.Count - ArgumentNameIndex) {
                                SplatArguments.Add(Arguments[ArgumentIndex]);
                                ArgumentIndex++;
                            }
                            if (SplatArguments.Count != 0)
                                ArgumentIndex--;
                            // Add extra ungiven double splat argument if available
                            if (ArgumentNameIndex + 1 < ArgumentNames.Count && ArgumentNames[ArgumentNameIndex + 1].SplatType == SplatType.Double
                                && Arguments[^1] is not HashArgumentsInstance)
                            {
                                SplatArguments.Add(Arguments[ArgumentIndex]);
                                ArgumentIndex++;
                            }
                            // Create array from splat arguments
                            ArrayInstance SplatArgumentsArray = new(Input.Interpreter.Array, SplatArguments);
                            // Add array to scope
                            lock (Scope.LocalVariables)
                                Scope.LocalVariables.Add(ArgumentIdentifier, SplatArgumentsArray);
                        }
                        // Double splat argument
                        else if (ArgumentName.SplatType == SplatType.Double && Arguments[^1] is HashArgumentsInstance DoubleSplatArgumentsHash) {
                            // Add hash to scope
                            lock (Scope.LocalVariables)
                                Scope.LocalVariables.Add(ArgumentIdentifier, DoubleSplatArgumentsHash.Value);
                        }
                        // Normal argument
                        else {
                            lock (Scope.LocalVariables)
                                Scope.LocalVariables.Add(ArgumentIdentifier, Arguments[ArgumentIndex]);
                        }
                    }
                    // Optional argument not given
                    else {
                        Instance DefaultValue = ArgumentName.DefaultValue != null ? (await Input.Script.InterpretExpressionAsync(ArgumentName.DefaultValue)) : Input.Script.Interpreter.Nil;
                        lock (Scope.LocalVariables)
                            Scope.LocalVariables.Add(ArgumentIdentifier, DefaultValue);
                    }
                    ArgumentNameIndex++;
                    ArgumentIndex++;
                }
            }
        }
        public class MethodInput {
            public readonly Script Script;
            public readonly Interpreter Interpreter;
            public readonly Instances Arguments;
            public readonly Method? OnYield;
            public Instance Instance => InputInstance!;
            readonly Instance? InputInstance;
            public MethodInput(Script script, Instance? instance, Instances arguments, Method? onYield = null) {
                Script = script;
                Interpreter = script.Interpreter;
                InputInstance = instance;
                Arguments = arguments;
                OnYield = onYield;
            }
            public DebugLocation Location => Script.ApproximateLocation;
        }
        public class IntRange {
            public readonly int? Min;
            public readonly int? Max;
            public IntRange(int? min = null, int? max = null) {
                Min = min;
                Max = max;
            }
            public IntRange(Range range) {
                if (range.Start.IsFromEnd) {
                    Min = null;
                    Max = range.End.Value;
                }
                else if (range.End.IsFromEnd) {
                    Min = range.Start.Value;
                    Max = null;
                }
                else {
                    Min = range.Start.Value;
                    Max = range.End.Value;
                }
            }
            public bool IsInRange(int Number) {
                if (Min != null && Number < Min) return false;
                if (Max != null && Number > Max) return false;
                return true;
            }
            public override string ToString() {
                if (Min == Max) {
                    if (Min == null) {
                        return "any";
                    }
                    else {
                        return $"{Min}";
                    }
                }
                else {
                    if (Min == null)
                        return $"{Max}";
                    else if (Max == null)
                        return $"{Min}+";
                    else
                        return $"{Min}..{Max}";
                }
            }
            public string Serialise() {
                return $"new {typeof(IntRange).PathTo()}({(Min != null ? Min : "null")}, {(Max != null ? Max : "null")})";
            }
        }
        public class LongRange {
            public readonly long? Min;
            public readonly long? Max;
            public LongRange(long? min = null, long? max = null) {
                Min = min;
                Max = max;
            }
            public bool IsInRange(long Number) {
                if (Min != null && Number < Min) return false;
                if (Max != null && Number > Max) return false;
                return true;
            }
            public bool IsInRange(double Number) {
                if (Min != null && Number < Min) return false;
                if (Max != null && Number > Max) return false;
                return true;
            }
            public override string ToString() {
                if (Min == Max) {
                    if (Min == null) {
                        return "any";
                    }
                    else {
                        return $"{Min}";
                    }
                }
                else {
                    if (Min == null)
                        return $"{Max}";
                    else if (Max == null)
                        return $"{Min}+";
                    else
                        return $"{Min}..{Max}";
                }
            }
            public string Serialise() {
                return $"new {typeof(LongRange).PathTo()}({(Min != null ? Min : "null")}, {(Max != null ? Max : "null")})";
            }
        }
        public class WeakEvent<TDelegate> where TDelegate : class {
            readonly List<WeakReference> Subscribers = new();

            public void Add(TDelegate Handler) {
                // Remove any dead references
                Subscribers.RemoveAll(WeakRef => !WeakRef.IsAlive);
                // Add the new handler as a weak reference
                Subscribers.Add(new WeakReference(Handler));
            }

            public void Remove(TDelegate Handler) {
                // Remove the handler from the list
                Subscribers.RemoveAll(WeakRef => WeakRef.Target == Handler);
            }

            public void Raise(Action<TDelegate> Action) {
                // Invoke the action for each subscriber that is still alive
                foreach (WeakReference WeakRef in Subscribers) {
                    if (WeakRef.Target is TDelegate Target) {
                        Action(Target);
                    }
                }
            }
        }
        public class ReactiveDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull {
            private readonly WeakEvent<DictionarySet> SetEvent = new();
            private readonly WeakEvent<DictionaryRemoved> RemovedEvent = new();

            public delegate void DictionarySet(TKey Key, TValue NewValue);
            public event DictionarySet Set {
                add => SetEvent.Add(value);
                remove => SetEvent.Remove(value);
            }

            public delegate void DictionaryRemoved(TKey Key);
            public event DictionaryRemoved Removed {
                add => RemovedEvent.Add(value);
                remove => RemovedEvent.Remove(value);
            }

            public new TValue this[TKey Key] {
                get => base[Key];
                set {
                    TrySetMethodName(Key, value);
                    base[Key] = value;
                    SetEvent.Raise(Handler => Handler(Key, value));
                }
            }
            public TValue this[params TKey[] Keys] {
                set {
                    foreach (TKey Key in Keys) {
                        TrySetMethodName(Key, value);
                        base[Key] = value;
                        SetEvent.Raise(Handler => Handler(Key, value));
                    }
                }
            }
            public new void Add(TKey Key, TValue Value) {
                TrySetMethodName(Key, Value);
                base.Add(Key, Value);
                SetEvent.Raise(Handler => Handler(Key, Value));
            }
            public new bool Remove(TKey Key) {
                if (base.Remove(Key)) {
                    RemovedEvent.Raise(Handler => Handler(Key));
                    return true;
                }
                return false;
            }

            static void TrySetMethodName(TKey Key, TValue Value) {
                if (Key is string MethodName && Value is Method Method) {
                    Method.Name = MethodName;
                }
            }
        }
        public class Instances {
            // At least one of Instance or InstanceList will be null
            readonly Instance? Instance;
            readonly List<Instance>? InstanceList;
            public readonly int Count;

            public Instances(Instance? instance = null) {
                Instance = instance;
                Count = instance != null ? 1 : 0;
            }
            public Instances(List<Instance> instanceList) {
                InstanceList = instanceList;
                Count = instanceList.Count;
            }
            public Instances(params Instance[] instanceArray) {
                InstanceList = instanceArray.ToList();
                Count = InstanceList.Count;
            }
            public static implicit operator Instances(Instance Instance) {
                return new Instances(Instance);
            }
            public static implicit operator Instances(List<Instance> InstanceList) {
                return new Instances(InstanceList);
            }
            public static implicit operator Instance(Instances Instances) {
                if (Instances.Count != 1) {
                    if (Instances.Count == 0)
                        throw new RuntimeException($"Cannot implicitly cast Instances to Instance because there are none");
                    else
                        throw new RuntimeException($"Cannot implicitly cast Instances to Instance because {Instances.Count - 1} instances would be overlooked");
                }
                return Instances[0];
            }
            public Instance this[Index i] => InstanceList != null ? InstanceList[i] : (i.Value == 0 && Instance != null ? Instance : throw new ApiException("Index was outside the range of the instances"));
            public IEnumerator<Instance> GetEnumerator() {
                if (InstanceList != null) {
                    for (int i = 0; i < InstanceList.Count; i++) {
                        yield return InstanceList[i];
                    }
                }
                else if (Instance != null) {
                    yield return Instance;
                }
            }
            public Instance SingleInstance { get {
                if (Count == 1) {
                    return this[0];
                }
                else {
                    throw new SyntaxErrorException($"Unexpected instances (expected one, got {Count})");
                }
            } }
            public List<Instance> MultiInstance { get {
                if (InstanceList != null) {
                    return InstanceList;
                }
                else if (Instance != null) {
                    return new List<Instance>() { Instance };
                }
                else {
                    return new List<Instance>();
                }
            } }
        }

        public async Task Warn(string Message) {
            await Interpreter.RootInstance.InstanceMethods["warn"].Call(this, new ModuleReference(Interpreter.RootModule), new StringInstance(Interpreter.String, Message));
        }
        public Module CreateModule(string Name, Module? Parent = null, Module? InheritsFrom = null) {
            Parent ??= Interpreter.RootModule;
            Module NewModule = new(Name, Parent, InheritsFrom);
            Parent.Constants[Name] = new ModuleReference(NewModule);
            return NewModule;
        }
        public Class CreateClass(string Name, Module? Parent = null, Module? InheritsFrom = null) {
            Parent ??= Interpreter.RootModule;
            Class NewClass = new(Name, Parent, InheritsFrom);
            Parent.Constants[Name] = new ModuleReference(NewClass);
            return NewClass;
        }
        public Method CreateMethod(Func<MethodInput, Task<Instance>> Function, Range ArgumentCountRange, bool IsUnsafe = false) {
            Method NewMethod = new(Function, ArgumentCountRange, IsUnsafe: IsUnsafe, accessModifier: CurrentAccessModifier, parent: CurrentModule);
            return NewMethod;
        }
        public Method CreateMethod(Func<MethodInput, Task<Instance>> Function, IntRange? ArgumentCountRange, bool IsUnsafe = false) {
            Method NewMethod = new(Function, ArgumentCountRange, IsUnsafe: IsUnsafe, accessModifier: CurrentAccessModifier, parent: CurrentModule);
            return NewMethod;
        }
        public Method CreateMethod(Func<MethodInput, Task<Instance>> Function, int ArgumentCount, bool IsUnsafe = false) {
            Method NewMethod = new(Function, ArgumentCount, IsUnsafe: IsUnsafe, accessModifier: CurrentAccessModifier, parent: CurrentModule);
            return NewMethod;
        }
        async Task<T> CreateTemporaryClassScope<T>(Module Module, Func<Task<T>> Do) {
            // Create temporary class/module scope
            CurrentObject.Push(Module);
            try {
                // Do action
                return await Do();
            }
            finally {
                // Step back a class/module
                CurrentObject.Pop();
            }
        }
        async Task<T> CreateTemporaryInstanceScope<T>(Instance Instance, Func<Task<T>> Do) {
            // Create temporary instance scope
            CurrentObject.Push(Instance);
            try {
                // Do action
                return await Do();
            }
            finally {
                // Step back an instance
                CurrentObject.Pop();
            }
        }
        async Task<T> CreateTemporaryScope<T>(Scope Scope, Func<Task<T>> Do) {
            // Create temporary scope
            CurrentObject.Push(Scope);
            try {
                // Do action
                return await Do();
            }
            finally {
                // Step back a scope
                CurrentObject.Pop();
            }
        }
        async Task<T> CreateTemporaryScope<T>(Func<Task<T>> Do) {
            return await CreateTemporaryScope(new Scope(), Do);
        }
        async Task CreateTemporaryClassScope(Module Module, Func<Task> Do) {
            // Create temporary class/module scope
            CurrentObject.Push(Module);
            try {
                // Do action
                await Do();
            }
            finally {
                // Step back a class/module
                CurrentObject.Pop();
            }
        }
        async Task CreateTemporaryInstanceScope(Instance Instance, Func<Task> Do) {
            // Create temporary instance scope
            CurrentObject.Push(Instance);
            try {
                // Do action
                await Do();
            }
            finally {
                // Step back an instance
                CurrentObject.Pop();
            }
        }
        async Task CreateTemporaryScope(Scope Scope, Func<Task> Do) {
            // Create temporary scope
            CurrentObject.Push(Scope);
            try {
                // Do action
                await Do();
            }
            finally {
                // Step back a scope
                CurrentObject.Pop();
            }
        }
        async Task CreateTemporaryScope(Func<Task> Do) {
            await CreateTemporaryScope(new Scope(), Do);
        }
        public SymbolInstance GetSymbol(string Value) {
            if (Interpreter.Symbols.TryGetValue(Value, out SymbolInstance? FindSymbolInstance)) {
                return FindSymbolInstance;
            }
            else {
                SymbolInstance SymbolInstance = new(Interpreter.Symbol, Value);
                Interpreter.Symbols[Value] = SymbolInstance;
                return SymbolInstance;
            }
        }
        public bool TryGetLocalVariable(string Name, out Instance? LocalVariable) {
            foreach (object Object in CurrentObject) {
                if (Object is Block Block && Block.LocalVariables.TryGetValue(Name, out Instance? FindLocalVariable)) {
                    LocalVariable = FindLocalVariable;
                    return true;
                }
            }
            LocalVariable = null;
            return false;
        }
        public bool TryGetLocalConstant(string Name, out Instance? LocalConstant) {
            foreach (object Object in CurrentObject) {
                if (Object is Block Block && Block.Constants.TryGetValue(Name, out Instance? FindLocalConstant)) {
                    LocalConstant = FindLocalConstant;
                    return true;
                }
            }
            LocalConstant = null;
            return false;
        }
        public bool TryGetLocalInstanceMethod(string Name, out Method? LocalInstanceMethod) {
            foreach (object Object in CurrentObject) {
                if (Object is Instance Instance && (Instance is PseudoInstance ? Instance.Module!.InstanceMethods : Instance.InstanceMethods).TryGetValue(Name, out Method? FindLocalInstanceMethod)) {
                    LocalInstanceMethod = FindLocalInstanceMethod;
                    return true;
                }
            }
            LocalInstanceMethod = null;
            return false;
        }
        internal Method? ToYieldMethod(Method? Current) {
            // This makes yield methods (do ... end) be called in the scope they're called in, not the scope of the instance/class.
            // e.g. 5.times do ... end should be called in the scope of the line, not in the instance of 5.
            // If you've changed this function and are receiving errors, ensure you're referencing Input.Script and not this script.
            if (Current != null) {
                Func<MethodInput, Task<Instance>> CurrentFunction = Current.Function;
                Stack<object> OriginalSnapshot = new(CurrentObject);
                Current.ChangeFunction(async Input => {
                    Stack<object> TemporarySnapshot = new(Input.Script.CurrentObject);
                    try {
                        Input.Script.CurrentObject.ReplaceContentsWith(OriginalSnapshot);
                        return await Input.Script.CreateTemporaryScope(async () => {
                            await Current.SetArgumentVariables(Input.Script.CurrentScope, Input);
                            return await CurrentFunction(Input);
                        });
                    }
                    finally {
                        Input.Script.CurrentObject.ReplaceContentsWith(TemporarySnapshot);
                    }
                });
            }
            return Current;
        }

        async Task<Instance> InterpretMethodCallExpression(MethodCallExpression MethodCallExpression) {
            Instance MethodPath = await InterpretExpressionAsync(MethodCallExpression.MethodPath, ReturnType.FoundVariable);
            if (MethodPath is VariableReference MethodReference) {
                // Static method
                if (MethodReference.Block != null) {
                    // Get class/module which owns method
                    Module MethodModule = MethodReference.Block as Module ?? CurrentModule;
                    // Get instance of the class/module which owns method
                    Instance MethodOwner;
                    if (MethodCallExpression.MethodPath is PathExpression MethodCallPathExpression) {
                        MethodOwner = await InterpretExpressionAsync(MethodCallPathExpression.ParentObject);
                    }
                    else {
                        MethodOwner = new ModuleReference(MethodModule);
                    }
                    // Call class method
                    bool Found = MethodModule.Methods.TryGetValue(MethodReference.Token.Value!, out Method? StaticMethod);
                    if (Found) {
                        return await StaticMethod!.Call(
                            this, MethodOwner, await InterpretExpressionsAsync(MethodCallExpression.Arguments), MethodCallExpression.OnYield?.ToYieldMethod(this, CurrentOnYield)
                        );
                    }
                    else {
                        throw new RuntimeException($"{MethodReference.Token.Location}: Undefined method '{MethodReference.Token.Value!}' for {CurrentInstance.Module!.Name}");
                    }
                }
                // Instance method
                else {
                    // Local
                    if (MethodReference.IsLocalReference) {
                        // Call local instance method
                        bool Found = TryGetLocalInstanceMethod(MethodReference.Token.Value!, out Method? LocalInstanceMethod);
                        if (Found) {
                            return await LocalInstanceMethod!.Call(
                                this, CurrentInstance, await InterpretExpressionsAsync(MethodCallExpression.Arguments), MethodCallExpression.OnYield?.ToYieldMethod(this, CurrentOnYield)
                            );
                        }
                        else {
                            throw new RuntimeException($"{MethodReference.Token.Location}: Undefined method '{MethodReference.Token.Value!}'");
                        }
                    }
                    // Path
                    else {
                        Instance MethodInstance = MethodReference.Instance!;
                        // Call instance method
                        bool Found = MethodInstance.InstanceMethods.TryGetValue(MethodReference.Token.Value!, out Method? PathInstanceMethod);
                        if (Found) {
                            return await PathInstanceMethod!.Call(
                                this, MethodInstance, await InterpretExpressionsAsync(MethodCallExpression.Arguments), MethodCallExpression.OnYield?.ToYieldMethod(this, CurrentOnYield)
                            );
                        }
                        else {
                            throw new RuntimeException($"{MethodReference.Token.Location}: Undefined method '{MethodReference.Token.Value!}' for {CurrentInstance.Module!.Name}");
                        }
                    }
                }
            }
            else {
                throw new InternalErrorException($"{MethodCallExpression.Location}: MethodPath should be VariableReference, not {MethodPath.GetType().Name}");
            }
        }
        async Task<Instance> InterpretObjectTokenExpression(ObjectTokenExpression ObjectTokenExpression, ReturnType ReturnType) {
            // Path
            if (ObjectTokenExpression is PathExpression PathExpression) {
                Instance ParentInstance = await InterpretExpressionAsync(PathExpression.ParentObject);
                // Static method
                if (ParentInstance is ModuleReference ParentModule) {
                    // Method
                    if (ReturnType != ReturnType.HypotheticalVariable) {
                        // Found
                        if (ParentModule.Module!.Methods.TryGetValue(PathExpression.Token.Value!, out Method? FindMethod)) {
                            // Call class/module method
                            if (ReturnType == ReturnType.InterpretResult) {
                                return await CreateTemporaryClassScope(ParentModule.Module, async () =>
                                    await FindMethod.Call(this, ParentModule)
                                );
                            }
                            // Return method
                            else {
                                return new VariableReference(ParentModule.Module, PathExpression.Token);
                            }
                        }
                        // Error
                        else {
                            throw new RuntimeException($"{PathExpression.Token.Location}: Undefined method '{PathExpression.Token.Value!}' for {ParentModule.Module.Name}");
                        }
                    }
                    // New method
                    else {
                        return new VariableReference(ParentModule.Module!, PathExpression.Token);
                    }
                }
                // Instance method
                else {
                    // Method
                    if (ReturnType != ReturnType.HypotheticalVariable) {
                        // Method
                        if (ParentInstance.InstanceMethods.TryGetValue(PathExpression.Token.Value!, out Method? FindMethod)) {
                            // Call instance method
                            if (ReturnType == ReturnType.InterpretResult) {
                                return await CreateTemporaryInstanceScope(ParentInstance, async () =>
                                    await FindMethod.Call(this, ParentInstance)
                                );
                            }
                            // Return method
                            else {
                                return new VariableReference(ParentInstance, PathExpression.Token);
                            }
                        }
                        // Error
                        else {
                            throw new RuntimeException($"{PathExpression.Token.Location}: Undefined method '{PathExpression.Token.Value!}' for {ParentInstance.Inspect()}");
                        }
                    }
                    // New method
                    else {
                        return new VariableReference(ParentInstance, PathExpression.Token);
                    }
                }
            }
            // Constant Path
            else if (ObjectTokenExpression is ConstantPathExpression ConstantPathExpression) {
                Instance ParentInstance = await InterpretExpressionAsync(ConstantPathExpression.ParentObject);
                // Constant
                if (ReturnType != ReturnType.HypotheticalVariable) {
                    // Constant
                    if (ParentInstance.Module!.Constants.TryGetValue(ConstantPathExpression.Token.Value!, out Instance? ConstantValue)) {
                        // Return constant
                        if (ReturnType == ReturnType.InterpretResult) {
                            return ConstantValue;
                        }
                        // Return constant reference
                        else {
                            return new VariableReference(ParentInstance.Module, ConstantPathExpression.Token);
                        }
                    }
                    // Error
                    else {
                        throw new RuntimeException($"{ConstantPathExpression.Token.Location}: Uninitialized constant {ConstantPathExpression.Inspect()}");
                    }
                }
                // New constant
                else {
                    return new VariableReference(ParentInstance.Module!, ConstantPathExpression.Token);
                }
            }
            // Local
            else {
                // Literal
                if (ObjectTokenExpression.Token.IsObjectToken) {
                    return await Instance.CreateFromToken(this, ObjectTokenExpression.Token);
                }
                else {
                    if (ReturnType != ReturnType.HypotheticalVariable) {
                        switch (ObjectTokenExpression.Token.Type) {
                            // Local variable or method
                            case Phase2TokenType.LocalVariableOrMethod: {
                                // Local variable (priority)
                                if (TryGetLocalVariable(ObjectTokenExpression.Token.Value!, out Instance? Value)) {
                                    // Return local variable value
                                    if (ReturnType == ReturnType.InterpretResult) {
                                        return Value!;
                                    }
                                    // Return local variable reference
                                    else {
                                        return new VariableReference(ObjectTokenExpression.Token, Interpreter);
                                    }
                                }
                                // Method
                                else if (TryGetLocalInstanceMethod(ObjectTokenExpression.Token.Value!, out Method? Method)) {
                                    // Call local method
                                    if (ReturnType == ReturnType.InterpretResult) {
                                        return await Method!.Call(this, CurrentInstance);
                                    }
                                    // Return method reference
                                    else {
                                        return new VariableReference(ObjectTokenExpression.Token, Interpreter);
                                    }
                                }
                                // Undefined
                                else {
                                    throw new RuntimeException($"{ObjectTokenExpression.Token.Location}: Undefined local variable or method '{ObjectTokenExpression.Token.Value!}' for {CurrentBlock}");
                                }
                            }
                            // Global variable
                            case Phase2TokenType.GlobalVariable: {
                                if (Interpreter.GlobalVariables.TryGetValue(ObjectTokenExpression.Token.Value!, out Instance? Value)) {
                                    // Return global variable value
                                    if (ReturnType == ReturnType.InterpretResult) {
                                        return Value;
                                    }
                                    // Return global variable reference
                                    else {
                                        return new VariableReference(ObjectTokenExpression.Token, Interpreter);
                                    }
                                }
                                else {
                                    return Interpreter.Nil;
                                }
                            }
                            // Constant
                            case Phase2TokenType.ConstantOrMethod: {
                                // Constant (priority)
                                if (TryGetLocalConstant(ObjectTokenExpression.Token.Value!, out Instance? ConstantValue)) {
                                    // Return constant value
                                    if (ReturnType == ReturnType.InterpretResult) {
                                        return ConstantValue!;
                                    }
                                    // Return constant reference
                                    else {
                                        return new VariableReference(ObjectTokenExpression.Token, Interpreter);
                                    }
                                }
                                // Method
                                else if (TryGetLocalInstanceMethod(ObjectTokenExpression.Token.Value!, out Method? Method)) {
                                    // Call local method
                                    if (ReturnType == ReturnType.InterpretResult) {
                                        return await Method!.Call(this, CurrentInstance);
                                    }
                                    // Return method reference
                                    else {
                                        return new VariableReference(ObjectTokenExpression.Token, Interpreter);
                                    }
                                }
                                // Uninitialized
                                else {
                                    throw new RuntimeException($"{ObjectTokenExpression.Token.Location}: Uninitialized constant '{ObjectTokenExpression.Token.Value!}' for {CurrentModule.Name}");
                                }
                            }
                            // Instance variable
                            case Phase2TokenType.InstanceVariable: {
                                if (CurrentInstance.InstanceVariables.TryGetValue(ObjectTokenExpression.Token.Value!, out Instance? Value)) {
                                    // Return instance variable value
                                    if (ReturnType == ReturnType.InterpretResult) {
                                        return Value;
                                    }
                                    // Return instance variable reference
                                    else {
                                        return new VariableReference(ObjectTokenExpression.Token, Interpreter);
                                    }
                                }
                                else {
                                    return Interpreter.Nil;
                                }
                            }
                            // Class variable
                            case Phase2TokenType.ClassVariable: {
                                if (CurrentModule.ClassVariables.TryGetValue(ObjectTokenExpression.Token.Value!, out Instance? Value)) {
                                    // Return class variable value
                                    if (ReturnType == ReturnType.InterpretResult) {
                                        return Value;
                                    }
                                    // Return class variable reference
                                    else {
                                        return new VariableReference(ObjectTokenExpression.Token, Interpreter);
                                    }
                                }
                                else {
                                    throw new RuntimeException($"{ObjectTokenExpression.Token.Location}: Uninitialized class variable '{ObjectTokenExpression.Token.Value!}' for {CurrentModule}");
                                }
                            }
                            // Symbol
                            case Phase2TokenType.Symbol: {
                                return GetSymbol(ObjectTokenExpression.Token.Value!);
                            }
                            // Error
                            default:
                                throw new InternalErrorException($"{ObjectTokenExpression.Token.Location}: Unknown variable type {ObjectTokenExpression.Token.Type}");
                        }
                    }
                    // Variable
                    else {
                        return new VariableReference(ObjectTokenExpression.Token, Interpreter);
                    }
                }
            }
        }
        async Task<Instance> InterpretIfExpression(IfExpression IfExpression) {
            if (IfExpression.Condition == null || (await InterpretExpressionAsync(IfExpression.Condition)).IsTruthy != IfExpression.Inverse) {
                return await InternalInterpretAsync(IfExpression.Statements, CurrentOnYield);
            }
            return Interpreter.Nil;
        }
        async Task<Instance> InterpretRescueExpression(RescueExpression RescueExpression) {
            try {
                await InterpretExpressionAsync(RescueExpression.Statement);
            }
            catch (Exception Ex) when (Ex is not NonErrorException) {
                await InterpretExpressionAsync(RescueExpression.RescueStatement);
            }
            return Interpreter.Nil;
        }
        async Task<Instance> InterpretTernaryExpression(TernaryExpression TernaryExpression) {
            bool ConditionIsTruthy = (await InterpretExpressionAsync(TernaryExpression.Condition)).IsTruthy;
            if (ConditionIsTruthy) {
                return await InterpretExpressionAsync(TernaryExpression.ExpressionIfTrue);
            }
            else {
                return await InterpretExpressionAsync(TernaryExpression.ExpressionIfFalse);
            }
        }
        async Task<Instance> InterpretCaseExpression(CaseExpression CaseExpression) {
            Instance Subject = await InterpretExpressionAsync(CaseExpression.Subject);
            foreach (IfExpression Branch in CaseExpression.Branches) {
                // Check if when statements apply
                bool WhenApplies = false;
                if (Branch.Condition != null) {
                    Instance ConditionObject = await InterpretExpressionAsync(Branch.Condition);
                    if (ConditionObject.InstanceMethods.TryGetValue("===", out Method? TripleEquality)) {
                        if ((await TripleEquality.Call(this, ConditionObject, Subject)).IsTruthy) {
                            WhenApplies = true;
                        }
                    }
                    else {
                        throw new RuntimeException($"{Branch.Location}: Case 'when' instance must have an '===' method");
                    }
                }
                else {
                    WhenApplies = true;
                }
                // Run when statements
                if (WhenApplies) {
                    return await InternalInterpretAsync(Branch.Statements, CurrentOnYield);
                }
            }
            return Interpreter.Nil;
        }
        async Task<ArrayInstance> InterpretArrayExpression(ArrayExpression ArrayExpression) {
            List<Instance> Items = new();
            foreach (Expression Item in ArrayExpression.Expressions) {
                Items.Add(await InterpretExpressionAsync(Item));
            }
            return new ArrayInstance(Interpreter.Array, Items);
        }
        async Task<HashInstance> InterpretHashExpression(HashExpression HashExpression) {
            Dictionary<Instance, Instance> Items = new();
            foreach (KeyValuePair<Expression, Expression> Item in HashExpression.Expressions) {
                Items.Add(await InterpretExpressionAsync(Item.Key), await InterpretExpressionAsync(Item.Value));
            }
            return new HashInstance(Interpreter.Hash, Items, Interpreter.Nil);
        }
        async Task<Instance> InterpretWhileExpression(WhileExpression WhileExpression) {
            while ((await InterpretExpressionAsync(WhileExpression.Condition!)).IsTruthy != WhileExpression.Inverse) {
                try {
                    await InternalInterpretAsync(WhileExpression.Statements, CurrentOnYield);
                }
                catch (BreakException) {
                    break;
                }
                catch (RetryException) {
                    throw new SyntaxErrorException($"{ApproximateLocation}: Retry not valid in while loop");
                }
                catch (RedoException) {
                    continue;
                }
                catch (NextException) {
                    continue;
                }
                catch (LoopControlException Ex) {
                    throw new SyntaxErrorException($"{ApproximateLocation}: {Ex.GetType().Name} not valid in while loop");
                }
            }
            return Interpreter.Nil;
        }
        async Task<Instance> InterpretWhileStatement(WhileStatement WhileStatement) {
            // Run statements
            await CreateTemporaryScope(async () =>
                await InterpretExpressionAsync(WhileStatement.WhileExpression)
            );
            return Interpreter.Nil;
        }
        async Task<Instance> InterpretForStatement(ForStatement ForStatement) {
            Instance InResult = await InterpretExpressionAsync(ForStatement.InExpression);
            if (InResult.InstanceMethods.TryGetValue("each", out Method? EachMethod)) {
                await EachMethod.Call(this, InResult, OnYield: ForStatement.BlockStatementsMethod);
            }
            else {
                throw new RuntimeException($"{ForStatement.Location}: The instance must have an 'each' method to iterate with 'for'");
            }
            return Interpreter.Nil;
        }
        async Task<Instance> InterpretLogicalExpression(LogicalExpression LogicalExpression) {
            Instance Left = await InterpretExpressionAsync(LogicalExpression.Left);
            switch (LogicalExpression.LogicType) {
                case LogicalExpression.LogicalExpressionType.And:
                    if (!Left.IsTruthy)
                        return Left;
                    break;
            }
            Instance Right = await InterpretExpressionAsync(LogicalExpression.Right);
            switch (LogicalExpression.LogicType) {
                case LogicalExpression.LogicalExpressionType.And:
                    return Right;
                case LogicalExpression.LogicalExpressionType.Or:
                    if (Left.IsTruthy)
                        return Left;
                    else
                        return Right;
                case LogicalExpression.LogicalExpressionType.Xor:
                    if (Left.IsTruthy && !Right.IsTruthy)
                        return Left;
                    else if (!Left.IsTruthy && Right.IsTruthy)
                        return Right;
                    else
                        return Interpreter.False;
                default:
                    throw new InternalErrorException($"{LogicalExpression.Location}: Unhandled logical expression type: '{LogicalExpression.LogicType}'");
            }
        }
        async Task<Instance> InterpretNotExpression(NotExpression NotExpression) {
            Instance Right = await InterpretExpressionAsync(NotExpression.Right);
            return Right.IsTruthy ? Interpreter.False : Interpreter.True;
        }
        async Task<Instance> InterpretDefineMethodStatement(DefineMethodStatement DefineMethodStatement) {
            Instance MethodNameObject = await InterpretExpressionAsync(DefineMethodStatement.MethodName, ReturnType.HypotheticalVariable);
            if (MethodNameObject is VariableReference MethodNameRef) {
                string MethodName = MethodNameRef.Token.Value!;
                // Define static method
                if (MethodNameRef.Block != null) {
                    Module MethodModule = (Module)MethodNameRef.Block;
                    // Prevent redefining unsafe API methods
                    if (!AllowUnsafeApi && MethodModule.Methods.TryGetValue(MethodName, out Method? ExistingMethod) && ExistingMethod.Unsafe) {
                        throw new RuntimeException($"{DefineMethodStatement.Location}: The static method '{MethodName}' cannot be redefined since 'AllowUnsafeApi' is disabled for this script.");
                    }
                    // Create or overwrite static method
                    lock (MethodModule.Methods)
                        MethodModule.Methods[MethodName] = DefineMethodStatement.MethodExpression.ToMethod(CurrentAccessModifier, CurrentModule);
                }
                // Define instance method
                else {
                    Instance MethodInstance = MethodNameRef.Instance ?? CurrentInstance;
                    // Prevent redefining unsafe API methods
                    if (!AllowUnsafeApi && MethodInstance.InstanceMethods.TryGetValue(MethodName, out Method? ExistingMethod) && ExistingMethod.Unsafe) {
                        throw new RuntimeException($"{DefineMethodStatement.Location}: The instance method '{MethodName}' cannot be redefined since 'AllowUnsafeApi' is disabled for this script.");
                    }
                    // Create or overwrite instance method
                    MethodInstance.AddOrUpdateInstanceMethod(MethodName, DefineMethodStatement.MethodExpression.ToMethod(CurrentAccessModifier, CurrentModule));
                }
            }
            else {
                throw new InternalErrorException($"{DefineMethodStatement.Location}: Invalid method name: {MethodNameObject}");
            }
            return Interpreter.Nil;
        }
        async Task<Instance> InterpretDefineClassStatement(DefineClassStatement DefineClassStatement) {
            Instance ClassNameObject = await InterpretExpressionAsync(DefineClassStatement.ClassName, ReturnType.HypotheticalVariable);
            if (ClassNameObject is VariableReference ClassNameRef) {
                string ClassName = ClassNameRef.Token.Value!;
                Module? InheritsFrom = DefineClassStatement.InheritsFrom != null ? (await InterpretExpressionAsync(DefineClassStatement.InheritsFrom)).Module : null;

                // Create or patch class
                Module NewModule;
                // Patch class
                if (CurrentModule.Constants.TryGetValue(ClassName, out Instance? ConstantValue) && ConstantValue is ModuleReference ModuleReference) {
                    if (InheritsFrom != null) {
                        throw new SyntaxErrorException($"{DefineClassStatement.Location}: Patch for already defined class/module cannot inherit");
                    }
                    NewModule = ModuleReference.Module!;
                }
                // Create class
                else {
                    if (DefineClassStatement.IsModule) {
                        if (ClassNameRef.Module != null) {
                            NewModule = CreateModule(ClassName, ClassNameRef.Module, InheritsFrom);
                        }
                        else {
                            NewModule = CreateModule(ClassName, null, InheritsFrom);
                        }
                    }
                    else {
                        if (ClassNameRef.Module != null) {
                            NewModule = CreateClass(ClassName, ClassNameRef.Module, InheritsFrom);
                        }
                        else {
                            NewModule = CreateClass(ClassName, null, InheritsFrom);
                        }
                    }
                }

                // Interpret class statements
                AccessModifier PreviousAccessModifier = CurrentAccessModifier;
                CurrentAccessModifier = AccessModifier.Public;
                await CreateTemporaryClassScope(NewModule, async () => {
                    await CreateTemporaryInstanceScope(new ModuleReference(NewModule), async () => {
                        await InternalInterpretAsync(DefineClassStatement.BlockStatements, CurrentOnYield);
                    });
                });
                CurrentAccessModifier = PreviousAccessModifier;

                // Store class/module constant
                Module Module;
                // Path
                if (ClassNameRef.Block != null)
                    Module = (Module)ClassNameRef.Block;
                // Local
                else
                    Module = (ClassNameRef.Instance ?? CurrentInstance).Module!;
                // Store constant
                lock (Module.Constants)
                    Module.Constants[ClassName] = new ModuleReference(NewModule);
            }
            else {
                throw new InternalErrorException($"{DefineClassStatement.Location}: Invalid class/module name: {ClassNameObject}");
            }
            return Interpreter.Nil;
        }
        async Task<Instance> InterpretYieldStatement(YieldStatement YieldStatement) {
            if (CurrentOnYield != null) {
                List<Instance> YieldArgs = YieldStatement.YieldValues != null
                    ? await InterpretExpressionsAsync(YieldStatement.YieldValues)
                    : new();
                await CurrentOnYield.Call(this, null, YieldArgs, BreakHandleType: BreakHandleType.Destroy, CatchReturn: false);
            }
            else {
                throw new RuntimeException($"{YieldStatement.Location}: No block given to yield to");
            }
            return Interpreter.Nil;
        }
        async Task<Instance> InterpretSuperStatement(SuperStatement SuperStatement) {
            Module CurrentModule = this.CurrentModule;
            string? SuperMethodName = null;
            try {
                SuperMethodName = CurrentMethodScope.Method?.Name;
                if (CurrentModule != Interpreter.RootModule && CurrentModule.SuperModule is Module SuperModule) {
                    if (SuperMethodName != null && SuperModule.InstanceMethods.TryGetValue(SuperMethodName, out Method? SuperMethod)) {
                        Instances? Arguments = null;
                        if (SuperStatement.Arguments != null) {
                            Arguments = await InterpretExpressionsAsync(SuperStatement.Arguments);
                        }
                        return await SuperMethod.Call(this, null, Arguments);
                    }
                }
            }
            catch { }
            throw new RuntimeException($"{SuperStatement.Location}: No super method '{SuperMethodName}' to call");
        }
        async Task<Instance> InterpretAliasStatement(AliasStatement AliasStatement) {
            Instance MethodToAlias = await InterpretExpressionAsync(AliasStatement.MethodToAlias, ReturnType.FoundVariable);
            if (MethodToAlias is VariableReference MethodToAliasRef) {
                // Get methods dictionary
                ReactiveDictionary<string, Method> Methods;
                if (MethodToAliasRef.Instance != null) {
                    Methods = MethodToAliasRef.Instance!.InstanceMethods;
                }
                else if (MethodToAliasRef.Block != null) {
                    Methods = ((Module)MethodToAliasRef.Block!).Methods;
                }
                else {
                    Methods = CurrentInstance.InstanceMethods;
                }
                // Add alias for method
                Methods[AliasStatement.AliasAs.Token.Value!] = Methods[MethodToAliasRef.Token.Value!];
            }
            else {
                throw new SyntaxErrorException($"{AliasStatement.Location}: Expected method to alias, got '{MethodToAlias.Inspect()}'");
            }
            return Interpreter.Nil;
        }
        async Task<Instance> InterpretRangeExpression(RangeExpression RangeExpression) {
            Instance? RawMin = null;
            if (RangeExpression.Min != null) RawMin = await InterpretExpressionAsync(RangeExpression.Min);
            Instance? RawMax = null;
            if (RangeExpression.Max != null) RawMax = await InterpretExpressionAsync(RangeExpression.Max);

            if (RawMin is IntegerInstance Min && RawMax is IntegerInstance Max) {
                return new RangeInstance(Interpreter.Range, Min, Max, RangeExpression.IncludesMax);
            }
            else if (RawMin == null && RawMax is IntegerInstance MaxOnly) {
                return new RangeInstance(Interpreter.Range, null, MaxOnly, RangeExpression.IncludesMax);
            }
            else if (RawMax == null && RawMin is IntegerInstance MinOnly) {
                return new RangeInstance(Interpreter.Range, MinOnly, null, RangeExpression.IncludesMax);
            }
            else {
                throw new RuntimeException($"{RangeExpression.Location}: Range bounds must be integers (got '{RawMin?.LightInspect()}' and '{RawMax?.LightInspect()}')");
            }
        }
        async Task<Instance> InterpretIfBranchesStatement(IfBranchesStatement IfStatement) {
            for (int i = 0; i < IfStatement.Branches.Count; i++) {
                IfExpression Branch = IfStatement.Branches[i];
                // If / elsif
                if (Branch.Condition != null) {
                    Instance ConditionResult = await InterpretExpressionAsync(Branch.Condition);
                    if (ConditionResult.IsTruthy != Branch.Inverse) {
                        // Run statements
                        return await CreateTemporaryScope(async () =>
                            await InternalInterpretAsync(Branch.Statements, CurrentOnYield)
                        );
                    }
                }
                // Else
                else {
                    // Run statements
                    return await CreateTemporaryScope(async () =>
                        await InternalInterpretAsync(Branch.Statements, CurrentOnYield)
                    );
                }
            }
            return Interpreter.Nil;
        }
        async Task<Instance> InterpretBeginBranchesStatement(BeginBranchesStatement BeginBranchesStatement) {
            // Begin
            BeginStatement BeginBranch = (BeginStatement)BeginBranchesStatement.Branches[0];
            Exception? ExceptionToRescue = null;
            try {
                await CreateTemporaryScope(async () => {
                    // Create scope
                    CurrentObject.Push(new Scope());
                    // Run statements
                    await InternalInterpretAsync(BeginBranch.Statements, CurrentOnYield);
                });
            }
            catch (Exception Ex) when (Ex is not NonErrorException) {
                ExceptionToRescue = Ex;
            }

            // Rescue
            bool Rescued = false;
            if (ExceptionToRescue != null) {
                // Find a rescue statement that can rescue the given error
                for (int i = 1; i < BeginBranchesStatement.Branches.Count; i++) {
                    BeginComponentStatement Branch = BeginBranchesStatement.Branches[i];
                    if (Branch is RescueStatement RescueStatement) {
                        // Get or create the exception to rescue
                        ExceptionsTable.TryGetValue(ExceptionToRescue, out ExceptionInstance? ExceptionInstance);
                        ExceptionInstance ??= new(Interpreter.RuntimeError, ExceptionToRescue.Message);
                        // Get the rescuing exception type
                        Module RescuingExceptionModule = RescueStatement.Exception != null
                            ? (await InterpretExpressionAsync(RescueStatement.Exception)).Module!
                            : Interpreter.StandardError;

                        // Check whether rescue applies to this exception
                        bool CanRescue = false;
                        if (ExceptionInstance.Module!.InheritsFrom(RescuingExceptionModule)) {
                            CanRescue = true;
                        }

                        // Run the statements in the rescue block
                        if (CanRescue) {
                            Rescued = true;
                            await CreateTemporaryScope(async () => {
                                // Set exception variable to exception instance
                                if (RescueStatement.ExceptionVariable != null) {
                                    CurrentScope.LocalVariables.Add(RescueStatement.ExceptionVariable.Value!, ExceptionInstance);
                                }
                                await InternalInterpretAsync(RescueStatement.Statements, CurrentOnYield);
                            });
                            break;
                        }
                    }
                }
                // Rethrow exception if not rescued
                if (!Rescued) throw ExceptionToRescue;
            }

            // Ensure & Else
            for (int i = 1; i < BeginBranchesStatement.Branches.Count; i++) {
                BeginComponentStatement Branch = BeginBranchesStatement.Branches[i];
                if (Branch is EnsureStatement || (Branch is RescueElseStatement && !Rescued)) {
                    // Run statements
                    await CreateTemporaryScope(async () => {
                        await InternalInterpretAsync(Branch.Statements, CurrentOnYield);
                    });
                }
            }

            return Interpreter.Nil;
        }
        async Task<Instance> InterpretAssignmentExpression(AssignmentExpression AssignmentExpression, ReturnType ReturnType) {
            async Task AssignToVariable(VariableReference Variable, Instance Value) {
                switch (Variable.Token.Type) {
                    case Phase2TokenType.LocalVariableOrMethod:
                        // call instance.variable=
                        if (Variable.Instance != null) {
                            await Variable.Instance.TryCallInstanceMethod(this, Variable.Token.Value! + "=", Value);
                        }
                        // set variable =
                        else {
                            // Find appropriate local variable block
                            Block SetBlock = CurrentBlock;
                            foreach (object Object in CurrentObject)
                                if (Object is Block Block) {
                                    if (Block.LocalVariables.ContainsKey(Variable.Token.Value!)) {
                                        SetBlock = Block;
                                        break;
                                    }
                                }
                                else break;
                            // Set local variable
                            lock (SetBlock.LocalVariables)
                                SetBlock.LocalVariables[Variable.Token.Value!] = Value;
                        }
                        break;
                    case Phase2TokenType.GlobalVariable:
                        lock (Interpreter.GlobalVariables)
                            Interpreter.GlobalVariables[Variable.Token.Value!] = Value;
                        break;
                    case Phase2TokenType.ConstantOrMethod:
                        if (CurrentBlock.Constants.ContainsKey(Variable.Token.Value!))
                            await Warn($"{Variable.Token.Location}: Already initialized constant '{Variable.Token.Value!}'");
                        lock (CurrentBlock.Constants)
                            CurrentBlock.Constants[Variable.Token.Value!] = Value;
                        break;
                    case Phase2TokenType.InstanceVariable:
                        lock (CurrentInstance.InstanceVariables)
                            CurrentInstance.InstanceVariables[Variable.Token.Value!] = Value;
                        break;
                    case Phase2TokenType.ClassVariable:
                        lock (CurrentModule.ClassVariables)
                            CurrentModule.ClassVariables[Variable.Token.Value!] = Value;
                        break;
                    default:
                        throw new InternalErrorException($"{Variable.Token.Location}: Assignment variable token is not a variable type (got {Variable.Token.Type})");
                }
            }

            Instance Right = await InterpretExpressionAsync(AssignmentExpression.Right);

            Instance Left = await InterpretExpressionAsync(AssignmentExpression.Left, ReturnType.HypotheticalVariable);
            if (Left is VariableReference LeftVariable) {
                if (Right is Instance RightInstance) {
                    // LeftVariable = RightInstance
                    await AssignToVariable(LeftVariable, RightInstance);

                    // Return left variable reference or value
                    if (ReturnType == ReturnType.InterpretResult) {
                        return RightInstance;
                    }
                    else {
                        return Left;
                    }
                }
                else {
                    throw new InternalErrorException($"{LeftVariable.Token.Location}: Assignment value should be an instance, but got {Right.GetType().Name}");
                }
            }
            else {
                throw new RuntimeException($"{AssignmentExpression.Left.Location}: {Left.GetType()} cannot be the target of an assignment");
            }
        }
        async Task<Instance> InterpretUndefineMethodStatement(UndefineMethodStatement UndefineMethodStatement) {
            string MethodName = UndefineMethodStatement.MethodName.Token.Value!;
            if (MethodName == "initialize") {
                await Warn($"{UndefineMethodStatement.MethodName.Token.Location}: undefining 'initialize' may cause serious problems");
            }
            if (!CurrentModule.InstanceMethods.Remove(MethodName)) {
                throw new RuntimeException($"{UndefineMethodStatement.MethodName.Token.Location}: Undefined method '{MethodName}' for {CurrentModule.Name}");
            }
            return Interpreter.Nil;
        }
        async Task<Instance> InterpretDefinedExpression(DefinedExpression DefinedExpression) {
            if (DefinedExpression.Expression is MethodCallExpression DefinedMethod) {
                try {
                    await InterpretExpressionAsync(DefinedMethod.MethodPath, ReturnType.FoundVariable);
                }
                catch (RuntimeException) {
                    return Interpreter.Nil;
                }
                return new StringInstance(Interpreter.String, "method");
            }
            if (DefinedExpression.Expression is PathExpression DefinedPath) {
                try {
                    await InterpretExpressionAsync(DefinedPath, ReturnType.FoundVariable);
                }
                catch (RuntimeException) {
                    return Interpreter.Nil;
                }
                return new StringInstance(Interpreter.String, "method");
            }
            else if (DefinedExpression.Expression is ObjectTokenExpression ObjectToken) {
                if (ObjectToken.Token.Type == Phase2TokenType.LocalVariableOrMethod) {
                    if (TryGetLocalVariable(ObjectToken.Token.Value!, out _)) {
                        return new StringInstance(Interpreter.String, "local-variable");
                    }
                    else if (TryGetLocalInstanceMethod(ObjectToken.Token.Value!, out _)) {
                        return new StringInstance(Interpreter.String, "method");
                    }
                    else {
                        return Interpreter.Nil;
                    }
                }
                else if (ObjectToken.Token.Type == Phase2TokenType.GlobalVariable) {
                    if (Interpreter.GlobalVariables.ContainsKey(ObjectToken.Token.Value!)) {
                        return new StringInstance(Interpreter.String, "global-variable");
                    }
                    else {
                        return Interpreter.Nil;
                    }
                }
                else if (ObjectToken.Token.Type == Phase2TokenType.ConstantOrMethod) {
                    if (TryGetLocalConstant(ObjectToken.Token.Value!, out _)) {
                        return new StringInstance(Interpreter.String, "constant");
                    }
                    else if (TryGetLocalInstanceMethod(ObjectToken.Token.Value!, out _)) {
                        return new StringInstance(Interpreter.String, "method");
                    }
                    else {
                        return Interpreter.Nil;
                    }
                }
                else if (ObjectToken.Token.Type == Phase2TokenType.InstanceVariable) {
                    if (CurrentInstance.InstanceVariables.ContainsKey(ObjectToken.Token.Value!)) {
                        return new StringInstance(Interpreter.String, "instance-variable");
                    }
                    else {
                        return Interpreter.Nil;
                    }
                }
                else if (ObjectToken.Token.Type == Phase2TokenType.ClassVariable) {
                    if (CurrentModule.ClassVariables.ContainsKey(ObjectToken.Token.Value!)) {
                        return new StringInstance(Interpreter.String, "class-variable");
                    }
                    else {
                        return Interpreter.Nil;
                    }
                }
                else {
                    return new StringInstance(Interpreter.String, "expression");
                }
            }
            else if (DefinedExpression.Expression is SelfExpression) {
                return new StringInstance(Interpreter.String, "self");
            }
            else if (DefinedExpression.Expression is SuperStatement) {
                return new StringInstance(Interpreter.String, "super");
            }
            else {
                throw new InternalErrorException($"{DefinedExpression.Location}: Unknown expression type for defined?: {DefinedExpression.Expression.GetType().Name}");
            }
        }
        async Task<Instance> InterpretHashArgumentsExpression(HashArgumentsExpression HashArgumentsExpression) {
            return new HashArgumentsInstance(
                await InterpretHashExpression(HashArgumentsExpression.HashExpression),
                Interpreter
            );
        }
        async Task<Instance> InterpretEnvironmentInfoExpression(EnvironmentInfoExpression EnvironmentInfoExpression) {
            if (EnvironmentInfoExpression.Type == EnvironmentInfoType.__LINE__) {
                return new IntegerInstance(Interpreter.Integer, ApproximateLocation.Line);
            }
            else {
                throw new InternalErrorException($"{ApproximateLocation}: Environment info type not handled: '{EnvironmentInfoExpression.Type}'");
            }
        }

        public enum AccessModifier {
            Public,
            Private,
            Protected,
        }
        public enum BreakHandleType {
            Invalid,
            Rethrow,
            Destroy
        }
        public enum ReturnType {
            InterpretResult,
            FoundVariable,
            HypotheticalVariable
        }
        async Task<Instance> InterpretExpressionAsync(Expression Expression, ReturnType ReturnType = ReturnType.InterpretResult) {
            // Set approximate location
            ApproximateLocation = Expression.Location;

            // Stop script
            if (Stopping)
                throw new StopException();

            // Interpret expression
            return Expression switch {
                MethodCallExpression MethodCallExpression => await InterpretMethodCallExpression(MethodCallExpression),
                ObjectTokenExpression ObjectTokenExpression => await InterpretObjectTokenExpression(ObjectTokenExpression, ReturnType),
                IfExpression IfExpression => await InterpretIfExpression(IfExpression),
                WhileExpression WhileExpression => await InterpretWhileExpression(WhileExpression),
                RescueExpression RescueExpression => await InterpretRescueExpression(RescueExpression),
                TernaryExpression TernaryExpression => await InterpretTernaryExpression(TernaryExpression),
                CaseExpression CaseExpression => await InterpretCaseExpression(CaseExpression),
                ArrayExpression ArrayExpression => await InterpretArrayExpression(ArrayExpression),
                HashExpression HashExpression => await InterpretHashExpression(HashExpression),
                WhileStatement WhileStatement => await InterpretWhileStatement(WhileStatement),
                ForStatement ForStatement => await InterpretForStatement(ForStatement),
                SelfExpression => CurrentInstance,
                LogicalExpression LogicalExpression => await InterpretLogicalExpression(LogicalExpression),
                NotExpression NotExpression => await InterpretNotExpression(NotExpression),
                DefineMethodStatement DefineMethodStatement => await InterpretDefineMethodStatement(DefineMethodStatement),
                DefineClassStatement DefineClassStatement => await InterpretDefineClassStatement(DefineClassStatement),
                ReturnStatement ReturnStatement => throw new ReturnException(
                                                        ReturnStatement.ReturnValue != null
                                                        ? await InterpretExpressionAsync(ReturnStatement.ReturnValue)
                                                        : Interpreter.Nil),
                LoopControlStatement LoopControlStatement => LoopControlStatement.Type switch {
                    LoopControlType.Break => throw new BreakException(),
                    LoopControlType.Retry => throw new RetryException(),
                    LoopControlType.Redo => throw new RedoException(),
                    LoopControlType.Next => throw new NextException(),
                    _ => throw new InternalErrorException($"{Expression.Location}: Loop control type not handled: '{LoopControlStatement.Type}'")},
                YieldStatement YieldStatement => await InterpretYieldStatement(YieldStatement),
                SuperStatement SuperStatement => await InterpretSuperStatement(SuperStatement),
                AliasStatement AliasStatement => await InterpretAliasStatement(AliasStatement),
                RangeExpression RangeExpression => await InterpretRangeExpression(RangeExpression),
                IfBranchesStatement IfBranchesStatement => await InterpretIfBranchesStatement(IfBranchesStatement),
                BeginBranchesStatement BeginBranchesStatement => await InterpretBeginBranchesStatement(BeginBranchesStatement),
                AssignmentExpression AssignmentExpression => await InterpretAssignmentExpression(AssignmentExpression, ReturnType),
                UndefineMethodStatement UndefineMethodStatement => await InterpretUndefineMethodStatement(UndefineMethodStatement),
                DefinedExpression DefinedExpression => await InterpretDefinedExpression(DefinedExpression),
                HashArgumentsExpression HashArgumentsExpression => await InterpretHashArgumentsExpression(HashArgumentsExpression),
                EnvironmentInfoExpression EnvironmentInfoExpression => await InterpretEnvironmentInfoExpression(EnvironmentInfoExpression),
                _ => throw new InternalErrorException($"{Expression.Location}: Not sure how to interpret expression {Expression.GetType().Name} ({Expression.Inspect()})"),
            };
        }
        async Task<List<Instance>> InterpretExpressionsAsync(List<Expression> Expressions) {
            List<Instance> Results = new();
            foreach (Expression Expression in Expressions) {
                Results.Add(await InterpretExpressionAsync(Expression));
            }
            return Results;
        }
        internal async Task<Instance> InternalInterpretAsync(List<Expression> Statements, Method? OnYield = null) {
            try {
                // Set on yield
                CurrentOnYield = OnYield;
                // Interpret statements
                Instance LastInstance = Interpreter.Nil;
                for (int Index = 0; Index < Statements.Count; Index++) {
                    // Interpret expression and store the result
                    Expression Statement = Statements[Index];
                    LastInstance = await InterpretExpressionAsync(Statement);
                }
                // Return last expression
                return LastInstance;
            }
            finally {
                // Reset on yield
                CurrentOnYield = null;
            }
        }
        internal async Task<Instance> InternalEvaluateAsync(string Code) {
            // Get statements from code
            List<Phase1.Phase1Token> Tokens = Phase1.GetPhase1Tokens(Code);
            List<Expression> Statements = ObjectsToExpressions(Tokens, ExpressionsType.Statements);

            // Interpret statements
            return await InternalInterpretAsync(Statements);
        }

        public async Task<Instance> InterpretAsync(List<Expression> Statements, Method? OnYield = null) {
            // Debounce
            if (Running) throw new ApiException("The script is already running.");
            Running = true;

            // Interpret statements and store the result
            Instance LastInstance;
            try {
                LastInstance = await InternalInterpretAsync(Statements, OnYield);
            }
            catch (LoopControlException Ex) {
                throw new SyntaxErrorException($"{ApproximateLocation}: Invalid {Ex.GetType().Name} (must be in a loop)");
            }
            catch (ReturnException Ex) {
                return Ex.Instance;
            }
            catch (ExitException) {
                return Interpreter.Nil;
            }
            catch (StopException) {
                return Interpreter.Nil;
            }
            finally {
                // Deactivate debounce
                Running = false;
            }
            return LastInstance;
        }
        public Instance Interpret(List<Expression> Statements) {
            return InterpretAsync(Statements).Result;
        }
        public async Task<Instance> EvaluateAsync(string Code) {
            List<Phase1.Phase1Token> Tokens = Phase1.GetPhase1Tokens(Code);
            List<Expression> Statements = ObjectsToExpressions(Tokens, ExpressionsType.Statements);

            /*Console.WriteLine(Statements.Inspect());
            Console.Write("Press enter to continue.");
            Console.ReadLine();*/

            return await InterpretAsync(Statements);
        }
        public Instance Evaluate(string Code) {
            return EvaluateAsync(Code).Result;
        }
        public async Task WaitForThreadsAsync() {
            while (ThreadCount > 0)
                await Task.Delay(10);
        }
        public void WaitForThreads() {
            WaitForThreadsAsync().Wait();
        }
        /// <summary>Stops the script, including all running threads.</summary>
        public void Stop() {
            Stopping = true;
        }

        public Script(Interpreter interpreter, bool AllowUnsafeApi = true) {
            Interpreter = interpreter;
            this.AllowUnsafeApi = AllowUnsafeApi;

            CurrentObject.Push(interpreter.RootModule);
            CurrentObject.Push(interpreter.RootInstance);
            CurrentObject.Push(interpreter.RootScope);
        }
    }
}
