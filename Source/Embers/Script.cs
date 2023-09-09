using static Embers.Phase2;

#pragma warning disable CS1998

namespace Embers
{
    public class Script
    {
        public readonly Interpreter Interpreter;
        public readonly bool AllowUnsafeApi;
        public bool Running { get; private set; }
        public DebugLocation ApproximateLocation { get; private set; } = DebugLocation.Unknown;

        readonly Stack<object> CurrentObject = new();
        Block CurrentBlock => (Block)CurrentObject.First(obj => obj is Block);
        Scope CurrentScope => (Scope)CurrentObject.First(obj => obj is Scope);
        Module CurrentModule => (Module)CurrentObject.First(obj => obj is Module);
        Instance CurrentInstance => (Instance)CurrentObject.First(obj => obj is Instance);

        public Instance CreateInstanceWithNew(Class Class) {
            if (Class == Interpreter.NilClass)
                return new NilInstance(Interpreter.NilClass);
            else if (Class == Interpreter.TrueClass)
                return new TrueInstance(Interpreter.TrueClass);
            else if (Class == Interpreter.FalseClass)
                return new FalseInstance(Interpreter.FalseClass);
            else if (Class == Interpreter.String)
                return new StringInstance(Interpreter.String, "");
            else if (Class == Interpreter.Symbol)
                return new SymbolInstance(Interpreter.Symbol, "");
            else if (Class == Interpreter.Integer)
                return new IntegerInstance(Interpreter.Integer, 0);
            else if (Class == Interpreter.Float)
                return new FloatInstance(Interpreter.Float, 0);
            else if (Class == Interpreter.Proc)
                throw new RuntimeException($"{ApproximateLocation}: Tried to create Proc object without a block");
            else if (Class == Interpreter.Array)
                return new ArrayInstance(Interpreter.Array, new List<Instance>());
            else if (Class == Interpreter.Hash)
                return new HashInstance(Interpreter.Hash, new Dictionary<Instance, Instance>(), Interpreter.Nil);
            else
                return new Instance(Class);
        }

