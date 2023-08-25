using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Embers.Phase2;

namespace Embers
{
    public class Interpreter
    {
        readonly Api Api = new();
        readonly Class RootClass;
        readonly Scope RootScope;
        readonly Dictionary<string, Instance> GlobalVariables = new();
        Class CurrentClass;
        Scope CurrentScope;

        readonly Class NilClass;
        readonly Class TrueClass;
        readonly Class FalseClass;
        readonly Class String;
        readonly Class Integer;
        readonly Class Float;

        public readonly Instance Nil;
        public readonly Instance True;
        public readonly Instance False;

        // public abstract class Instance { }
        public class Block {
            public readonly Block? Parent;
            public Block(Block? parent) {
                Parent = parent;
            }
            public T? FindAncestorWhichIsA<T>() where T : Block {
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
        }
        public class Scope : Block {
            public readonly Dictionary<string, Instance> LocalVariables = new();
            public Scope(Block? parent) : base(parent) { }
        }
        public class Class : Block {
            public readonly string Name;
            public readonly Dictionary<string, Method> Methods = new();
            public readonly Dictionary<string, Class> Classes = new();
            public readonly Dictionary<string, Instance> ClassVariables = new();
            public Method Constructor;
            public Class(string name, Class? parent, Method? constructor = null) : base(parent) {
                Name = name;
                if (constructor != null) {
                    Constructor = constructor;
                }
                else {
                    Constructor = new Method(async (Interpreter, Instance, Arguments) => {
                        return Interpreter.Nil;
                    }, 0);
                }
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
            public virtual double Float { get { throw new ApiException("Instance is not a Float"); } }
            // public virtual Method Method { get { throw new Exception("Ruby object is not a method"); } }
            // public virtual Class Class { get { throw new Exception("Ruby object is not a class"); } }
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
            public static async Task<Instance> New(Interpreter Interpreter, Class fromClass, params Instance[] Arguments) {
                Instance NewInstance = new(fromClass);
                Instance ConstructorReturn = await NewInstance.Class.Constructor.Call(Interpreter, NewInstance, Arguments.ToList());
                if (ConstructorReturn is Instance Instance) {
                    return Instance;
                }
                else {
                    throw new InternalErrorException("Constructor did not return instance");
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
                return $"{Scope.GetType().Name}";
            }
            public ScopeReference(Scope scope) : base(null) {
                Scope = scope;
            }
        }
        public class ClassReference : Instance {
            public override Dictionary<string, Instance> InstanceVariables { get { throw new ApiException($"{nameof(ClassReference)} instance does not have instance variables"); } }
            public override string Inspect() {
                return $"{Class.GetType().Name}";
            }
            public ClassReference(Class _class) : base(_class) { }
        }
        /*public class RubyInteger : Instance {
            readonly long Value;
            public override object? Object { get { return Value; } }*/
           
        /*public class RubyBoolean : Instance {
            readonly bool Value;
            public override object Object { get { return Value; } }
            public override bool Boolean { get { return Value; } }
            public override string Inspect() {
                return Value ? "true" : "false";
            }
            public RubyBoolean(bool value) {
                Value = value;
            }
        }*/
        /*public class NilClass : Instance {
            public override object? Object { get { return null; } }
            public override string Inspect() {
                return "nil";
            }
            public NilClass() {
            }
        }
        public class RubyTrue : Instance {
            public override object? Object { get { return true; } }
            public override bool Boolean { get { return true; } }
            public override string Inspect() {
                return "true";
            }
            public RubyTrue() {
            }
        }
        public class RubyFalse : Instance {
            public override object? Object { get { return false; } }
            public override bool Boolean { get { return false; } }
            public override string Inspect() {
                return "false";
            }
            public RubyFalse() {
            }
        }
        public class RubyString : Instance {
            readonly string Value;
            public override object? Object { get { return Value; } }
            public override string String { get { return Value; } }
            public override string Inspect() {
                return "\"" + Value + "\"";
            }
            public RubyString(string value) {
                Value = value;

                Methods.Add("+", new Method(async (Interpreter Interpreter, List<Instance> Arguments) => {
                    return new RubyString(Value + Arguments[0].String);
                }, 1));
                Methods.Add("*", new Method(async (Interpreter Interpreter, List<Instance> Arguments) => {
                    StringBuilder JoinedString = new();
                    long DuplicateCount = Arguments[0].Integer;
                    for (long i = 0; i < DuplicateCount; i++) {
                        JoinedString.Append(Value);
                    }
                    return new RubyString(JoinedString.ToString());
                }, 1));
            }
        }
        public class RubyInteger : Instance {
            readonly long Value;
            public override object? Object { get { return Value; } }
            public override long Integer { get { return Value; } }
            public override double Float { get { return Value; } }
            public override string Inspect() {
                return Value.ToString();
            }
            public RubyInteger(long value) {
                Value = value;

                static Instance GetResult(double Result, bool RightIsInteger) {
                    if (RightIsInteger) {
                        return new RubyInteger((long)Result);
                    }
                    else {
                        return new RubyFloat(Result);
                    }
                }
                Methods.Add("+", new Method(async (Interpreter Interpreter, List<Instance> Arguments) => {
                    Instance Right = Arguments[0];
                    return GetResult(Value + Right.Float, Right is RubyInteger);
                }, 1));
                Methods.Add("-", new Method(async (Interpreter Interpreter, List<Instance> Arguments) => {
                    Instance Right = Arguments[0];
                    return GetResult(Value - Right.Float, Right is RubyInteger);
                }, 1));
                Methods.Add("*", new Method(async (Interpreter Interpreter, List<Instance> Arguments) => {
                    Instance Right = Arguments[0];
                    return GetResult(Value * Right.Float, Right is RubyInteger);
                }, 1));
                Methods.Add("/", new Method(async (Interpreter Interpreter, List<Instance> Arguments) => {
                    Instance Right = Arguments[0];
                    return GetResult(Value / Right.Float, Right is RubyInteger);
                }, 1));
                Methods.Add("%", new Method(async (Interpreter Interpreter, List<Instance> Arguments) => {
                    Instance Right = Arguments[0];
                    return GetResult(Value % Right.Float, Right is RubyInteger);
                }, 1));
                Methods.Add("**", new Method(async (Interpreter Interpreter, List<Instance> Arguments) => {
                    Instance Right = Arguments[0];
                    return GetResult(Math.Pow(Value, Right.Float), Right is RubyInteger);
                }, 1));
            }
        }
        public class RubyFloat : Instance {
            readonly double Value;
            public override object? Object { get { return Value; } }
            public override double Float { get { return Value; } }
            public override long Integer { get { return (long)Value; } }
            public override string Inspect() {
                return Value.ToString("0.0");
            }
            public RubyFloat(double value) {
                Value = value;

                Methods.Add("+", new Method(async (Interpreter Interpreter, List<Instance> Arguments) => {
                    Instance Right = Arguments[0];
                    return new RubyFloat(Value + Right.Float);
                }, 1));
                Methods.Add("-", new Method(async (Interpreter Interpreter, List<Instance> Arguments) => {
                    Instance Right = Arguments[0];
                    return new RubyFloat(Value - Right.Float);
                }, 1));
                Methods.Add("*", new Method(async (Interpreter Interpreter, List<Instance> Arguments) => {
                    Instance Right = Arguments[0];
                    return new RubyFloat(Value * Right.Float);
                }, 1));
                Methods.Add("/", new Method(async (Interpreter Interpreter, List<Instance> Arguments) => {
                    Instance Right = Arguments[0];
                    return new RubyFloat(Value / Right.Float);
                }, 1));
                Methods.Add("%", new Method(async (Interpreter Interpreter, List<Instance> Arguments) => {
                    Instance Right = Arguments[0];
                    return new RubyFloat(Value % Right.Float);
                }, 1));
                Methods.Add("**", new Method(async (Interpreter Interpreter, List<Instance> Arguments) => {
                    Instance Right = Arguments[0];
                    return new RubyFloat(Math.Pow(Value, Right.Float));
                }, 1));
            }
        }
        public class RubyMethod : Instance {
            readonly Method Value;
            public override object? Object { get { return Value; } }
            public override Method Method { get { return Value; } }
            public override string Inspect() {
                return Value.ToString()!;
            }
            public RubyMethod(Method value) {
                Value = value;
            }
        }
        public class RubyClass : Instance {
            readonly Class Value;
            public override object? Object { get { return Value; } }
            public override Class Class { get { return Value; } }
            public override string Inspect() {
                return Value.ToString()!;
            }
            public RubyClass(Class value) {
                Value = value;
            }
        }*/
        
        public class Method {
            Func<Interpreter, Instance, List<Instance>, Task<Instance>> Function;
            public IntRange ArgumentCountRange;
            public Method(Func<Interpreter, Instance, List<Instance>, Task<Instance>> function, IntRange? argumentCountRange) {
                Function = function;
                ArgumentCountRange = argumentCountRange ?? new IntRange();
            }
            public Method(Func<Interpreter, Instance, List<Instance>, Task<Instance>> function, Range argumentCountRange) {
                Function = function;
                ArgumentCountRange = new IntRange(
                    argumentCountRange.Start.Value >= 0 ? argumentCountRange.Start.Value : null,
                    argumentCountRange.End.Value >= 0 ? argumentCountRange.End.Value : null
                );
            }
            public Method(Func<Interpreter, Instance, List<Instance>, Task<Instance>> function, int argumentCount) {
                Function = function;
                ArgumentCountRange = new IntRange(argumentCount, argumentCount);
            }
            public async Task<Instance> Call(Interpreter Interpreter, Instance Instance, List<Instance> Arguments) {
                if (ArgumentCountRange.IsInRange(Arguments.Count)) {
                    // Create temporary scope
                    Scope PreviousScope = Interpreter.CurrentScope;
                    Interpreter.CurrentScope = new Scope(PreviousScope);
                    // Call method
                    Instance ReturnValue = await Function(Interpreter, Instance, Arguments);
                    // Step back a scope
                    Interpreter.CurrentScope = PreviousScope;
                    // Return method return value
                    return ReturnValue;
                }
                else {
                    throw new RuntimeException($"Too many or too few arguments given for method (expected {ArgumentCountRange}, got {Arguments.Count})");
                }
            }
            public async Task<Instance> Call(Interpreter Interpreter, Instance Instance, Instance Argument) {
                return await Call(Interpreter, Instance, new List<Instance>() {Argument});
            }
            public async Task<Instance> Call(Interpreter Interpreter, Instance Instance) {
                return await Call(Interpreter, Instance, new List<Instance>());
            }
            public void ChangeFunction(Func<Interpreter, Instance, List<Instance>, Task<Instance>> function) {
                Function = function;
            }
        }
        public class MethodScope : Scope {
            public readonly Method? Method;
            public MethodScope(Block? parent, Method method) : base(parent) {
                Method = method;
            }
        }
        public class IntRange {
            public readonly int? Min;
            public readonly int? Max;
            public IntRange(int? min = null, int? max = null) {
                Min = min;
                Max = max;
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
        }

        public async Task<Instance> InterpretAsync(List<Statement> Statements) {
            for (int Index = 0; Index < Statements.Count; Index++) {
                Statement Statement = Statements[Index];

                void AssignToVariable(VariableReference Variable, Instance Value) {
                    switch (Variable.Token.Type) {
                        case Phase2TokenType.LocalVariableOrMethod:
                            CurrentScope.LocalVariables[Variable.Token.Value!] = Value;
                            break;
                        case Phase2TokenType.GlobalVariable:
                            GlobalVariables[Variable.Token.Value!] = Value;
                            break;
                        case Phase2TokenType.Constant:
                            throw new NotImplementedException();
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
                async Task<Instance> InterpretExpressionAsync(Expression Expression, bool ReturnVariableReference = false) {
                    // Method call
                    if (Expression is MethodCallExpression MethodCallExpression) {
                        /*// Global method
                        if (MethodCallExpression.MethodName.Path.Count == 1) {
                            Phase2Object MethodName = MethodCallExpression.MethodName.Path[0];
                            if (MethodName is Phase2Token MethodNameToken && MethodNameToken.Value != null) {
                                if (CurrentClass.Methods.TryGetValue(MethodNameToken.Value, out Method? Value)) {
                                    return await Value.Call(this, await InterpretExpressions(MethodCallExpression.Arguments));
                                }
                                else {
                                    throw new RuntimeException($"Undefined method '{MethodNameToken.Value}' for {MethodCallExpression.MethodName.Inspect()}");
                                }
                            }
                        }*/
                        Instance MethodPath = await InterpretExpressionAsync(MethodCallExpression.MethodPath, true);
                        if (MethodPath is VariableReference MethodReference) {
                            Class MethodClass = MethodReference.Block as Class ?? CurrentClass;
                            Instance MethodOwner;
                            if (MethodCallExpression.MethodPath is PathExpression MethodCallPathExpression) {
                                MethodOwner = await InterpretExpressionAsync(((PathExpression)MethodCallExpression.MethodPath).RootObject);
                            }
                            else {
                                MethodOwner = new ClassReference(MethodClass);
                            }
                            return await MethodClass.Methods[MethodReference.Token.Value!].Call(this, MethodOwner, await InterpretExpressionsAsync(MethodCallExpression.Arguments));
                        }
                        else {
                            throw new InternalErrorException($"MethodPath should be VariableReference, not {MethodPath.GetType().Name}");
                        }
                    }
                    // Object Token / Path
                    else if (Expression is ObjectTokenExpression ObjectTokenExpression) {
                        /*Phase2Object PathResult = InterpretPath(ValueExpression.Path, CurrentScope);
                        if (PathResult is VariableReference PathVariable) {
                            return await UseVariable(PathVariable, ValueExpression);
                        }
                        else {
                            return Instance.CreateFromToken(this, (Phase2Token)PathResult);
                        }*/
                        // Path
                        if (ObjectTokenExpression is PathExpression PathExpression) {
                            Instance RootObject = await InterpretExpressionAsync(PathExpression.RootObject);
                            if (RootObject is Instance RootInstance) {
                                if (ObjectTokenExpression.Token.Type == Phase2TokenType.LocalVariableOrMethod || ObjectTokenExpression.Token.Type == Phase2TokenType.Constant) {
                                    if (RootInstance.Class.Methods.TryGetValue(ObjectTokenExpression.Token.Value!, out Method? FindMethod)) {
                                        if (!ReturnVariableReference) {
                                            return await FindMethod.Call(this, RootInstance);
                                        }
                                        else {
                                            return new VariableReference(RootInstance.Class, ObjectTokenExpression.Token);
                                        }
                                    }
                                    else if (RootInstance.Class.Classes.TryGetValue(ObjectTokenExpression.Token.Value!, out Class? FindClass)) {
                                        return new ClassReference(FindClass);
                                    }
                                    else {
                                        throw new RuntimeException($"Undefined method '{ObjectTokenExpression.Token.Value!}' for {RootInstance.Class.Name}");
                                    }
                                }
                                else {
                                    throw new SyntaxErrorException($"Expected identifier after ., got {ObjectTokenExpression.Token.Type}");
                                }
                            }
                            else {
                                throw new InternalErrorException($"Expected instance, got block ({RootObject})");
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
                                        if (CurrentScope.LocalVariables.TryGetValue(ObjectTokenExpression.Token.Value!, out Instance? Value)) {
                                            return Value;
                                        }
                                        // Method
                                        else if (CurrentScope.FindAncestorWhichIsA<Class>()!.Methods.TryGetValue(ObjectTokenExpression.Token.Value!, out Method? Method)) {
                                            return await Method.Call(this, new ScopeReference(CurrentScope));
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
                                        throw new InternalErrorException($"Using variable type {ObjectTokenExpression.Token.Type} not implemented");
                                    }
                                }
                                else {
                                    return new VariableReference(CurrentScope, ObjectTokenExpression.Token);
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
                            else if (ObjectToken.Token.Type == Phase2TokenType.Constant) {
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

                if (Statement is ExpressionStatement ExpressionStatement) {
                    await InterpretExpressionAsync(ExpressionStatement.Expression);
                }
                else if (Statement is AssignmentStatement AssignmentStatement) {
                    Instance Right = await InterpretExpressionAsync(AssignmentStatement.Right);

                    Instance Left = await InterpretExpressionAsync(AssignmentStatement.Left, true);
                    if (Left is VariableReference LeftVariable) {
                        if (Right is Instance RightInstance) {
                            AssignToVariable(LeftVariable, RightInstance);
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
                        /*if (MethodName.Block is Class MethodClass) {
                            MethodClass.Methods.Add(MethodName.Token.Value!, DefineMethodStatement.Method);
                        }
                        else {
                            throw new InternalErrorException($"Expected class from VariableReference.Block, got {MethodName.Block.GetType().Name} ({MethodName.Inspect()})");
                        }*/
                        CurrentClass.Methods.Add(MethodName.Token.Value!, DefineMethodStatement.Method);
                    }
                    else {
                        throw new InternalErrorException($"Invalid method name: {MethodNameObject}");
                    }
                }
                else {
                    throw new InternalErrorException($"Not sure how to interpret statement {Statement.GetType().Name}");
                }
            }
            return Nil;
        }
        public Instance Interpret(List<Statement> Statements) {
            return InterpretAsync(Statements).Result;
        }
        public async Task<Instance> EvaluateAsync(string Code) {
            List<Phase1.Phase1Token> Tokens = Phase1.GetPhase1Tokens(Code);
            List<Statement> Statements = GetStatements(Tokens);
            return await InterpretAsync(Statements);
        }
        public Instance Evaluate(string Code) {
            return EvaluateAsync(Code).Result;
        }

        public Interpreter() {
            RootClass = new Class("RootClass", null);
            RootScope = new Scope(RootClass);
            CurrentClass = RootClass;
            CurrentScope = RootScope;

            NilClass = new Class("NilClass", RootClass); RootClass.Classes.Add("NilClass", NilClass); Nil = new NilInstance(NilClass);
            TrueClass = new Class("TrueClass", RootClass); RootClass.Classes.Add("TrueClass", TrueClass); True = new TrueInstance(TrueClass);
            FalseClass = new Class("FalseClass", RootClass); RootClass.Classes.Add("FalseClass", FalseClass); False = new FalseInstance(FalseClass);
            String = new Class("String", RootClass, new Method(async (Interpreter, Instance, Arguments) => {
                if (Arguments.Count == 1) {
                    ((StringInstance)Instance).SetValue(Arguments[0].String); // Change to implicit conversion
                }
                return Nil;
            }, 0..1)); RootClass.Classes.Add("String", String);
            Integer = new Class("Integer", RootClass, null); RootClass.Classes.Add("Integer", Integer);
            Integer.Methods.Add("+", new Method(async (Interpreter, Instance, Arguments) => {
                return new IntegerInstance(Integer, Instance.Integer + Arguments[0].Integer);
            }, 1));
            Float = new Class("Float", RootClass, null); RootClass.Classes.Add("Float", Float);

            foreach (KeyValuePair<string, Method> Method in Api.GetBuiltInMethods()) {
                RootClass.Methods.Add(Method.Key, Method.Value);
            }
        }
    }
}
