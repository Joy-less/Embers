using static Embers.Phase2;

namespace Embers
{
    public class Interpreter
    {
        public readonly Class RootClass;
        public readonly Scope RootScope;
        readonly Dictionary<string, Instance> GlobalVariables = new();
        Class CurrentClass;
        Scope CurrentScope;
        Block CurrentBlock;
        void SetCurrentClass(Class NewCurrentClass) { CurrentClass = NewCurrentClass; CurrentBlock = NewCurrentClass; }
        void SetCurrentScope(Scope NewCurrentScope) { CurrentScope = NewCurrentScope; CurrentBlock = NewCurrentScope; }

        public readonly Class NilClass;
        public readonly Class TrueClass;
        public readonly Class FalseClass;
        public readonly Class String;
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
            public readonly Dictionary<string, Instance> ClassVariables = new();
            public Method Constructor;
            public Class(string name, Class? parent, Method? constructor = null) : base(parent) {
                Name = name;
                if (constructor != null) {
                    Constructor = constructor;
                }
                else {
                    Constructor = new Method(async Input => {
                        return Input.Interpreter.Nil;
                    }, 0);
                }
            }
            public bool TryGetMethod(string Name, out Method? Method) {
                Block? CurrentBlock = this;
                do {
                    if (CurrentBlock is Class CurrentClass && CurrentClass.Methods.TryGetValue(Name, out Method? FindMethod)) {
                        Method = FindMethod;
                        return true;
                    }
                    CurrentBlock = CurrentBlock.Parent;
                } while (CurrentBlock != null);
                Method = null;
                return false;
            }
        }
        public class Instance {
            /*public bool IsA<T>() {
                return GetType() == typeof(T);
            }*/
            public readonly Class Class;
            public virtual Dictionary<string, Instance> InstanceVariables { get; } = new();
            public virtual object? Object { get { return null; } }
            public virtual bool Boolean { get { throw new ApiException("Instance is not a boolean"); } }
            public virtual string String { get { throw new ApiException("Instance is not a string"); } }
            public virtual long Integer { get { throw new ApiException("Instance is not an integer"); } }
            public virtual double Float { get { throw new ApiException("Instance is not a float"); } }
            // public virtual Method Method { get { throw new ApiException("Instance is not a method"); } }
            public virtual Class ClassRef { get { throw new ApiException("Instance is not a class reference"); } }
            public virtual Method MethodRef { get { throw new ApiException("Instance is not a method reference"); } }
            public virtual string Inspect() {
                return ToString()!;
            }
            public static Instance CreateFromToken(Interpreter Interpreter, Phase2Token Token) {
                return Token.Type switch {
                    Phase2TokenType.Nil => Interpreter.Nil,
                    Phase2TokenType.True => Interpreter.True,
                    Phase2TokenType.False => Interpreter.False,
                    Phase2TokenType.String => new StringInstance(Interpreter.String, Token.Value!),
                    Phase2TokenType.Integer => new IntegerInstance(Interpreter.Integer, long.Parse(Token.Value!)),
                    Phase2TokenType.Float => new FloatInstance(Interpreter.Float, double.Parse(Token.Value!)),
                    _ => throw new InternalErrorException($"Cannot create new object from token type {Token.Type}")
                };
            }
            protected Instance(Class fromClass) {
                Class = fromClass;
            }
            public static async Task<Instance> New(Interpreter Interpreter, Class fromClass, Instances Arguments) {
                Instance NewInstance = new(fromClass);
                Instances ConstructorReturn = await NewInstance.Class.Constructor.Call(Interpreter, NewInstance, Arguments);
                if (ConstructorReturn.Instance != null) {
                    return ConstructorReturn.Instance;
                }
                else {
                    throw new InternalErrorException("Constructor did not return a single instance");
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
                return "\"" + Value + "\"";
            }
            public StringInstance(Class fromClass, string value) : base(fromClass) {
                Value = value;
            }
            public void SetValue(string value) {
                Value = value;
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
                return Value.ToString("0.0");
            }
            public FloatInstance(Class fromClass, double value) : base(fromClass) {
                Value = value;
            }
            public void SetValue(double value) {
                Value = value;
            }
        }
        public class VariableReference : Instance {
            public override Dictionary<string, Instance> InstanceVariables { get { throw new ApiException($"{nameof(VariableReference)} instance does not have instance variables"); } }
            public Block Block;
            public Phase2Token Token;
            public override string Inspect() {
                return $"{Block.GetType().Name}::{Token.Inspect()}";
            }
            public VariableReference(Block block, Phase2Token token) : base(null) {
                Block = block;
                Token = token;
            }
        }
        public class ScopeReference : Instance {
            public override Dictionary<string, Instance> InstanceVariables { get { throw new ApiException($"{nameof(ScopeReference)} instance does not have instance variables"); } }
            public Scope Scope;
            public override string Inspect() {
                return Scope.GetType().Name;
            }
            public ScopeReference(Scope scope) : base(null) {
                Scope = scope;
            }
        }
        public class ClassReference : Instance {
            public override Dictionary<string, Instance> InstanceVariables { get { throw new ApiException($"{nameof(ClassReference)} instance does not have instance variables"); } }
            public override object? Object { get { return Class; } }
            public override Class ClassRef { get { return Class; } }
            public override string Inspect() {
                return Class.Name;
            }
            public ClassReference(Class _class) : base(_class) { }
        }
        public class MethodReference : Instance {
            readonly Method Method;
            public override Dictionary<string, Instance> InstanceVariables { get { throw new ApiException($"{nameof(MethodReference)} instance does not have instance variables"); } }
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
            public async Task<Instances> Call(Interpreter Interpreter, Instance OnInstance, Instances Arguments, Method? OnYield = null) {
                if (ArgumentCountRange.IsInRange(Arguments.Count)) {
                    // Create temporary scope
                    Scope PreviousScope = Interpreter.CurrentScope;
                    Interpreter.SetCurrentScope(new Scope(PreviousScope));
                    // Set argument variables
                    for (int i = 0; i < ArgumentNames.Count; i++) {
                        MethodArgumentExpression Argument = ArgumentNames[i];
                        Instance GivenArgument;
                        if (i >= Arguments.Count && Argument.DefaultValue != null) {
                            GivenArgument = await Interpreter.InterpretExpressionAsync(Argument.DefaultValue);
                        }
                        else {
                            GivenArgument = Arguments[i];
                        }
                        Interpreter.CurrentScope.LocalVariables.Add(Argument.ArgumentName.Value!, GivenArgument);
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
            public async Task<Instances> Call(Interpreter Interpreter, Instance OnInstance) {
                return await Call(Interpreter, OnInstance, new Instances());
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
                        return Min.ToString()!;
                    }
                }
                else {
                    string MinString = Min != null ? Min.ToString()! : "";
                    string MaxString = Max != null ? Max.ToString()! : "";
                    return MinString + ".." + MaxString;
                }
            }
            public string Serialise() {
                return $"new IntRange({(Min != null ? Min : "null")}, {(Max != null ? Max : "null")})";
            }
        }
        public class Instances {
            // At least one of Instance or InstanceList will be null
            public readonly Instance? Instance;
            public readonly List<Instance>? InstanceList;
            public Instances(Instance? instance = null) {
                Instance = instance;
            }
            public Instances(List<Instance> instanceList) {
                InstanceList = instanceList;
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
            public int Count { get {
                return InstanceList != null ? InstanceList.Count : (Instance != null ? 1 : 0);
            } }
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

        async Task Warn(string Message) {
            await RootClass.Methods["warn"].Call(this, new ClassReference(RootClass), new StringInstance(String, Message));
        }

        async Task<Instances> InterpretExpressionAsync(Expression Expression, bool ReturnVariableReference = false) {
            // Method call
            if (Expression is MethodCallExpression MethodCallExpression) {
                Instance MethodPath = await InterpretExpressionAsync(MethodCallExpression.MethodPath, true);
                if (MethodPath is VariableReference MethodReference) {
                    Class MethodClass = MethodReference.Block as Class ?? CurrentClass;
                    Instance MethodOwner;
                    if (MethodCallExpression.MethodPath is PathExpression MethodCallPathExpression) {
                        MethodOwner = await InterpretExpressionAsync(MethodCallPathExpression.ParentObject);
                    }
                    else {
                        MethodOwner = new ClassReference(MethodClass);
                    }
                    return await MethodClass.Methods[MethodReference.Token.Value!].Call(
                        this, MethodOwner, await InterpretExpressionsAsync(MethodCallExpression.Arguments), MethodCallExpression.OnYield?.ToMethod()
                    );
                }
                else {
                    throw new InternalErrorException($"MethodPath should be VariableReference, not {MethodPath.GetType().Name}");
                }
            }
            // Path or Object Token
            else if (Expression is ObjectTokenExpression ObjectTokenExpression) {
                // Path
                if (ObjectTokenExpression is PathExpression PathExpression) {
                    Instance ParentInstance = await InterpretExpressionAsync(PathExpression.ParentObject);
                    if (ObjectTokenExpression.Token.Type == Phase2TokenType.LocalVariableOrMethod || ObjectTokenExpression.Token.Type == Phase2TokenType.ConstantOrMethod) {
                        // Method
                        if (ParentInstance.Class.Methods.TryGetValue(ObjectTokenExpression.Token.Value!, out Method? FindMethod)) {
                            if (!ReturnVariableReference) {
                                return await FindMethod.Call(this, ParentInstance);
                            }
                            else {
                                return new VariableReference(ParentInstance.Class, ObjectTokenExpression.Token);
                            }
                        }
                        // Constant
                        else if (ParentInstance.Class.Constants.TryGetValue(ObjectTokenExpression.Token.Value!, out Instance? FindInstance)) {
                            return FindInstance;
                        }
                        // Error
                        else {
                            throw new RuntimeException($"Undefined method '{ObjectTokenExpression.Token.Value!}' for {ParentInstance.Class.Name}");
                        }
                    }
                    else {
                        throw new SyntaxErrorException($"Expected identifier after ., got {ObjectTokenExpression.Token.Type}");
                    }
                }
                // Local
                else {
                    // Literal
                    if (IsObjectToken(ObjectTokenExpression.Token)) {
                        return Instance.CreateFromToken(this, ObjectTokenExpression.Token);
                    }
                    else {
                        if (!ReturnVariableReference) {
                            // Local variable or method
                            if (ObjectTokenExpression.Token.Type == Phase2TokenType.LocalVariableOrMethod) {
                                // Local variable (priority)
                                if (CurrentBlock.TryGetLocalVariable(ObjectTokenExpression.Token.Value!, out Instance? Value)) {
                                    return Value!;
                                }
                                // Method
                                else if (CurrentClass.TryGetMethod(ObjectTokenExpression.Token.Value!, out Method? Method)) {
                                    return await Method!.Call(this, new ScopeReference(CurrentScope));
                                }
                                // Undefined
                                else {
                                    throw new RuntimeException($"Undefined local variable or method '{ObjectTokenExpression.Token.Value!}' for {CurrentScope}");
                                }
                            }
                            // Global variable
                            else if (ObjectTokenExpression.Token.Type == Phase2TokenType.GlobalVariable) {
                                if (GlobalVariables.TryGetValue(ObjectTokenExpression.Token.Value!, out Instance? Value)) {
                                    return Value;
                                }
                                else {
                                    return Nil;
                                }
                            }
                            // Constant
                            else if (ObjectTokenExpression.Token.Type == Phase2TokenType.ConstantOrMethod) {
                                // Constant (priority)
                                if (CurrentBlock.TryGetConstant(ObjectTokenExpression.Token.Value!, out Instance? ConstantValue)) {
                                    return ConstantValue!;
                                }
                                // Method
                                else if (CurrentClass.TryGetMethod(ObjectTokenExpression.Token.Value!, out Method? Method)) {
                                    return await Method!.Call(this, new ScopeReference(CurrentScope));
                                }
                                // Uninitialized
                                else {
                                    throw new RuntimeException($"Uninitialized constant '{ObjectTokenExpression.Token.Value!}' for {CurrentBlock}");
                                }
                            }
                            // Instance variable
                            else if (ObjectTokenExpression.Token.Type == Phase2TokenType.InstanceVariable) {
                                throw new NotImplementedException("Instance variables not yet implemented");
                            }
                            // Class variable
                            else if (ObjectTokenExpression.Token.Type == Phase2TokenType.ClassVariable) {
                                if (CurrentClass.ClassVariables.TryGetValue(ObjectTokenExpression.Token.Value!, out Instance? Value)) {
                                    return Value;
                                }
                                else {
                                    throw new RuntimeException($"Uninitialized class variable '{ObjectTokenExpression.Token.Value!}' for {CurrentClass}");
                                }
                            }
                            // Error
                            else {
                                throw new InternalErrorException($"Unknown variable type {ObjectTokenExpression.Token.Type}");
                            }
                        }
                        // Variable
                        else {
                            return new VariableReference(CurrentBlock, ObjectTokenExpression.Token);
                        }
                    }
                }
            }
            /*// Arithmetic
            else if (Expression is ArithmeticExpression ArithmeticExpression) {
                Instance Left = await InterpretExpression(ArithmeticExpression.Left);
                Instance Right = await InterpretExpression(ArithmeticExpression.Right);
                if (Left == null) {
                    throw new RuntimeException($"Cannot call {ArithmeticExpression.Operator} on nil value");
                }
                Method? Operation = ArithmeticExpression.Operator switch {
                    "+" => Left.Add,
                    "-" => Left.Subtract,
                    "*" => Left.Multiply,
                    "/" => Left.Divide,
                    "%" => Left.Modulo,
                    "**" => Left.Exponentiate,
                    _ => throw new InternalErrorException($"Operator not recognised: '{ArithmeticExpression.Operator}'")
                };
                if (Operation != null) {
                    return await Operation.Call(this, Right);
                }
                else {
                    throw new RuntimeException($"Undefined method '{ArithmeticExpression.Operator}' for {Left}");
                }
            }*/
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
                        else if (CurrentClass.Methods.ContainsKey(ObjectToken.Token.Value!)) {
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
            throw new InternalErrorException($"Not sure how to interpret expression {Expression.GetType().Name} ({Expression.Inspect()})");
        }
        async Task<List<Instance>> InterpretExpressionsAsync(List<Expression> Expressions) {
            List<Instance> Results = new();
            foreach (Expression Expression in Expressions) {
                Results.Add(await InterpretExpressionAsync(Expression));
            }
            return Results;
        }
        public async Task<Instances> InterpretAsync(List<Statement> Statements, Func<Instances, Task>? OnYield = null) {
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
                            throw new NotImplementedException();
                        case Phase2TokenType.ClassVariable:
                            throw new NotImplementedException();
                        default:
                            throw new InternalErrorException($"Assignment variable token is not a variable type (got {Variable.Token.Type})");
                    }
                }
                /*Phase2Object InterpretPath(List<Phase2Token> Path, Block CurrentBlock) {
                    if (IsVariableToken(Path[^1].Type)) {
                        // To do: change current block to actual block if present
                        return new VariableReference(CurrentBlock, Path[^1]);
                    }
                    else if (Path.Count == 1) {
                        return Path[0];
                    }
                    else {
                        throw new SyntaxErrorException($"Last token in path must be an identifier (got {Path[^1].Type})");
                    }
                }
                Phase2Object InterpretExpressionAsPath(Expression Expression, Block CurrentBlock) {
                    if (Expression is ValueExpression ValueExpression) {
                        return InterpretPath(ValueExpression.Path, CurrentBlock);
                    }
                    else {
                        throw new RuntimeException($"Cannot interpret {Expression.GetType().Name} {Expression.Inspect()} as a variable");
                    }
                }*/
                /*async Task<Instance> UseVariable(VariableReference Identifier, PathExpression IdentifierPath) {
                    // Local variable or method
                    if (Identifier.Token.Type == Phase2TokenType.LocalVariableOrMethod) {
                        // Local variable (priority)
                        if (Identifier.Block is Scope PathScope && PathScope.LocalVariables.TryGetValue(Identifier.Token.Value!, out Instance? Value)) {
                            return Value;
                        }
                        // Method
                        else if (Identifier.Block.FindAncestorWhichIsA<Class>()!.Methods.TryGetValue(Identifier.Token.Value!, out Method? Method)) {
                            return await Method.Call(this, await InterpretExpressionAsync(IdentifierPath) is VariableReference VarRef ? VarRef.Block : CurrentScope);
                        }
                        // Undefined
                        else {
                            throw new RuntimeException($"Undefined local variable or method '{Identifier.Token.Value!}' for {IdentifierPath.Inspect()}");
                        }
                    }
                    // Global variable
                    else if (Identifier.Token.Type == Phase2TokenType.GlobalVariable) {
                        if (GlobalVariables.TryGetValue(Identifier.Token.Value!, out Instance? Value)) {
                            return Value;
                        }
                        else {
                            return Nil;
                        }
                    }
                    // Error
                    else {
                        throw new InternalErrorException($"Using variable type {Identifier.Token.Type.GetType().Name} not implemented");
                    }
                }*/
                /*Instance GetDefinedResult(VariableReference Identifier) {
                    // Local variable or method
                    if (Identifier.Token.Type == Phase2TokenType.LocalVariableOrMethod) {
                        // Local variable (priority)
                        if (Identifier.Block is Scope PathScope && PathScope.LocalVariables.TryGetValue(Identifier.Token.Value!, out Instance? Value)) {
                            return new StringInstance(String, "local-variable");
                        }
                        // Method
                        else if (Identifier.Block.FindAncestorWhichIsA<Class>()!.Methods.TryGetValue(Identifier.Token.Value!, out Method? Method)) {
                            return new StringInstance(String, "method");
                        }
                        // Undefined
                        else {
                            return Nil;
                        }
                    }
                    // Global variable
                    else if (Identifier.Token.Type == Phase2TokenType.GlobalVariable) {
                        if (GlobalVariables.TryGetValue(Identifier.Token.Value!, out Instance? Value)) {
                            return new StringInstance(String, "global-variable");
                        }
                        else {
                            return Nil;
                        }
                    }
                    // Constant
                    else if (Identifier.Token.Type == Phase2TokenType.Constant) {
                        // return "constant";
                        return Nil;
                    }
                    // Instance variable
                    else if (Identifier.Token.Type == Phase2TokenType.InstanceVariable) {
                        // return "instance-variable";
                        return Nil;
                    }
                    // Class variable
                    else if (Identifier.Token.Type == Phase2TokenType.ClassVariable) {
                        if (CurrentClass.ClassVariables.TryGetValue(Identifier.Token.Value!, out Instance? Value)) {
                            return new StringInstance(String, "class-variable");
                        }
                        else {
                            return Nil;
                        }
                    }
                    // Other
                    else {
                        return new StringInstance(String, "expression");
                    }
                }*/
                

                if (Statement is ExpressionStatement ExpressionStatement) {
                    await InterpretExpressionAsync(ExpressionStatement.Expression);
                }
                else if (Statement is AssignmentStatement AssignmentStatement) {
                    Instance Right = await InterpretExpressionAsync(AssignmentStatement.Right);

                    Instance Left = await InterpretExpressionAsync(AssignmentStatement.Left, true);
                    if (Left is VariableReference LeftVariable) {
                        if (Right is Instance RightInstance) {
                            await AssignToVariable(LeftVariable, RightInstance);
                        }
                        else {
                            throw new InternalErrorException($"Assignment value should be an instance, but got {Right.GetType().Name}");
                        }
                    }
                    else {
                        throw new RuntimeException($"{Left.GetType()} cannot be the target of an assignment");
                    }
                }
                else if (Statement is DefineMethodStatement DefineMethodStatement) {
                    Instance MethodNameObject = await InterpretExpressionAsync(DefineMethodStatement.MethodName, true);
                    if (MethodNameObject is VariableReference MethodName) {
                        MethodName.Block.FindFirstAncestorOrSelfWhichIsA<Class>().Methods[MethodName.Token.Value!] = DefineMethodStatement.Method.ToMethod();
                    }
                    else {
                        throw new InternalErrorException($"Invalid method name: {MethodNameObject}");
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
                        // Interpret the statements inside the class definition
                        await InterpretAsync(DefineClassStatement.BlockStatements);
                        // Step back a class scope
                        SetCurrentClass(PreviousClass);
                        // Store class constant
                        ClassName.Block.Parent!.Constants[ClassName.Token.Value!] = new ClassReference(NewClass);
                    }
                    else {
                        throw new InternalErrorException($"Invalid class name: {ClassNameObject}");
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
                        throw new RuntimeException("No block given to yield to");
                    }
                }
                else {
                    throw new InternalErrorException($"Not sure how to interpret statement {Statement.GetType().Name}");
                }
            }
            return Nil;
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

        public Interpreter() {
            RootClass = new Class("RootClass", null);
            RootScope = new Scope(RootClass);
            CurrentClass = RootClass;
            CurrentScope = RootScope;
            CurrentBlock = RootScope;

            NilClass = new Class("NilClass", RootClass); RootClass.Constants.Add("NilClass", new ClassReference(NilClass)); Nil = new NilInstance(NilClass);
            TrueClass = new Class("TrueClass", RootClass); RootClass.Constants.Add("TrueClass", new ClassReference(TrueClass)); True = new TrueInstance(TrueClass);
            FalseClass = new Class("FalseClass", RootClass); RootClass.Constants.Add("FalseClass", new ClassReference(FalseClass)); False = new FalseInstance(FalseClass);
            String = new Class("String", RootClass, null); RootClass.Constants.Add("String", new ClassReference(String));
            Integer = new Class("Integer", RootClass, null); RootClass.Constants.Add("Integer", new ClassReference(Integer));
            Float = new Class("Float", RootClass, null); RootClass.Constants.Add("Float", new ClassReference(Float));

            Api.Setup(this);
        }
    }
}
