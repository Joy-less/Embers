using System.Text;
using static Embers.Interpreter;
using static Embers.Phase2;

namespace Embers
{
    public class Interpreter
    {
        public readonly bool AllowUnsafeApi;

        public readonly Instance RootInstance;
        public readonly Scope RootScope;
        readonly Dictionary<string, Instance> GlobalVariables = new();
        readonly Dictionary<string, SymbolInstance> Symbols = new();
        Class CurrentClass;
        Scope CurrentScope;
        Block CurrentBlock;
        Instance CurrentInstance;
        void SetCurrentClass(Class NewCurrentClass) { CurrentClass = NewCurrentClass; CurrentBlock = NewCurrentClass; }
        void SetCurrentScope(Scope NewCurrentScope) { CurrentScope = NewCurrentScope; CurrentBlock = NewCurrentScope; }

        public readonly Class NilClass;
        public readonly Class TrueClass;
        public readonly Class FalseClass;
        public readonly Class String;
        public readonly Class Symbol;
        public readonly Class Integer;
        public readonly Class Float;

        public readonly Instance Nil;
        public readonly Instance True;
        public readonly Instance False;

        // public abstract class Instance { }
        public class Block {
            public readonly Block? Parent;
            public readonly Dictionary<string, Instance> LocalVariables = new();
            public readonly Dictionary<string, Instance> Constants = new();
            public Block(Block? parent) {
                Parent = parent;
            }
            public T? FindFirstAncestorWhichIsA<T>() where T : Block {
                Block? Ancestor = Parent;
                T? AncestorAsT = null;
                while (Ancestor != null) {
                    if (Ancestor is T TypedAncestor) {
                        AncestorAsT = TypedAncestor;
                        break;
                    }
                    else
                        Ancestor = Ancestor.Parent;
                }
                return AncestorAsT;
            }
            public T FindFirstAncestorOrSelfWhichIsA<T>() where T : Block {
                if (this is T TThis) {
                    return TThis;
                }
                return FindFirstAncestorWhichIsA<T>()!;
            }
            public bool TryGetLocalVariable(string Name, out Instance? LocalVariable) {
                Block? CurrentBlock = this;
                do {
                    if (CurrentBlock.LocalVariables.TryGetValue(Name, out Instance? FindLocalVariable)) {
                        LocalVariable = FindLocalVariable;
                        return true;
                    }
                    CurrentBlock = CurrentBlock.Parent;
                } while (CurrentBlock != null);
                LocalVariable = null;
                return false;
            }
            public bool TryGetConstant(string Name, out Instance? Constant) {
                Block? CurrentBlock = this;
                do {
                    if (CurrentBlock.Constants.TryGetValue(Name, out Instance? FindConstant)) {
                        Constant = FindConstant;
                        return true;
                    }
                    CurrentBlock = CurrentBlock.Parent;
                } while (CurrentBlock != null);
                Constant = null;
                return false;
            }
        }
        public class Scope : Block {
            public Scope(Block? parent) : base(parent) { }
        }
        public class Class : Block {
            public readonly string Name;
            public readonly Dictionary<string, Method> Methods = new();
            public readonly Dictionary<string, Method> InstanceMethods = new();
            public readonly Dictionary<string, Instance> ClassVariables = new();
            public Class(string name, Class? parent) : base(parent) {
                Name = name;
                // Default method: new
                Methods.Add("new", new Method(async Input => {
                    Instance NewInstance = new(this);
                    if (NewInstance.InstanceMethods.TryGetValue("initialize", out Method? Initialize)) {
                        // Set instance
                        Instance PreviousInstance = Input.Interpreter.CurrentInstance;
                        Input.Interpreter.CurrentInstance = NewInstance;
                        // Call initialize
                        await Initialize.Call(Input.Interpreter, NewInstance, Input.Arguments);
                        // Step back an instance
                        Input.Interpreter.CurrentInstance = PreviousInstance;
                    }
                    else {
                        throw new RuntimeException($"Undefined method 'initialize' for {Name}");
                    }
                    return NewInstance;
                }, null));
                // Default class and instance methods
                Api.DefaultClassAndInstanceMethods.CopyTo(InstanceMethods);
                Api.DefaultClassAndInstanceMethods.CopyTo(Methods);
            }
        }
        public class Instance {
            /*public bool IsA<T>() {
                return GetType() == typeof(T);
            }*/
            public readonly Class Class;
            public virtual Dictionary<string, Instance> InstanceVariables { get; } = new();
            public virtual Dictionary<string, Method> InstanceMethods { get; } = new();
            public virtual object? Object { get { return null; } }
            public virtual bool Boolean { get { throw new ApiException("Instance is not a boolean"); } }
            public virtual string String { get { throw new ApiException("Instance is not a string"); } }
            public virtual long Integer { get { throw new ApiException("Instance is not an integer"); } }
            public virtual double Float { get { throw new ApiException("Instance is not a float"); } }
            public virtual Class ClassRef { get { throw new ApiException("Instance is not a class reference"); } }
            public virtual Method MethodRef { get { throw new ApiException("Instance is not a method reference"); } }
            public virtual string Inspect() {
                return ToString()!;
            }
            public virtual string LightInspect() {
                return Inspect();
            }
            public static async Task<Instance> CreateFromToken(Interpreter Interpreter, Phase2Token Token) {
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

                                string Formatted = (await Interpreter.EvaluateAsync(ToFormat))[0].LightInspect();
                                String = FirstHalf + Formatted + SecondHalf;
                                i = FirstHalf.Length - 1;
                            }
                        }
                        LastChara = Chara;
                    }
                    return new StringInstance(Interpreter.String, String);
                }

                return Token.Type switch {
                    Phase2TokenType.Nil => Interpreter.Nil,
                    Phase2TokenType.True => Interpreter.True,
                    Phase2TokenType.False => Interpreter.False,
                    Phase2TokenType.String => new StringInstance(Interpreter.String, Token.Value!),
                    Phase2TokenType.Integer => new IntegerInstance(Interpreter.Integer, Token.ValueAsLong),
                    Phase2TokenType.Float => new FloatInstance(Interpreter.Float, Token.ValueAsDouble),
                    _ => throw new InternalErrorException($"{Token.Location}: Cannot create new object from token type {Token.Type}")
                };
            }
            public Instance(Class fromClass) {
                Class = fromClass;
                // Copy instance methods
                if (this is not PseudoInstance) {
                    fromClass.InstanceMethods.CopyTo(InstanceMethods);
                }
            }
            public bool TryGetInstanceMethod(Interpreter Interpreter, string Name, out Method? Method) {
                if (InstanceMethods.TryGetValue(Name, out Method? FindMethod)) {
                    Method = FindMethod;
                    return true;
                }
                else if (Interpreter.RootInstance.InstanceMethods.TryGetValue(Name, out Method? FindRootMethod)) {
                    Method = FindRootMethod;
                    return true;
                }
                Method = null;
                return false;
            }
            public void AddOrUpdateInstanceMethod(string Name, Method Method) {
                InstanceMethods[Name] =
                Class.InstanceMethods[Name] = Method;
            }
            public bool IsTruthy => !(Object == null || false.Equals(Object));
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
        public abstract class PseudoInstance : Instance {
            public override Dictionary<string, Instance> InstanceVariables { get { throw new ApiException($"{this.GetType().Name} instance does not have instance variables"); } }
            public override Dictionary<string, Method> InstanceMethods { get { throw new ApiException($"{this.GetType().Name} instance does not have instance methods"); } }
            public PseudoInstance(Class? fromClass) : base(fromClass) { }
        }
        public class VariableReference : PseudoInstance {
            public Block? Block;
            public Instance? Instance;
            public Phase2Token Token;
            public override string Inspect() {
                return $"{(Block != null ? Block.GetType().Name : Instance!.Inspect())} var ref in {Token.Inspect()}";
            }
            public VariableReference(Block block, Phase2Token token) : base(null) {
                Block = block;
                Token = token;
            }
            public VariableReference(Instance instance, Phase2Token token) : base(null) {
                Instance = instance;
                Token = token;
            }
        }
        public class ScopeReference : PseudoInstance {
            public Scope Scope;
            public override string Inspect() {
                return Scope.GetType().Name;
            }
            public ScopeReference(Scope scope) : base(null) {
                Scope = scope;
            }
        }
        public class ClassReference : PseudoInstance {
            public override object? Object { get { return Class; } }
            public override Class ClassRef { get { return Class; } }
            public override string Inspect() {
                return Class.Name;
            }
            public override string LightInspect() {
                return Class.Name;
            }
            public ClassReference(Class _class) : base(_class) { }
        }
        public class MethodReference : PseudoInstance {
            readonly Method Method;
            public override object? Object { get { return Method; } }
            public override Method MethodRef { get { return Method; } }
            public override string Inspect() {
                return Method.ToString()!;
            }
            public MethodReference(Method method) : base(null) {
                Method = method;
            }
        }
        public class Method {
            Func<MethodInput, Task<Instances>> Function;
            public readonly IntRange ArgumentCountRange;
            public readonly List<MethodArgumentExpression> ArgumentNames;
            public Method(Func<MethodInput, Task<Instances>> function, IntRange? argumentCountRange, List<MethodArgumentExpression>? argumentNames = null) {
                Function = function;
                ArgumentCountRange = argumentCountRange ?? new IntRange();
                ArgumentNames = argumentNames ?? new();
            }
            public Method(Func<MethodInput, Task<Instances>> function, Range argumentCountRange, List<MethodArgumentExpression>? argumentNames = null) {
                Function = function;
                ArgumentCountRange = new IntRange(argumentCountRange);
                ArgumentNames = argumentNames ?? new();
            }
            public Method(Func<MethodInput, Task<Instances>> function, int argumentCount, List<MethodArgumentExpression>? argumentNames = null) {
                Function = function;
                ArgumentCountRange = new IntRange(argumentCount, argumentCount);
                ArgumentNames = argumentNames ?? new();
            }
            public async Task<Instances> Call(Interpreter Interpreter, Instance OnInstance, Instances? Arguments = null, Method? OnYield = null) {
                Arguments ??= new Instances();
                if (ArgumentCountRange.IsInRange(Arguments.Count)) {
                    // Create temporary scope
                    Scope PreviousScope = Interpreter.CurrentScope;
                    Interpreter.SetCurrentScope(new Scope(PreviousScope));
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
                            Interpreter.CurrentScope.LocalVariables.Add(Argument.ArgumentName.Value!, Arguments[i]);
                        }
                    }
                    // Call method
                    Instances ReturnValues = await Function(new MethodInput(Interpreter, OnInstance, Arguments, OnYield));
                    // Step back a scope
                    Interpreter.SetCurrentScope(PreviousScope);
                    // Return method return value
                    return ReturnValues;
                }
                else {
                    throw new RuntimeException($"Wrong number of arguments (given {Arguments.Count}, expected {ArgumentCountRange})");
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
            public Interpreter Interpreter;
            public Instance Instance;
            public Instances Arguments;
            public Method? OnYield;
            public MethodInput(Interpreter interpreter, Instance instance, Instances arguments, Method? onYield = null) {
                Interpreter = interpreter;
                Instance = instance;
                Arguments = arguments;
                OnYield = onYield;
            }
        }
        public class IntRange {
            public readonly int? Min;
            public readonly int? Max;
            public IntRange(int? min = null, int? max = null) {
                Min = min;
                Max = max;
            }
            public IntRange(Range range) {
                Min = range.Start.Value >= 0 ? range.Start.Value : null;
                Max = range.End.Value >= 0 ? range.End.Value : null;
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
                return $"new IntRange({(Min != null ? Min : "null")}, {(Max != null ? Max : "null")})";
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
        }

        public async Task Warn(string Message) {
            await RootInstance.InstanceMethods["warn"].Call(this, new ClassReference(RootInstance.Class), new StringInstance(String, Message));
        }
        public static Class CreateClass(Class Parent, string Name) {
            Class NewClass = new(Name, Parent);
            Parent.Constants[Name] = new ClassReference(NewClass);
            return NewClass;
        }
        public Class CreateClass(string Name) {
            return CreateClass(RootInstance.Class, Name);
        }
        T CreateTemporaryClassScope<T>(Class Class, Func<T> Do) {
            // Create temporary class scope
            Class PreviousClass = CurrentClass;
            CurrentClass = Class;
            // Do action
            T Result = Do();
            // Step back a class
            CurrentClass = PreviousClass;
            // Return result
            return Result;
        }
        T CreateTemporaryInstanceScope<T>(Instance Instance, Func<T> Do) {
            // Create temporary instance scope
            Instance PreviousInstance = CurrentInstance;
            CurrentInstance = Instance;
            // Do action
            T Result = Do();
            // Step back an instance
            CurrentInstance = PreviousInstance;
            // Return result
            return Result;
        }
        SymbolInstance GetSymbol(string Value) {
            if (Symbols.TryGetValue(Value, out SymbolInstance? FindSymbolInstance)) {
                return FindSymbolInstance;
            }
            else {
                SymbolInstance SymbolInstance = new(Symbol, Value);
                Symbols[Value] = SymbolInstance;
                return SymbolInstance;
            }
        }

        async Task<Instances> InterpretExpressionAsync(Expression Expression, bool ReturnVariableReference = false) {
            // Method call
            if (Expression is MethodCallExpression MethodCallExpression) {
                Instance MethodPath = await InterpretExpressionAsync(MethodCallExpression.MethodPath, true);
                if (MethodPath is VariableReference MethodReference) {
                    // Static method
                    if (MethodReference.Block != null) {
                        // Get class which owns method
                        Class MethodClass = MethodReference.Block as Class ?? CurrentClass;
                        // Get instance of the class which owns method
                        Instance MethodOwner;
                        if (MethodCallExpression.MethodPath is PathExpression MethodCallPathExpression) {
                            MethodOwner = await InterpretExpressionAsync(MethodCallPathExpression.ParentObject);
                        }
                        else {
                            MethodOwner = new ClassReference(MethodClass);
                        }
                        // Call class method
                        return await CreateTemporaryClassScope(MethodClass, async () =>
                            await MethodClass.Methods[MethodReference.Token.Value!].Call(
                                this, MethodOwner, await InterpretExpressionsAsync(MethodCallExpression.Arguments), MethodCallExpression.OnYield?.Method
                            )
                        );
                    }
                    // Instance method
                    else {
                        // Call instance method
                        Instance MethodInstance = MethodReference.Instance!;
                        return await CreateTemporaryInstanceScope(MethodInstance, async () =>
                            await MethodReference.Instance!.InstanceMethods[MethodReference.Token.Value!].Call(
                                this, MethodReference.Instance, await InterpretExpressionsAsync(MethodCallExpression.Arguments), MethodCallExpression.OnYield?.Method
                            )
                        );
                    }
                }
                else {
                    throw new InternalErrorException($"{MethodCallExpression.Location}: MethodPath should be VariableReference, not {MethodPath.GetType().Name}");
                }
            }
            // Path or Object Token
            else if (Expression is ObjectTokenExpression ObjectTokenExpression) {
                // Path
                if (ObjectTokenExpression is PathExpression PathExpression) {
                    Instance ParentInstance = await InterpretExpressionAsync(PathExpression.ParentObject);
                    // Static method
                    if (ParentInstance is ClassReference ParentClass) {
                        // Method
                        if (ParentClass.Class.Methods.TryGetValue(PathExpression.Token.Value!, out Method? FindMethod)) {
                            if (!ReturnVariableReference) {
                                // Call class method
                                return await CreateTemporaryClassScope(ParentClass.Class, async () =>
                                    await FindMethod.Call(this, ParentClass)
                                );
                            }
                            else {
                                return new VariableReference(ParentClass.Class, PathExpression.Token);
                            }
                        }
                        // Error
                        else {
                            throw new RuntimeException($"{PathExpression.Token.Location}: Undefined method '{PathExpression.Token.Value!}' for {ParentClass.Class.Name}");
                        }
                    }
                    // Instance method
                    else {
                        // Method
                        if (ParentInstance.InstanceMethods.TryGetValue(PathExpression.Token.Value!, out Method? FindMethod)) {
                            if (!ReturnVariableReference) {
                                // Call instance method
                                return await CreateTemporaryInstanceScope(ParentInstance, async () =>
                                    await FindMethod.Call(this, ParentInstance)
                                );
                            }
                            else {
                                return new VariableReference(ParentInstance, PathExpression.Token);
                            }
                        }
                        // Error
                        else {
                            throw new RuntimeException($"{PathExpression.Token.Location}: Undefined method '{PathExpression.Token.Value!}' for {ParentInstance.Class.Name} instance");
                        }
                    }
                }
                // Constant Path
                else if (ObjectTokenExpression is ConstantPathExpression ConstantPathExpression) {
                    Instance ParentInstance = await InterpretExpressionAsync(ConstantPathExpression.ParentObject);
                    // Constant
                    if (ParentInstance.Class.Constants.TryGetValue(ConstantPathExpression.Token.Value!, out Instance? ConstantValue)) {
                        if (!ReturnVariableReference) {
                            return ConstantValue;
                        }
                        else {
                            return new VariableReference(ParentInstance.Class, ConstantPathExpression.Token);
                        }
                    }
                    // Error
                    else {
                        throw new RuntimeException($"{ConstantPathExpression.Token.Location}: Uninitialized constant {ConstantPathExpression.Inspect()}");
                    }
                }
                // Local
                else {
                    // Literal
                    if (ObjectTokenExpression.Token.IsObjectToken) {
                        return await Instance.CreateFromToken(this, ObjectTokenExpression.Token);
                    }
                    else {
                        if (!ReturnVariableReference) {

                            switch (ObjectTokenExpression.Token.Type) {
                                // Local variable or method
                                case Phase2TokenType.LocalVariableOrMethod: {
                                    // Local variable (priority)
                                    if (CurrentBlock.TryGetLocalVariable(ObjectTokenExpression.Token.Value!, out Instance? Value)) {
                                        return Value!;
                                    }
                                    // Method
                                    else if (CurrentInstance.TryGetInstanceMethod(this, ObjectTokenExpression.Token.Value!, out Method? Method)) {
                                        return await Method!.Call(this, new ScopeReference(CurrentScope));
                                    }
                                    // Undefined
                                    else {
                                        throw new RuntimeException($"{ObjectTokenExpression.Token.Location}: Undefined local variable or method '{ObjectTokenExpression.Token.Value!}' for {CurrentScope}");
                                    }
                                }
                                // Global variable
                                case Phase2TokenType.GlobalVariable: {
                                    if (GlobalVariables.TryGetValue(ObjectTokenExpression.Token.Value!, out Instance? Value)) {
                                        return Value;
                                    }
                                    else {
                                        return Nil;
                                    }
                                }
                                // Constant
                                case Phase2TokenType.ConstantOrMethod: {
                                    // Constant (priority)
                                    if (CurrentBlock.TryGetConstant(ObjectTokenExpression.Token.Value!, out Instance? ConstantValue)) {
                                        return ConstantValue!;
                                    }
                                    // Method
                                    else if (CurrentInstance.TryGetInstanceMethod(this, ObjectTokenExpression.Token.Value!, out Method? Method)) {
                                        return await Method!.Call(this, new ScopeReference(CurrentScope));
                                    }
                                    // Uninitialized
                                    else {
                                        throw new RuntimeException($"{ObjectTokenExpression.Token.Location}: Uninitialized constant '{ObjectTokenExpression.Token.Value!}' for {CurrentBlock}");
                                    }
                                }
                                // Instance variable
                                case Phase2TokenType.InstanceVariable: {
                                    if (CurrentInstance.InstanceVariables.TryGetValue(ObjectTokenExpression.Token.Value!, out Instance? Value)) {
                                        return Value;
                                    }
                                    else {
                                        return Nil;
                                    }
                                }
                                // Class variable
                                case Phase2TokenType.ClassVariable: {
                                    if (CurrentClass.ClassVariables.TryGetValue(ObjectTokenExpression.Token.Value!, out Instance? Value)) {
                                        return Value;
                                    }
                                    else {
                                        throw new RuntimeException($"{ObjectTokenExpression.Token.Location}: Uninitialized class variable '{ObjectTokenExpression.Token.Value!}' for {CurrentClass}");
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
                            return new VariableReference(CurrentInstance, ObjectTokenExpression.Token);
                        }
                    }
                }
            }
            // If
            else if (Expression is IfExpression IfExpression) {
                if (IfExpression.Condition == null || (await InterpretExpressionAsync(IfExpression.Condition))[0].IsTruthy) {
                    return await InterpretAsync(IfExpression.Statements);
                }
                else return Nil;
            }
            // While
            else if (Expression is WhileExpression WhileExpression) {
                while ((await InterpretExpressionAsync(WhileExpression.Condition!))[0].IsTruthy) {
                    await InterpretAsync(WhileExpression.Statements);
                }
                return Nil;
            }
            // Self
            else if (Expression is SelfExpression) {
                return new ClassReference(CurrentClass);
            }
            // Defined?
            else if (Expression is DefinedExpression DefinedExpression) {
                if (DefinedExpression.Expression is MethodCallExpression || DefinedExpression.Expression is PathExpression) {
                    return new StringInstance(String, "method");
                }
                else if (DefinedExpression.Expression is ObjectTokenExpression ObjectToken) {
                    if (ObjectToken.Token.Type == Phase2TokenType.LocalVariableOrMethod) {
                        if (CurrentScope.LocalVariables.ContainsKey(ObjectToken.Token.Value!)) {
                            return new StringInstance(String, "local-variable");
                        }
                        else if (CurrentInstance.InstanceMethods.ContainsKey(ObjectToken.Token.Value!)) {
                            return new StringInstance(String, "method");
                        }
                        else {
                            return Nil;
                        }
                    }
                    else if (ObjectToken.Token.Type == Phase2TokenType.GlobalVariable) {
                        if (GlobalVariables.ContainsKey(ObjectToken.Token.Value!)) {
                            return new StringInstance(String, "global-variable");
                        }
                        else {
                            return Nil;
                        }
                    }
                    else if (ObjectToken.Token.Type == Phase2TokenType.ConstantOrMethod) {
                        throw new NotImplementedException("Defined? not yet implemented for constants");
                    }
                    else if (ObjectToken.Token.Type == Phase2TokenType.InstanceVariable) {
                        throw new NotImplementedException("Defined? not yet implemented for instance variables");
                    }
                    else if (ObjectToken.Token.Type == Phase2TokenType.ClassVariable) {
                        if (CurrentClass.ClassVariables.ContainsKey(ObjectToken.Token.Value!)) {
                            return new StringInstance(String, "class-variable");
                        }
                        else {
                            return Nil;
                        }
                    }
                    else {
                        return new StringInstance(String, "expression");
                    }
                }
                else {
                    throw new InternalErrorException($"Unknown expression type for defined?: {DefinedExpression.Expression.GetType().Name}");
                }
            }
            // Unknown
            throw new InternalErrorException($"{Expression.Location}: Not sure how to interpret expression {Expression.GetType().Name} ({Expression.Inspect()})");
        }
        async Task<List<Instance>> InterpretExpressionsAsync(List<Expression> Expressions) {
            List<Instance> Results = new();
            foreach (Expression Expression in Expressions) {
                Results.Add(await InterpretExpressionAsync(Expression));
            }
            return Results;
        }
        public async Task<Instances> InterpretAsync(List<Statement> Statements, Func<Instances, Task>? OnYield = null) {
            Instance LastExpression = Nil;
            for (int Index = 0; Index < Statements.Count; Index++) {
                Statement Statement = Statements[Index];

                async Task AssignToVariable(VariableReference Variable, Instance Value) {
                    switch (Variable.Token.Type) {
                        case Phase2TokenType.LocalVariableOrMethod:
                            CurrentBlock.LocalVariables[Variable.Token.Value!] = Value;
                            break;
                        case Phase2TokenType.GlobalVariable:
                            GlobalVariables[Variable.Token.Value!] = Value;
                            break;
                        case Phase2TokenType.ConstantOrMethod:
                            if (CurrentBlock.Constants.ContainsKey(Variable.Token.Value!))
                                await Warn($"Already initialized constant '{Variable.Token.Value!}'");
                            CurrentBlock.Constants[Variable.Token.Value!] = Value;
                            break;
                        case Phase2TokenType.InstanceVariable:
                            CurrentInstance.InstanceVariables[Variable.Token.Value!] = Value;
                            break;
                        case Phase2TokenType.ClassVariable:
                            CurrentClass.ClassVariables[Variable.Token.Value!] = Value;
                            break;
                        default:
                            throw new InternalErrorException($"{Variable.Token.Location}: Assignment variable token is not a variable type (got {Variable.Token.Type})");
                    }
                }
                
                if (Statement is ExpressionStatement ExpressionStatement) {
                    LastExpression = await InterpretExpressionAsync(ExpressionStatement.Expression);
                }
                else if (Statement is AssignmentStatement AssignmentStatement) {
                    Instance Right = await InterpretExpressionAsync(AssignmentStatement.Right);

                    Instance Left = await InterpretExpressionAsync(AssignmentStatement.Left, true);
                    if (Left is VariableReference LeftVariable) {
                        if (Right is Instance RightInstance) {
                            await AssignToVariable(LeftVariable, RightInstance);
                            LastExpression = Left;
                        }
                        else {
                            throw new InternalErrorException($"{LeftVariable.Token.Location}: Assignment value should be an instance, but got {Right.GetType().Name}");
                        }
                    }
                    else {
                        throw new RuntimeException($"{AssignmentStatement.Left.Location}: {Left.GetType()} cannot be the target of an assignment");
                    }
                }
                else if (Statement is DefineMethodStatement DefineMethodStatement) {
                    Instance MethodNameObject = await InterpretExpressionAsync(DefineMethodStatement.MethodName, true);
                    if (MethodNameObject is VariableReference MethodName) {
                        // Define static method
                        if (MethodName.Block != null) {
                            MethodName.Block.FindFirstAncestorOrSelfWhichIsA<Class>().Methods[MethodName.Token.Value!] = DefineMethodStatement.Method.Method;
                        }
                        // Define instance method
                        else {
                            MethodName.Instance!.AddOrUpdateInstanceMethod(MethodName.Token.Value!, DefineMethodStatement.Method.Method);
                        }
                    }
                    else {
                        throw new InternalErrorException($"{DefineMethodStatement.Location}: Invalid method name: {MethodNameObject}");
                    }
                }
                else if (Statement is DefineClassStatement DefineClassStatement) {
                    Instance ClassNameObject = await InterpretExpressionAsync(DefineClassStatement.ClassName, true);
                    if (ClassNameObject is VariableReference ClassName) {
                        /*// Replace this with monkey patching.
                        if (CurrentClass.Constants.ContainsKey(ClassName.Token.Value!)) {
                            await Warn($"Already initialized constant '{ClassName.Token.Value!}'");
                        }*/
                        // Create class
                        Class NewClass = new(ClassName.Token.Value!, ClassName.Class);
                        // Create temporary class scope
                        Class PreviousClass = CurrentClass;
                        SetCurrentClass(NewClass);
                        // Create temporary class instance
                        Instance PreviousInstance = CurrentInstance;
                        CurrentInstance = new Instance(NewClass);
                        // Interpret the statements inside the class definition
                        await InterpretAsync(DefineClassStatement.BlockStatements);
                        // Step back an instance
                        CurrentInstance = PreviousInstance;
                        // Step back a class scope
                        SetCurrentClass(PreviousClass);
                        // Store class constant
                        if (ClassName.Block != null) {
                            // Path
                            ClassName.Block.Parent!.Constants[ClassName.Token.Value!] = new ClassReference(NewClass);
                        }
                        else if (ClassName.Instance == CurrentInstance) {
                            // Local
                            CurrentInstance.Class.Constants[ClassName.Token.Value!] = new ClassReference(NewClass);
                        }
                        else {
                            throw new RuntimeException($"{ClassName.Inspect()} is not a class/module");
                        }
                    }
                    else {
                        throw new InternalErrorException($"{DefineClassStatement.Location}: Invalid class name: {ClassNameObject}");
                    }
                }
                else if (Statement is ReturnStatement ReturnStatement) {
                    return ReturnStatement.ReturnValues != null
                        ? await InterpretExpressionsAsync(ReturnStatement.ReturnValues)
                        : Nil;
                }
                else if (Statement is YieldStatement YieldStatement) {
                    if (OnYield != null) {
                        List<Instance> YieldArgs = YieldStatement.YieldValues != null
                            ? await InterpretExpressionsAsync(YieldStatement.YieldValues)
                            : new();
                        await OnYield(YieldArgs);
                    }
                    else {
                        throw new RuntimeException($"{YieldStatement.Location}: No block given to yield to");
                    }
                }
                else if (Statement is IfStatement IfStatement) {
                    for (int i = 0; i < IfStatement.Branches.Count; i++) {
                        IfExpression Branch = IfStatement.Branches[i];
                        if (Branch.Condition != null) {
                            Instance ConditionResult = await InterpretExpressionAsync(Branch.Condition);
                            if (ConditionResult.IsTruthy) {
                                await InterpretAsync(Branch.Statements);
                                break;
                            }
                        }
                        else {
                            await InterpretAsync(Branch.Statements);
                            break;
                        }
                    }
                }
                else {
                    throw new InternalErrorException($"{Statement.Location}: Not sure how to interpret statement {Statement.GetType().Name}");
                }
            }
            return LastExpression;
        }
        public Instances Interpret(List<Statement> Statements, Func<Instances, Task>? OnYield = null) {
            return InterpretAsync(Statements, OnYield).Result;
        }
        public async Task<Instances> EvaluateAsync(string Code) {
            List<Phase1.Phase1Token> Tokens = Phase1.GetPhase1Tokens(Code);
            List<Statement> Statements = GetStatements(Tokens);

            return await InterpretAsync(Statements);
        }
        public Instances Evaluate(string Code) {
            return EvaluateAsync(Code).Result;
        }
        public static string Serialise(string Code) {
            List<Phase1.Phase1Token> Tokens = Phase1.GetPhase1Tokens(Code);
            List<Statement> Statements = GetStatements(Tokens);

            return Statements.Serialise();
        }

        public Interpreter(bool allowUnsafeApi = true) {
            AllowUnsafeApi = allowUnsafeApi;

            RootInstance = new Instance(new Class("main", null)); Class RootClass = RootInstance.Class;
            RootScope = new Scope(RootClass);
            CurrentClass = RootClass;
            CurrentScope = RootScope;
            CurrentBlock = RootScope;
            CurrentInstance = RootInstance;

            NilClass = CreateClass("NilClass"); Nil = new NilInstance(NilClass);
            TrueClass = CreateClass("TrueClass"); True = new TrueInstance(TrueClass);
            FalseClass = CreateClass("FalseClass"); False = new FalseInstance(FalseClass);
            String = CreateClass("String");
            Symbol = CreateClass("Symbol");
            Integer = CreateClass("Integer");
            Float = CreateClass("Float");

            Api.Setup(this);
        }
    }
}