        public class Block {
            public readonly object? Parent;
            public readonly Dictionary<string, Instance> LocalVariables = new();
            public readonly Dictionary<string, Instance> Constants = new();
            public Block(object? parent) {
                Parent = parent;
            }
        }
        public class Scope : Block {
            public Scope(object? parent) : base(parent) { }
        }
        public class Module : Block {
            public readonly string Name;
            public readonly Dictionary<string, Method> Methods = new();
            public readonly Dictionary<string, Method> InstanceMethods = new();
            public readonly Dictionary<string, Instance> ClassVariables = new();
            public readonly Interpreter Interpreter;
            public Module(string name, Module parent) : base(parent) {
                Name = name;
                Interpreter = parent.Interpreter;
                Setup();
            }
            public Module(string name, Interpreter interpreter) : base(null) {
                Name = name;
                Interpreter = interpreter;
                Setup();
            }
            void Setup() {
                // Default class and instance methods
                Api.DefaultClassAndInstanceMethods.CopyTo(InstanceMethods);
                Api.DefaultClassAndInstanceMethods.CopyTo(Methods);
                Api.DefaultInstanceMethods.CopyTo(InstanceMethods);
            }
        }
        public class Class : Module {
            public Class(string name, Module parent) : base(name, parent) {
                Setup();
            }
            public Class(string name, Interpreter interpreter) : base(name, interpreter) {
                Setup();
            }
            void Setup() {
                // Default method: new
                Methods.Add("new", new Method(async Input => {
                    Instance NewInstance = Input.Script.CreateInstanceWithNew(this);
                    if (NewInstance.InstanceMethods.TryGetValue("initialize", out Method? Initialize)) {
                        // Set instance
                        Input.Script.CurrentObject.Push(NewInstance);
                        // Call initialize & ignore result
                        await Initialize.Call(Input.Script, NewInstance, Input.Arguments);
                        // Step back an instance
                        Input.Script.CurrentObject.Pop();
                        // Return instance
                        return NewInstance;
                    }
                    else {
                        throw new RuntimeException($"Undefined method 'initialize' for {Name}");
                    }
                }, null));
                // Default method: initialize
                InstanceMethods.Add("initialize", new Method(async Input => {
                    return Input.Instance;
                }, 0));
            }
        }
        public class Instance {
            /*public bool IsA<T>() {
                return GetType() == typeof(T);
            }*/
            public readonly Module? Module; // Will be null if instance is a pseudoinstance
            public readonly long ObjectId;
            public virtual Dictionary<string, Instance> InstanceVariables { get; } = new();
            public virtual Dictionary<string, Method> InstanceMethods { get; } = new();
            public bool IsTruthy => !(Object == null || false.Equals(Object));
            public virtual object? Object { get { return null; } }
            public virtual bool Boolean { get { throw new RuntimeException("Instance is not a boolean"); } }
            public virtual string String { get { throw new RuntimeException("Instance is not a string"); } }
            public virtual long Integer { get { throw new RuntimeException("Instance is not an integer"); } }
            public virtual double Float { get { throw new RuntimeException("Instance is not a float"); } }
            public virtual Method Proc { get { throw new RuntimeException("Instance is not a proc"); } }
            public virtual List<Instance> Array { get { throw new RuntimeException("Instance is not an array"); } }
            public virtual Dictionary<Instance, Instance> Hash { get { throw new RuntimeException("Instance is not a hash"); } }
            public virtual Module ModuleRef { get { throw new ApiException("Instance is not a class/module reference"); } }
            public virtual Method MethodRef { get { throw new ApiException("Instance is not a method reference"); } }
            public virtual string Inspect() {
                return $"Instance of {Module?.Name}";
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

                                string Formatted = (await Script.InternalEvaluateAsync(ToFormat))[0].LightInspect();
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
                // Copy instance methods
                if (this is not PseudoInstance) {
                    fromModule.InstanceMethods.CopyTo(InstanceMethods);
                }
            }
            public Instance(Interpreter interpreter) {
                Module = null;
                ObjectId = interpreter.GenerateObjectId;
            }
            public void AddOrUpdateInstanceMethod(string Name, Method Method) {
                lock (InstanceMethods) lock (Module!.InstanceMethods)
                    InstanceMethods[Name] =
                    Module.InstanceMethods[Name] = Method;
            }
            public async Task<Instances> TryCallInstanceMethod(Script Script, string MethodName, Instances? Arguments = null) {
                // Found
                if (InstanceMethods.TryGetValue(MethodName, out Method? FindMethod)) {
                    return await Script.CreateTemporaryClassScope(Module!, async () =>
                        await FindMethod.Call(Script, this, Arguments)
                    );
                }
                // Error
                else {
                    throw new RuntimeException($"{DebugLocation.Unknown}: Undefined method '{MethodName}' for {Module?.Name}");
                }
            }
        }
        public class NilInstance : Instance {
            public override string Inspect() {
                return "nil";
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
                IsStringSymbol = Value.Any("(){}[]<>=+-*/%!?.,;@#&|~^$_".Contains) || Value.Any(char.IsWhiteSpace) || (Value.Length != 0 && char.IsAsciiDigit(Value[0]));
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
                string FloatString = Value.ToString();
                if (!FloatString.Contains('.')) {
                    FloatString += ".0";
                }
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
            public override string Inspect() {
                return "ProcInstance";
            }
            public ProcInstance(Class fromClass, Method value) : base(fromClass) {
                Value = value;
            }
            public void SetValue(Method value) {
                Value = value;
            }
        }
        public class ArrayInstance : Instance {
            List<Instance> Value;
            public override object? Object { get { return Value; } }
            public override List<Instance> Array { get { return Value; } }
            public override string Inspect() {
                return $"[{Value.InspectInstances()}]";
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
        public abstract class PseudoInstance : Instance {
            public override Dictionary<string, Instance> InstanceVariables { get { throw new ApiException($"{GetType().Name} instance does not have instance variables"); } }
            public override Dictionary<string, Method> InstanceMethods { get { throw new ApiException($"{GetType().Name} instance does not have instance methods"); } }
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
        public class ModuleReference : PseudoInstance {
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
            Func<MethodInput, Task<Instances>> Function;
            public readonly IntRange ArgumentCountRange;
            public readonly List<MethodArgumentExpression> ArgumentNames;
            public readonly bool Unsafe;
            public Method(Func<MethodInput, Task<Instances>> function, IntRange? argumentCountRange, List<MethodArgumentExpression>? argumentNames = null, bool isUnsafe = false) {
                Function = function;
                ArgumentCountRange = argumentCountRange ?? new IntRange();
                ArgumentNames = argumentNames ?? new();
                Unsafe = isUnsafe;
            }
            public Method(Func<MethodInput, Task<Instances>> function, Range argumentCountRange, List<MethodArgumentExpression>? argumentNames = null, bool isUnsafe = false) {
                Function = function;
                ArgumentCountRange = new IntRange(argumentCountRange);
                ArgumentNames = argumentNames ?? new();
                Unsafe = isUnsafe;
            }
            public Method(Func<MethodInput, Task<Instances>> function, int argumentCount, List<MethodArgumentExpression>? argumentNames = null, bool isUnsafe = false) {
                Function = function;
                ArgumentCountRange = new IntRange(argumentCount, argumentCount);
                ArgumentNames = argumentNames ?? new();
                Unsafe = isUnsafe;
            }
            public async Task<Instances> Call(Script Script, Instance? OnInstance, Instances? Arguments = null, Method? OnYield = null, BreakHandleType BreakHandleType = BreakHandleType.Invalid) {
                if (Unsafe && !Script.AllowUnsafeApi)
                    throw new RuntimeException($"{Script.ApproximateLocation}: This method is unavailable since 'AllowUnsafeApi' is disabled for this script.");

                Arguments ??= new Instances();
                if (ArgumentCountRange.IsInRange(Arguments.Count)) {
                    // Create temporary scope
                    Script.CurrentObject.Push(new Scope(Script.CurrentBlock));
                    // Set argument variables
                    for (int i = 0; i < ArgumentNames.Count; i++) {
                        MethodArgumentExpression Argument = ArgumentNames[i];
                        if (Argument.SplatType == SplatType.Single) {
                            // Get splat arguments
                            int RemainingArgumentNames = ArgumentNames.Count - i - 1;
                            int RemainingGivenArguments = Arguments.Count - i;
                            int SplatArgumentCount = RemainingGivenArguments - RemainingArgumentNames;
                            List<MethodArgumentExpression> SplatArguments = ArgumentNames.GetRange(i, SplatArgumentCount);
                            i += SplatArgumentCount;
                            // Create array from splat arguments
                            throw new NotImplementedException($"Splat arguments not implemented yet, because arrays not implemented yet (but {SplatArguments.Count} arguments were splat)");
                            // Interpreter.CurrentScope.LocalVariables.Add(Argument.ArgumentName.Value!, SplatArguments);
                        }
                        else if (Argument.SplatType == SplatType.Double) {
                            throw new NotImplementedException("Double splat arguments not implemented yet");
                        }
                        else {
                            // Declare argument as variable in local scope
                            if (i < Arguments.Count) {
                                Script.CurrentScope.LocalVariables.Add(Argument.ArgumentName.Value!, Arguments[i]);
                            }
                            // Optional argument not given
                            else {
                                Instance DefaultValue = Argument.DefaultValue != null ? (await Script.InterpretExpressionAsync(Argument.DefaultValue)) : Script.Interpreter.Nil;
                                Script.CurrentScope.LocalVariables.Add(Argument.ArgumentName.Value!, DefaultValue);
                            }
                        }
                    }
                    // Call method
                    Instances ReturnValues;
                    try {
                        ReturnValues = await Function(new MethodInput(Script, OnInstance, Arguments, OnYield));
                    }
                    catch (BreakException) {
                        if (BreakHandleType == BreakHandleType.Rethrow) {
                            throw;
                        }
                        else if (BreakHandleType == BreakHandleType.Destroy) {
                            ReturnValues = Script.Interpreter.Nil;
                        }
                        else {
                            throw new SyntaxErrorException($"{Script.ApproximateLocation}: Invalid break (break must be in a loop)");
                        }
                    }
                    catch (ReturnException Ex) {
                        ReturnValues = Ex.Instances;
                    }
                    finally {
                        // Step back a scope
                        Script.CurrentObject.Pop();
                    }
                    // Return method return value
                    return ReturnValues;
                }
                else {
                    throw new RuntimeException($"{Script.ApproximateLocation}: Wrong number of arguments (given {Arguments.Count}, expected {ArgumentCountRange})");
                }
            }
            public void ChangeFunction(Func<MethodInput, Task<Instances>> function) {
                Function = function;
            }
        }
        public class MethodScope : Scope {
            public readonly Method? Method;
            public MethodScope(Block? parent, Method method) : base(parent) {
                Method = method;
            }
        }
        public class MethodInput {
            public Script Script;
            public Interpreter Interpreter;
            public Instance Instance;
            public Instances Arguments;
            public Method? OnYield;
            public MethodInput(Script script, Instance instance, Instances arguments, Method? onYield = null) {
                Script = script;
                Interpreter = script.Interpreter;
                Instance = instance;
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
            public static implicit operator Instances(Instance Instance) {
                return new Instances(Instance);
            }
            public static implicit operator Instances(List<Instance> InstanceList) {
                return new Instances(InstanceList);
            }
            public static implicit operator Instance(Instances Instances) {
                if (Instances.Count == 0)
                    throw new RuntimeException($"Cannot implicitly cast Instances to Instance because there are none.");
                if (Instances.Count != 1)
                    throw new RuntimeException($"Cannot implicitly cast Instances to Instance because {Instances.Count - 1} instances would be overlooked");
                return Instances[0];
            }
            public Instance this[int i] => InstanceList != null ? InstanceList[i] : (i == 0 && Instance != null ? Instance : throw new ApiException("Index was outside the range of the instances"));
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
                    return new List<Instance>() {Instance};
                }
                else {
                    return new List<Instance>();
                }
            } }
        }

