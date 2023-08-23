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
        readonly Dictionary<string, RubyObject> GlobalVariables = new();
        Class CurrentClass;
        Scope CurrentScope;

        public abstract class RubyObject {
            public bool IsA<T>() {
                return GetType() == typeof(T);
            }
            public abstract object? Object { get; }
            public virtual bool Boolean { get { throw new Exception("Ruby object is not a boolean"); } }
            public virtual string String { get { throw new Exception("Ruby object is not a string"); } }
            public virtual long Integer { get { throw new Exception("Ruby object is not an integer"); } }
            public virtual double Float { get { throw new Exception("Ruby object is not a Float"); } }
            public virtual Method Method { get { throw new Exception("Ruby object is not a method"); } }
            public abstract string Inspect();
            public Method? Add = null;
            public Method? Subtract = null;
            public Method? Multiply = null;
            public Method? Divide = null;
            public Method? Modulo = null;
            public Method? Exponentiate = null;
            public static RubyObject CreateFromToken(Phase2Token Token) {
                return Token.Type switch {
                    Phase2TokenType.Nil => Nil,
                    Phase2TokenType.True => True,
                    Phase2TokenType.False => False,
                    Phase2TokenType.String => new RubyString(Token.Value!),
                    Phase2TokenType.Integer => new RubyInteger(long.Parse(Token.Value!)),
                    Phase2TokenType.Float => new RubyFloat(double.Parse(Token.Value!)),
                    _ => throw new InternalErrorException($"Cannot create new object from token type {Token.Type}")
                };
            }
            public static readonly RubyNil Nil = new();
            public static readonly RubyTrue True = new();
            public static readonly RubyFalse False = new();
        }
        /*public class RubyBoolean : RubyObject {
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
        public class RubyNil : RubyObject {
            public override object? Object { get { return null; } }
            public override string Inspect() {
                return "nil";
            }
            public RubyNil() {
            }
        }
        public class RubyTrue : RubyObject {
            public override object? Object { get { return true; } }
            public override bool Boolean { get { return true; } }
            public override string Inspect() {
                return "true";
            }
            public RubyTrue() {
            }
        }
        public class RubyFalse : RubyObject {
            public override object? Object { get { return false; } }
            public override bool Boolean { get { return false; } }
            public override string Inspect() {
                return "false";
            }
            public RubyFalse() {
            }
        }
        public class RubyString : RubyObject {
            readonly string Value;
            public override object? Object { get { return Value; } }
            public override string String { get { return Value; } }
            public override string Inspect() {
                return "\"" + Value + "\"";
            }
            public RubyString(string value) {
                Value = value;

                Add = new Method(async (Interpreter Interpreter, List<RubyObject> Arguments) => {
                    return new RubyString(Value + Arguments[0].String);
                }, 1);
                Multiply = new Method(async (Interpreter Interpreter, List<RubyObject> Arguments) => {
                    StringBuilder JoinedString = new();
                    long DuplicateCount = Arguments[0].Integer;
                    for (long i = 0; i < DuplicateCount; i++) {
                        JoinedString.Append(Value);
                    }
                    return new RubyString(JoinedString.ToString());
                }, 1);
            }
        }
        public class RubyInteger : RubyObject {
            readonly long Value;
            public override object? Object { get { return Value; } }
            public override long Integer { get { return Value; } }
            public override double Float { get { return Value; } }
            public override string Inspect() {
                return Value.ToString();
            }
            public RubyInteger(long value) {
                Value = value;

                static RubyObject GetResult(double Result, bool RightIsInteger) {
                    if (RightIsInteger) {
                        return new RubyInteger((long)Result);
                    }
                    else {
                        return new RubyFloat(Result);
                    }
                }
                Add = new Method(async (Interpreter Interpreter, List<RubyObject> Arguments) => {
                    RubyObject Right = Arguments[0];
                    return GetResult(Value + Right.Float, Right is RubyInteger);
                }, 1);
                Subtract = new Method(async (Interpreter Interpreter, List<RubyObject> Arguments) => {
                    RubyObject Right = Arguments[0];
                    return GetResult(Value - Right.Float, Right is RubyInteger);
                }, 1);
                Multiply = new Method(async (Interpreter Interpreter, List<RubyObject> Arguments) => {
                    RubyObject Right = Arguments[0];
                    return GetResult(Value * Right.Float, Right is RubyInteger);
                }, 1);
                Divide = new Method(async (Interpreter Interpreter, List<RubyObject> Arguments) => {
                    RubyObject Right = Arguments[0];
                    return GetResult(Value / Right.Float, Right is RubyInteger);
                }, 1);
                Modulo = new Method(async (Interpreter Interpreter, List<RubyObject> Arguments) => {
                    RubyObject Right = Arguments[0];
                    return GetResult(Value % Right.Float, Right is RubyInteger);
                }, 1);
                Exponentiate = new Method(async (Interpreter Interpreter, List<RubyObject> Arguments) => {
                    RubyObject Right = Arguments[0];
                    return GetResult(Math.Pow(Value, Right.Float), Right is RubyInteger);
                }, 1);
            }
        }
        public class RubyFloat : RubyObject {
            readonly double Value;
            public override object? Object { get { return Value; } }
            public override double Float { get { return Value; } }
            public override long Integer { get { return (long)Value; } }
            public override string Inspect() {
                return Value.ToString("0.0");
            }
            public RubyFloat(double value) {
                Value = value;

                Add = new Method(async (Interpreter Interpreter, List<RubyObject> Arguments) => {
                    RubyObject Right = Arguments[0];
                    return new RubyFloat(Value + Right.Float);
                }, 1);
                Subtract = new Method(async (Interpreter Interpreter, List<RubyObject> Arguments) => {
                    RubyObject Right = Arguments[0];
                    return new RubyFloat(Value - Right.Float);
                }, 1);
                Multiply = new Method(async (Interpreter Interpreter, List<RubyObject> Arguments) => {
                    RubyObject Right = Arguments[0];
                    return new RubyFloat(Value * Right.Float);
                }, 1);
                Divide = new Method(async (Interpreter Interpreter, List<RubyObject> Arguments) => {
                    RubyObject Right = Arguments[0];
                    return new RubyFloat(Value / Right.Float);
                }, 1);
                Modulo = new Method(async (Interpreter Interpreter, List<RubyObject> Arguments) => {
                    RubyObject Right = Arguments[0];
                    return new RubyFloat(Value % Right.Float);
                }, 1);
                Exponentiate = new Method(async (Interpreter Interpreter, List<RubyObject> Arguments) => {
                    RubyObject Right = Arguments[0];
                    return new RubyFloat(Math.Pow(Value, Right.Float));
                }, 1);
            }
        }
        public class RubyMethod : RubyObject {
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
        
        public class Block {
            public readonly Block? Parent;
            public Block(Block? parent) {
                Parent = parent;
            }
            /*public Block? FindAncestorOfClass<T>() {
                Block? Ancestor = Parent;
                while (Ancestor != null && Ancestor.GetType() != typeof(T)) {
                    Ancestor = Ancestor.Parent;
                }
                return Ancestor;
            }*/
            public T? FindAncestorWhichIsA<T>() where T : Block {
                /*Block? Ancestor = Parent;
                T? AncestorAsT = null;
                while (Ancestor != null && Ancestor is not T AncestorAsT) {
                    Ancestor = Ancestor.Parent;
                }
                return AncestorAsT;*/

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
            public readonly Dictionary<string, RubyObject> LocalVariables = new();
            public Scope(Block? parent) : base(parent) { }
        }
        public class Class : Block {
            public readonly Dictionary<string, Method> Methods = new();
            public readonly Dictionary<string, Class> Classes = new();
            public readonly Dictionary<string, RubyObject> ClassVariables = new();
            public Class(Class? parent) : base(parent) { }
        }
        public class ClassInstance : Class {
            public readonly Dictionary<string, RubyObject> InstanceVariables = new();
            public ClassInstance(ClassInstance? parent) : base(parent) { }
        }
        public class Method {
            public Func<Interpreter, List<RubyObject>, Task<RubyObject>> Function;
            public IntRange ArgumentCountRange;
            public Method(Func<Interpreter, List<RubyObject>, Task<RubyObject>> function, IntRange? argumentCountRange) {
                Function = function;
                ArgumentCountRange = argumentCountRange ?? new IntRange();
            }
            public Method(Func<Interpreter, List<RubyObject>, Task<RubyObject>> function, Range argumentCountRange) {
                Function = function;
                ArgumentCountRange = new IntRange(
                    argumentCountRange.Start.Value >= 0 ? argumentCountRange.Start.Value : null,
                    argumentCountRange.End.Value >= 0 ? argumentCountRange.End.Value : null
                );
            }
            public Method(Func<Interpreter, List<RubyObject>, Task<RubyObject>> function, int argumentCount) {
                Function = function;
                ArgumentCountRange = new IntRange(argumentCount, argumentCount);
            }
            public async Task<RubyObject> Call(Interpreter Interpreter, List<RubyObject> Arguments) {
                if (ArgumentCountRange.IsInRange(Arguments.Count)) {
                    // Create temporary scope
                    Scope PreviousScope = Interpreter.CurrentScope;
                    Interpreter.CurrentScope = new Scope(PreviousScope);
                    // Call method
                    RubyObject ReturnValue = await Function(Interpreter, Arguments);
                    // Step back a scope
                    Interpreter.CurrentScope = PreviousScope;
                    // Return method return value
                    return ReturnValue;
                }
                else {
                    throw new ScriptErrorException($"Too many or too few arguments given for method (expected {ArgumentCountRange}, got {Arguments.Count})");
                }
            }
            public async Task<RubyObject> Call(Interpreter Interpreter, RubyObject Argument) {
                return await Call(Interpreter, new List<RubyObject>() {Argument});
            }
            public async Task<RubyObject> Call(Interpreter Interpreter) {
                return await Call(Interpreter, new List<RubyObject>());
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

        public async Task<RubyObject> InterpretAsync(List<Statement> Statements) {
            for (int Index = 0; Index < Statements.Count; Index++) {
                Statement Statement = Statements[Index];

                void AssignToVariable(VariableReference Variable, RubyObject Value) {
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
                async Task<RubyObject> UseVariable(VariableReference Identifier, ValueExpression DebugOrigin) {
                    // Local variable or method
                    if (Identifier.Token.Type == Phase2TokenType.LocalVariableOrMethod) {
                        // Local variable (priority)
                        if (Identifier.Block is Scope PathScope && PathScope.LocalVariables.TryGetValue(Identifier.Token.Value!, out RubyObject? Value)) {
                            return Value;
                        }
                        // Method
                        else if (Identifier.Block.FindAncestorWhichIsA<Class>()!.Methods.TryGetValue(Identifier.Token.Value!, out Method? Method)) {
                            return await Method.Call(this);
                        }
                        // Undefined
                        else {
                            throw new ScriptErrorException($"Undefined local variable or method '{Identifier.Token.Value!}' for {DebugOrigin.Inspect()}");
                        }
                    }
                    // Global variable
                    else if (Identifier.Token.Type == Phase2TokenType.GlobalVariable) {
                        if (GlobalVariables.TryGetValue(Identifier.Token.Value!, out RubyObject? Value)) {
                            return Value;
                        }
                        else {
                            return RubyObject.Nil;
                        }
                    }
                    // Error
                    else {
                        throw new InternalErrorException($"Using variable type {Identifier.Token.Type.GetType().Name} not implemented");
                    }
                }
                RubyObject GetDefinedResult(VariableReference Identifier) {
                    // Local variable or method
                    if (Identifier.Token.Type == Phase2TokenType.LocalVariableOrMethod) {
                        // Local variable (priority)
                        if (Identifier.Block is Scope PathScope && PathScope.LocalVariables.TryGetValue(Identifier.Token.Value!, out RubyObject? Value)) {
                            return new RubyString("local-variable");
                        }
                        // Method
                        else if (Identifier.Block.FindAncestorWhichIsA<Class>()!.Methods.TryGetValue(Identifier.Token.Value!, out Method? Method)) {
                            return new RubyString("method");
                        }
                        // Undefined
                        else {
                            return RubyObject.Nil;
                        }
                    }
                    // Global variable
                    else if (Identifier.Token.Type == Phase2TokenType.GlobalVariable) {
                        if (GlobalVariables.TryGetValue(Identifier.Token.Value!, out RubyObject? Value)) {
                            return new RubyString("global-variable");
                        }
                        else {
                            return RubyObject.Nil;
                        }
                    }
                    // Constant
                    else if (Identifier.Token.Type == Phase2TokenType.Constant) {
                        // return "constant";
                        return RubyObject.Nil;
                    }
                    // Instance variable
                    else if (Identifier.Token.Type == Phase2TokenType.InstanceVariable) {
                        // return "instance-variable";
                        return RubyObject.Nil;
                    }
                    // Class variable
                    else if (Identifier.Token.Type == Phase2TokenType.ClassVariable) {
                        if (CurrentClass.ClassVariables.TryGetValue(Identifier.Token.Value!, out RubyObject? Value)) {
                            return new RubyString("class-variable");
                        }
                        else {
                            return RubyObject.Nil;
                        }
                    }
                    // Other
                    else {
                        return new RubyString("expression");
                    }
                }
                async Task<RubyObject> InterpretExpression(Expression Expression, bool WantsVariable = false) {
                    // Method call
                    if (Expression is MethodCallExpression MethodCallExpression) {
                        // Global method
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
                        }
                    }
                    // Path
                    else if (Expression is ValueExpression ValueExpression) {
                        Phase2Object PathResult = InterpretPath(ValueExpression.Path, CurrentScope);
                        if (PathResult is VariableReference PathVariable) {
                            return await UseVariable(PathVariable, ValueExpression);
                        }
                        else {
                            return RubyObject.CreateFromToken((Phase2Token)PathResult);
                        }
                    }
                    // Arithmetic
                    else if (Expression is ArithmeticExpression ArithmeticExpression) {
                        RubyObject Left = await InterpretExpression(ArithmeticExpression.Left);
                        RubyObject Right = await InterpretExpression(ArithmeticExpression.Right);
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
                    }
                    // Defined?
                    else if (Expression is DefinedExpression DefinedExpression) {
                        try {
                            Phase2Object PathResult = InterpretExpressionAsPath(DefinedExpression.Expression, CurrentScope);
                            if (PathResult is VariableReference PathVariable) {
                                return GetDefinedResult(PathVariable);
                            }
                            else {
                                return new RubyString("expression");
                            }
                        }
                        catch (EmbersException) {
                            return new RubyString("expression");
                        }
                    }
                    // Unknown
                    throw new InternalErrorException($"Not sure how to interpret expression {Expression.GetType().Name} ({Expression.Inspect()})");
                }
                async Task<List<RubyObject>> InterpretExpressions(IEnumerable<Expression> Expressions) {
                    List<RubyObject> Results = new();
                    foreach (Expression Expression in Expressions) {
                        Results.Add(await InterpretExpression(Expression));
                    }
                    return Results;
                }

                if (Statement is ExpressionStatement ExpressionStatement) {
                    await InterpretExpression(ExpressionStatement.Expression);
                }
                else if (Statement is AssignmentStatement AssignmentStatement) {
                    RubyObject Right = await InterpretExpression(AssignmentStatement.Right);

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
            return RubyObject.Nil;
        }
        public RubyObject Interpret(List<Statement> Statements) {
            return InterpretAsync(Statements).Result;
        }
        public async Task<RubyObject> EvaluateAsync(string Code) {
            List<Phase1.Phase1Token> Tokens = Phase1.GetPhase1Tokens(Code);
            List<Statement> Statements = GetStatements(Tokens);
            return await InterpretAsync(Statements);
        }
        public RubyObject Evaluate(string Code) {
            return EvaluateAsync(Code).Result;
        }

        public Interpreter() {
            RootClass = new Class(null);
            RootScope = new Scope(RootClass);
            CurrentClass = RootClass;
            CurrentScope = RootScope;

            foreach (KeyValuePair<string, Method> Method in Api.GetBuiltInMethods()) {
                RootClass.Methods.Add(Method.Key, Method.Value);
            }
        }
    }
}
