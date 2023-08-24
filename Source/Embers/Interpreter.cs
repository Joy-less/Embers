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

        public abstract class InstanceOrBlock { }
        public class Block : InstanceOrBlock {
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
            public readonly Dictionary<string, Method> Methods = new();
            public readonly Dictionary<string, Class> Classes = new();
            public readonly Dictionary<string, Instance> ClassVariables = new();
            public Method Constructor;
            public Class(Class? parent, Method? constructor = null) : base(parent) {
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
        public class Instance : InstanceOrBlock {
            /*public bool IsA<T>() {
                return GetType() == typeof(T);
            }*/
            readonly Class Class;
            public readonly Dictionary<string, Instance> InstanceVariables = new();
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
                return await NewInstance.Class.Constructor.Call(Interpreter, NewInstance, Arguments.ToList());
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
            public Func<Interpreter, InstanceOrBlock, List<Instance>, Task<Instance>> Function;
            public IntRange ArgumentCountRange;
            public Method(Func<Interpreter, InstanceOrBlock, List<Instance>, Task<Instance>> function, IntRange? argumentCountRange) {
                Function = function;
                ArgumentCountRange = argumentCountRange ?? new IntRange();
            }
            public Method(Func<Interpreter, InstanceOrBlock, List<Instance>, Task<Instance>> function, Range argumentCountRange) {
                Function = function;
                ArgumentCountRange = new IntRange(
                    argumentCountRange.Start.Value >= 0 ? argumentCountRange.Start.Value : null,
                    argumentCountRange.End.Value >= 0 ? argumentCountRange.End.Value : null
                );
            }
            public Method(Func<Interpreter, InstanceOrBlock, List<Instance>, Task<Instance>> function, int argumentCount) {
                Function = function;
                ArgumentCountRange = new IntRange(argumentCount, argumentCount);
            }
            public async Task<Instance> Call(Interpreter Interpreter, InstanceOrBlock InstanceOrBlock, List<Instance> Arguments) {
                if (ArgumentCountRange.IsInRange(Arguments.Count)) {
                    // Create temporary scope
                    Scope PreviousScope = Interpreter.CurrentScope;
                    Interpreter.CurrentScope = new Scope(PreviousScope);
                    // Call method
                    Instance ReturnValue = await Function(Interpreter, InstanceOrBlock, Arguments);
                    // Step back a scope
                    Interpreter.CurrentScope = PreviousScope;
                    // Return method return value
                    return ReturnValue;
                }
                else {
                    throw new ScriptErrorException($"Too many or too few arguments given for method (expected {ArgumentCountRange}, got {Arguments.Count})");
                }
            }
            public async Task<Instance> Call(Interpreter Interpreter, InstanceOrBlock InstanceOrBlock, Instance Argument) {
                return await Call(Interpreter, InstanceOrBlock, new List<Instance>() {Argument});
            }
            public async Task<Instance> Call(Interpreter Interpreter, InstanceOrBlock InstanceOrBlock) {
                return await Call(Interpreter, InstanceOrBlock, new List<Instance>());
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
        public class VariableReference : Phase2Object {
            public Block Block;
            public Phase2Token Token;
            public VariableReference(Block block, Phase2Token token) {
                Block = block;
                Token = token;
            }
            public override string Inspect() {
                return $"{Block}::{Token.Inspect()}";
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
                Phase2Object InterpretPath(List<Phase2Token> Path, Block CurrentBlock) {
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
                        throw new ScriptErrorException($"Cannot interpret {Expression.GetType().Name} {Expression.Inspect()} as a variable");
                    }
                }
                async Task<Instance> UseVariable(VariableReference Identifier, ValueExpression IdentifierPath) {
                    // Local variable or method
                    if (Identifier.Token.Type == Phase2TokenType.LocalVariableOrMethod) {
                        // Local variable (priority)
                        if (Identifier.Block is Scope PathScope && PathScope.LocalVariables.TryGetValue(Identifier.Token.Value!, out Instance? Value)) {
                            return Value;
                        }
                        // Method
                        else if (Identifier.Block.FindAncestorWhichIsA<Class>()!.Methods.TryGetValue(Identifier.Token.Value!, out Method? Method)) {
                            return await Method.Call(this, InterpretPath(IdentifierPath.Path, CurrentScope) is VariableReference VarRef ? VarRef.Block : CurrentScope);
                        }
                        // Undefined
                        else {
                            throw new ScriptErrorException($"Undefined local variable or method '{Identifier.Token.Value!}' for {IdentifierPath.Inspect()}");
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
                }
                Instance GetDefinedResult(VariableReference Identifier) {
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
                }
                async Task<Instance> InterpretExpression(Expression Expression) {
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
                                    throw new ScriptErrorException($"Undefined method '{MethodNameToken.Value}' for {MethodCallExpression.MethodName.Inspect()}");
                                }
                            }
                        }*/

                    }
                    // Path
                    else if (Expression is ValueExpression ValueExpression) {
                        Phase2Object PathResult = InterpretPath(ValueExpression.Path, CurrentScope);
                        if (PathResult is VariableReference PathVariable) {
                            return await UseVariable(PathVariable, ValueExpression);
                        }
                        else {
                            return Instance.CreateFromToken(this, (Phase2Token)PathResult);
                        }
                    }
                    /*// Arithmetic
                    else if (Expression is ArithmeticExpression ArithmeticExpression) {
                        Instance Left = await InterpretExpression(ArithmeticExpression.Left);
                        Instance Right = await InterpretExpression(ArithmeticExpression.Right);
                        if (Left == null) {
                            throw new ScriptErrorException($"Cannot call {ArithmeticExpression.Operator} on nil value");
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
                            throw new ScriptErrorException($"Undefined method '{ArithmeticExpression.Operator}' for {Left}");
                        }
                    }*/
                    // Defined?
                    else if (Expression is DefinedExpression DefinedExpression) {
                        try {
                            Phase2Object PathResult = InterpretExpressionAsPath(DefinedExpression.Expression, CurrentScope);
                            if (PathResult is VariableReference PathVariable) {
                                return GetDefinedResult(PathVariable);
                            }
                            else {
                                return new StringInstance(String, "expression");
                            }
                        }
                        catch (EmbersException) {
                            return new StringInstance(String, "expression");
                        }
                    }
                    // Unknown
                    throw new InternalErrorException($"Not sure how to interpret expression {Expression.GetType().Name} ({Expression.Inspect()})");
                }
                async Task<List<Instance>> InterpretExpressions(IEnumerable<Expression> Expressions) {
                    List<Instance> Results = new();
                    foreach (Expression Expression in Expressions) {
                        Results.Add(await InterpretExpression(Expression));
                    }
                    return Results;
                }

                if (Statement is ExpressionStatement ExpressionStatement) {
                    await InterpretExpression(ExpressionStatement.Expression);
                }
                else if (Statement is AssignmentStatement AssignmentStatement) {
                    Instance Right = await InterpretExpression(AssignmentStatement.Right);

                    Phase2Object Left = InterpretExpressionAsPath(AssignmentStatement.Left, CurrentScope);
                    if (Left is VariableReference LeftVariable) {
                        AssignToVariable(LeftVariable, Right);
                    }
                    else {
                        throw new ScriptErrorException($"{Left.GetType()} cannot be the target of an assignment");
                    }
                }
                else if (Statement is DefineMethodStatement DefineMethodStatement) {
                    Phase2Object MethodNameObject = InterpretExpressionAsPath(DefineMethodStatement.MethodName, CurrentClass);
                    if (MethodNameObject is VariableReference MethodName) {

                        if (MethodName.Block == CurrentClass) {
                            CurrentClass.Methods.Add(MethodName.Token.Value!, DefineMethodStatement.Method);
                        }
                        else {
                            throw new SyntaxErrorException("Method name paths not yet supported");
                        }

                        /*Class? MethodNameClass = MethodName.Block.FindAncestorWhichIsA<Class>();

                        if (MethodNameClass != null) {
                            MethodNameClass.Methods.Add(MethodName.Token.Value!, DefineMethodStatement.Method);
                        }
                        else {
                            RootClass.Methods.Add(MethodName.Token.Value!, DefineMethodStatement.Method);
                        }*/

                    }
                    else {
                        throw new InternalErrorException($"Invalid method name: {MethodNameObject.Inspect()}");
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
            RootClass = new Class(null);
            RootScope = new Scope(RootClass);
            CurrentClass = RootClass;
            CurrentScope = RootScope;

            NilClass = new Class(RootClass); RootClass.Classes.Add("NilClass", NilClass); Nil = new NilInstance(NilClass);
            TrueClass = new Class(RootClass); RootClass.Classes.Add("TrueClass", TrueClass); True = new TrueInstance(TrueClass);
            FalseClass = new Class(RootClass); RootClass.Classes.Add("FalseClass", FalseClass); False = new FalseInstance(FalseClass);
            String = new Class(RootClass, new Method(async (Interpreter, Instance, Arguments) => {
                if (Arguments.Count == 1) {
                    ((StringInstance)Instance).SetValue(Arguments[0].String); // Change to implicit conversion
                }
                return Nil;
            }, 0..1)); RootClass.Classes.Add("String", String);
            Integer = new Class(RootClass, null); RootClass.Classes.Add("Integer", Integer);
            Float = new Class(RootClass, null); RootClass.Classes.Add("Float", Float);

            foreach (KeyValuePair<string, Method> Method in Api.GetBuiltInMethods()) {
                RootClass.Methods.Add(Method.Key, Method.Value);
            }
        }
    }
}