        public async Task Warn(string Message) {
            await Interpreter.RootInstance.InstanceMethods["warn"].Call(this, new ModuleReference(Interpreter.RootModule), new StringInstance(Interpreter.String, Message));
        }
        public static Module CreateModule(Module Parent, string Name) {
            Module NewClass = new(Name, Parent);
            Parent.Constants[Name] = new ModuleReference(NewClass);
            return NewClass;
        }
        public Module CreateModule(string Name) {
            return CreateModule(Interpreter.RootModule, Name);
        }
        public static Class CreateClass(Module Parent, string Name) {
            Class NewClass = new(Name, Parent);
            Parent.Constants[Name] = new ModuleReference(NewClass);
            return NewClass;
        }
        public Class CreateClass(string Name) {
            return CreateClass(Interpreter.RootModule, Name);
        }
        T CreateTemporaryClassScope<T>(Module Module, Func<T> Do) {
            // Create temporary class/module scope
            CurrentObject.Push(Module);
            // Do action
            T Result = Do();
            // Step back a class/module
            CurrentObject.Pop();
            // Return result
            return Result;
        }
        T CreateTemporaryInstanceScope<T>(Instance Instance, Func<T> Do) {
            // Create temporary instance scope
            CurrentObject.Push(Instance);
            // Do action
            T Result = Do();
            // Step back an instance
            CurrentObject.Pop();
            // Return result
            return Result;
        }
        SymbolInstance GetSymbol(string Value) {
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
            IEnumerable<Block> CurrentBlockStack = CurrentObject.Where(obj => obj is Block).Cast<Block>();

            foreach (Block Block in CurrentBlockStack) {
                if (Block.LocalVariables.TryGetValue(Name, out Instance? FindLocalVariable)) {
                    LocalVariable = FindLocalVariable;
                    return true;
                }
            }
            LocalVariable = null;
            return false;
        }
        public bool TryGetLocalConstant(string Name, out Instance? LocalConstant) {
            IEnumerable<Block> CurrentBlockStack = CurrentObject.Where(obj => obj is Block).Cast<Block>();

            foreach (Block Block in CurrentBlockStack) {
                if (Block.Constants.TryGetValue(Name, out Instance? FindLocalConstant)) {
                    LocalConstant = FindLocalConstant;
                    return true;
                }
            }
            LocalConstant = null;
            return false;
        }
        public bool TryGetLocalInstanceMethod(string Name, out Method? LocalInstanceMethod) {
            IEnumerable<Instance> CurrentInstanceStack = CurrentObject.Where(obj => obj is Instance).Cast<Instance>();

            foreach (Instance Instance in CurrentInstanceStack) {
                if (Instance.InstanceMethods.TryGetValue(Name, out Method? FindLocalInstanceMethod)) {
                    LocalInstanceMethod = FindLocalInstanceMethod;
                    return true;
                }
            }
            LocalInstanceMethod = null;
            return false;
        }

        async Task<Instances> InterpretMethodCallExpression(MethodCallExpression MethodCallExpression) {
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
                        return await CreateTemporaryClassScope(MethodModule, async () =>
                            await StaticMethod!.Call(
                                this, MethodOwner, await InterpretExpressionsAsync(MethodCallExpression.Arguments), MethodCallExpression.OnYield?.Method
                            )
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
                            return await CreateTemporaryInstanceScope(CurrentInstance, async () =>
                                await LocalInstanceMethod!.Call(
                                    this, CurrentInstance, await InterpretExpressionsAsync(MethodCallExpression.Arguments), MethodCallExpression.OnYield?.Method
                                )
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
                            return await CreateTemporaryInstanceScope(MethodInstance, async () =>
                                await PathInstanceMethod!.Call(
                                    this, MethodInstance, await InterpretExpressionsAsync(MethodCallExpression.Arguments), MethodCallExpression.OnYield?.Method
                                )
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
        async Task<Instances> InterpretObjectTokenExpression(ObjectTokenExpression ObjectTokenExpression, ReturnType ReturnType) {
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
                                    throw new RuntimeException($"{ObjectTokenExpression.Token.Location}: Undefined local variable or method '{ObjectTokenExpression.Token.Value!}' for {CurrentScope}");
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
                            // Self
                            case Phase2TokenType.Self: {
                                return new ModuleReference(CurrentModule);
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
        async Task<Instances> InterpretIfExpression(IfExpression IfExpression) {
            if (IfExpression.Condition == null || (await InterpretExpressionAsync(IfExpression.Condition))[0].IsTruthy != IfExpression.Inverse) {
                return await InternalInterpretAsync(IfExpression.Statements);
            }
            return Interpreter.Nil;
        }
        async Task<Instances> InterpretWhileExpression(WhileExpression WhileExpression) {
            while ((await InterpretExpressionAsync(WhileExpression.Condition!))[0].IsTruthy != WhileExpression.Inverse) {
                try {
                    await InternalInterpretAsync(WhileExpression.Statements);
                }
                catch (BreakException) {
                    break;
                }
            }
            return Interpreter.Nil;
        }
        async Task<Instances> InterpretArrayExpression(ArrayExpression ArrayExpression) {
            List<Instance> Items = new();
            foreach (Expression Item in ArrayExpression.Expressions) {
                Items.Add(await InterpretExpressionAsync(Item));
            }
            return new ArrayInstance(Interpreter.Array, Items);
        }
        async Task<Instances> InterpretHashExpression(HashExpression HashExpression) {
            Dictionary<Instance, Instance> Items = new();
            foreach (KeyValuePair<Expression, Expression> Item in HashExpression.Expressions) {
                Items.Add(await InterpretExpressionAsync(Item.Key), await InterpretExpressionAsync(Item.Value));
            }
            return new HashInstance(Interpreter.Hash, Items, Interpreter.Nil);
        }
        async Task<Instances> InterpretWhileStatement(WhileStatement WhileStatement) {
            await InterpretExpressionAsync(WhileStatement.WhileExpression);
            return Interpreter.Nil;
        }
        async Task<Instances> InterpretLogicalExpression(LogicalExpression LogicalExpression) {
            Instance Left = (await InterpretExpressionAsync(LogicalExpression.Left)).SingleInstance;
            switch (LogicalExpression.LogicType) {
                case LogicalExpression.LogicalExpressionType.And:
                    if (!Left.IsTruthy)
                        return Left;
                    break;
            }
            Instance Right = (await InterpretExpressionAsync(LogicalExpression.Right)).SingleInstance;
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
        async Task<Instances> InterpretDefineMethodStatement(DefineMethodStatement DefineMethodStatement) {
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
                        MethodModule.Methods[MethodName] = DefineMethodStatement.MethodExpression.Method;
                }
                // Define instance method
                else {
                    Instance MethodInstance = MethodNameRef.Instance ?? CurrentInstance;
                    // Prevent redefining unsafe API methods
                    if (!AllowUnsafeApi && MethodInstance.InstanceMethods.TryGetValue(MethodName, out Method? ExistingMethod) && ExistingMethod.Unsafe) {
                        throw new RuntimeException($"{DefineMethodStatement.Location}: The instance method '{MethodName}' cannot be redefined since 'AllowUnsafeApi' is disabled for this script.");
                    }
                    // Create or overwrite instance method
                    MethodInstance.AddOrUpdateInstanceMethod(MethodName, DefineMethodStatement.MethodExpression.Method);
                }
            }
            else {
                throw new InternalErrorException($"{DefineMethodStatement.Location}: Invalid method name: {MethodNameObject}");
            }
            return Interpreter.Nil;
        }
        async Task<Instances> InterpretDefineClassStatement(DefineClassStatement DefineClassStatement) {
            Instance ClassNameObject = await InterpretExpressionAsync(DefineClassStatement.ClassName, ReturnType.HypotheticalVariable);
            if (ClassNameObject is VariableReference ClassNameRef) {
                string ClassName = ClassNameRef.Token.Value!;

                // Create or patch class
                Module NewModule;
                // Patch class
                if (CurrentModule.Constants.TryGetValue(ClassName, out Instance? ConstantValue) && ConstantValue is ModuleReference ModuleReference) {
                    NewModule = ModuleReference.Module!;
                }
                // Create class
                else {
                    if (DefineClassStatement.IsModule) {
                        if (ClassNameRef.Module != null) {
                            NewModule = new Module(ClassName, ClassNameRef.Module);
                        }
                        else {
                            NewModule = new Module(ClassName, Interpreter);
                        }
                    }
                    else {
                        if (ClassNameRef.Module != null) {
                            NewModule = new Class(ClassName, ClassNameRef.Module);
                        }
                        else {
                            NewModule = new Class(ClassName, Interpreter);
                        }
                    }
                }

                // Interpret class statements
                await CreateTemporaryClassScope(NewModule, async () => {
                    await CreateTemporaryInstanceScope(new Instance(NewModule), async () => {
                        await InternalInterpretAsync(DefineClassStatement.BlockStatements);
                    });
                });

                // Store class/module constant
                if (ClassNameRef.Block != null) {
                    // Path
                    Module Module = (Module)ClassNameRef.Block;
                    lock (Module.Constants)
                        Module.Constants[ClassName] = new ModuleReference(NewModule);
                }
                else if (ClassNameRef.IsLocalReference) {
                    // Local
                    Module Module = (ClassNameRef.Instance ?? CurrentInstance).Module!;
                    lock (Module)
                        Module.Constants[ClassName] = new ModuleReference(NewModule);
                }
            }
            else {
                throw new InternalErrorException($"{DefineClassStatement.Location}: Invalid class/module name: {ClassNameObject}");
            }
            return Interpreter.Nil;
        }
        async Task<Instances> InterpretYieldStatement(YieldStatement YieldStatement, Method? OnYield) {
            if (OnYield != null) {
                List<Instance> YieldArgs = YieldStatement.YieldValues != null
                    ? await InterpretExpressionsAsync(YieldStatement.YieldValues)
                    : new();
                await OnYield.Call(this, null, YieldArgs, BreakHandleType: BreakHandleType.Destroy);
            }
            else {
                throw new RuntimeException($"{YieldStatement.Location}: No block given to yield to");
            }
            return Interpreter.Nil;
        }
        async Task<Instances> InterpretIfBranchesStatement(IfBranchesStatement IfStatement) {
            for (int i = 0; i < IfStatement.Branches.Count; i++) {
                IfExpression Branch = IfStatement.Branches[i];
                if (Branch.Condition != null) {
                    Instance ConditionResult = await InterpretExpressionAsync(Branch.Condition);
                    if (ConditionResult.IsTruthy != Branch.Inverse) {
                        return await InternalInterpretAsync(Branch.Statements);
                    }
                }
                else {
                    return await InternalInterpretAsync(Branch.Statements);
                }
            }
            return Interpreter.Nil;
        }
        async Task<Instances> InterpretAssignmentExpression(AssignmentExpression AssignmentExpression, ReturnType ReturnType) {
            async Task AssignToVariable(VariableReference Variable, Instance Value) {
                switch (Variable.Token.Type) {
                    case Phase2TokenType.LocalVariableOrMethod:
                        lock (CurrentBlock.LocalVariables)
                            CurrentBlock.LocalVariables[Variable.Token.Value!] = Value;
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
        async Task<Instances> InterpretUndefineMethodStatement(UndefineMethodStatement UndefineMethodStatement) {
            string MethodName = UndefineMethodStatement.MethodName.Token.Value!;
            if (MethodName == "initialize") {
                await Warn($"{UndefineMethodStatement.MethodName.Token.Location}: undefining 'initialize' may cause serious problems");
            }
            if (!CurrentModule.InstanceMethods.Remove(MethodName)) {
                throw new RuntimeException($"{UndefineMethodStatement.MethodName.Token.Location}: Undefined method '{MethodName}' for {CurrentModule.Name}");
            }
            return Interpreter.Nil;
        }
        async Task<Instances> InterpretDefinedExpression(DefinedExpression DefinedExpression) {
            if (DefinedExpression.Expression is MethodCallExpression || DefinedExpression.Expression is PathExpression) {
                return new StringInstance(Interpreter.String, "method");
            }
            else if (DefinedExpression.Expression is ObjectTokenExpression ObjectToken) {
                if (ObjectToken.Token.Type == Phase2TokenType.LocalVariableOrMethod) {
                    if (CurrentScope.LocalVariables.ContainsKey(ObjectToken.Token.Value!)) {
                        return new StringInstance(Interpreter.String, "local-variable");
                    }
                    else if (CurrentInstance.InstanceMethods.ContainsKey(ObjectToken.Token.Value!)) {
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
                    throw new NotImplementedException("Defined? not yet implemented for constants");
                }
                else if (ObjectToken.Token.Type == Phase2TokenType.InstanceVariable) {
                    throw new NotImplementedException("Defined? not yet implemented for instance variables");
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
            else {
                throw new InternalErrorException($"{DefinedExpression.Location}: Unknown expression type for defined?: {DefinedExpression.Expression.GetType().Name}");
            }
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
        async Task<Instances> InterpretExpressionAsync(Expression Expression, ReturnType ReturnType = ReturnType.InterpretResult, Method? OnYield = null) {
            // Set approximate location
            ApproximateLocation = Expression.Location;

            // Interpret expression
            return Expression switch {
                MethodCallExpression MethodCallExpression => await InterpretMethodCallExpression(MethodCallExpression),
                ObjectTokenExpression ObjectTokenExpression => await InterpretObjectTokenExpression(ObjectTokenExpression, ReturnType),
                IfExpression IfExpression => await InterpretIfExpression(IfExpression),
                WhileExpression WhileExpression => await InterpretWhileExpression(WhileExpression),
                ArrayExpression ArrayExpression => await InterpretArrayExpression(ArrayExpression),
                HashExpression HashExpression => await InterpretHashExpression(HashExpression),
                WhileStatement WhileStatement => await InterpretWhileStatement(WhileStatement),
                SelfExpression => new ModuleReference(CurrentModule),
                LogicalExpression LogicalExpression => await InterpretLogicalExpression(LogicalExpression),
                DefineMethodStatement DefineMethodStatement => await InterpretDefineMethodStatement(DefineMethodStatement),
                DefineClassStatement DefineClassStatement => await InterpretDefineClassStatement(DefineClassStatement),
                ReturnStatement ReturnStatement => throw new ReturnException(
                                                ReturnStatement.ReturnValues != null
                                                ? await InterpretExpressionsAsync(ReturnStatement.ReturnValues)
                                                : Interpreter.Nil),
                BreakStatement => throw new BreakException(),
                YieldStatement YieldStatement => await InterpretYieldStatement(YieldStatement, OnYield),
                IfBranchesStatement IfBranchesStatement => await InterpretIfBranchesStatement(IfBranchesStatement),
                AssignmentExpression AssignmentExpression => await InterpretAssignmentExpression(AssignmentExpression, ReturnType),
                UndefineMethodStatement UndefineMethodStatement => await InterpretUndefineMethodStatement(UndefineMethodStatement),
                DefinedExpression DefinedExpression => await InterpretDefinedExpression(DefinedExpression),
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
        internal async Task<Instances> InternalInterpretAsync(List<Expression> Statements, Method? OnYield = null) {
            // Interpret statements
            Instances LastExpression = Interpreter.Nil;
            for (int Index = 0; Index < Statements.Count; Index++) {
                // Interpret expression and store the result
                Expression Statement = Statements[Index];
                LastExpression = await InterpretExpressionAsync(Statement, OnYield: OnYield);
            }
            // Return last expression
            return LastExpression;
        }
        internal async Task<Instances> InternalEvaluateAsync(string Code) {
            // Get statements from code
            List<Phase1.Phase1Token> Tokens = Phase1.GetPhase1Tokens(Code);
            List<Expression> Statements = ObjectsToExpressions(Tokens, ExpressionsType.Statements);

            // Interpret statements
            return await InternalInterpretAsync(Statements);
        }
        
        public async Task<Instances> InterpretAsync(List<Expression> Statements, Method? OnYield = null) {
            // Debounce
            if (Running) throw new ApiException("The script is already running.");
            Running = true;

            // Interpret statements and store the result
            Instances LastExpression;
            try {
                LastExpression = await InternalInterpretAsync(Statements, OnYield);
            }
            catch (BreakException) {
                throw new SyntaxErrorException($"{ApproximateLocation}: Invalid break (break must be in a loop)");
            }
            catch (ReturnException Ex) {
                return Ex.Instances;
            }
            catch (ExitException) {
                return Interpreter.Nil;
            }
            finally {
                // Deactivate debounce
                Running = false;
            }
            return LastExpression;
        }
        public Instances Interpret(List<Expression> Statements) {
            return InterpretAsync(Statements).Result;
        }
        public async Task<Instances> EvaluateAsync(string Code) {
            List<Phase1.Phase1Token> Tokens = Phase1.GetPhase1Tokens(Code);
            List<Expression> Statements = ObjectsToExpressions(Tokens, ExpressionsType.Statements);

            /*Console.WriteLine(Statements.Inspect());
            Console.Write("Press enter to continue.");
            Console.ReadLine();*/

            return await InterpretAsync(Statements);
        }
        public Instances Evaluate(string Code) {
            return EvaluateAsync(Code).Result;
        }

        public Script(Interpreter interpreter, bool allowUnsafeApi = true) {
            Interpreter = interpreter;
            AllowUnsafeApi = allowUnsafeApi;

            CurrentObject.Push(interpreter.RootModule);
            CurrentObject.Push(interpreter.RootInstance);
            CurrentObject.Push(interpreter.RootScope);
        }
    }
}
