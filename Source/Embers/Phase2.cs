using System;
using System.Collections.Generic;
using System.Linq;
using static Embers.Phase1;
using static Embers.Script;
using static Embers.SpecialTypes;

#nullable enable

namespace Embers
{
    public static class Phase2
    {
        public abstract class Phase2Object {
            public readonly DebugLocation Location;
            public Phase2Object(DebugLocation location) {
                Location = location;
            }
            public abstract string Inspect();
            public abstract string Serialise();
            public string PathToSelf => (GetType().FullName ?? "").Replace('+', '.');
        }

        public enum Phase2TokenType {
            LocalVariableOrMethod,
            GlobalVariable,
            ConstantOrMethod,
            InstanceVariable,
            ClassVariable,
            Symbol,

            Integer,
            Float,
            String,

            AssignmentOperator,
            Operator,

            // Keywords
            Alias, Begin, Break, Case, Class, Def, Defined, Do, Else, Elsif, End, Ensure, False, For, If, In, Module, Next, Nil, Redo, Rescue, Retry, Return, Self, Super, Then, True, Undef, Unless, Until, When, While, Yield,
            __LINE__,

            // Temporary
            Dot,
            DoubleColon,
            Comma,
            SplatOperator,
            OpenBracket,
            CloseBracket,
            StartCurly,
            EndCurly,
            StartSquare,
            EndSquare,
            Pipe,
            RightArrow,
            InclusiveRange,
            ExclusiveRange,
            TernaryQuestion,
            TernaryElse,
            EndOfStatement,
        }
        public readonly static Dictionary<string, Phase2TokenType> Keywords = new() {
            {"alias", Phase2TokenType.Alias},
            {"begin", Phase2TokenType.Begin},
            {"break", Phase2TokenType.Break},
            {"case", Phase2TokenType.Case},
            {"class", Phase2TokenType.Class},
            {"def", Phase2TokenType.Def},
            {"defined?", Phase2TokenType.Defined},
            {"do", Phase2TokenType.Do},
            {"else", Phase2TokenType.Else},
            {"elsif", Phase2TokenType.Elsif},
            {"end", Phase2TokenType.End},
            {"ensure", Phase2TokenType.Ensure},
            {"false", Phase2TokenType.False},
            {"for", Phase2TokenType.For},
            {"if", Phase2TokenType.If},
            {"in", Phase2TokenType.In},
            {"module", Phase2TokenType.Module},
            {"next", Phase2TokenType.Next},
            {"nil", Phase2TokenType.Nil},
            {"redo", Phase2TokenType.Redo},
            {"rescue", Phase2TokenType.Rescue},
            {"retry", Phase2TokenType.Retry},
            {"return", Phase2TokenType.Return},
            {"self", Phase2TokenType.Self},
            {"super", Phase2TokenType.Super},
            {"then", Phase2TokenType.Then},
            {"true", Phase2TokenType.True},
            {"undef", Phase2TokenType.Undef},
            {"unless", Phase2TokenType.Unless},
            {"until", Phase2TokenType.Until},
            {"when", Phase2TokenType.When},
            {"while", Phase2TokenType.While},
            {"yield", Phase2TokenType.Yield},
            {"__LINE__", Phase2TokenType.__LINE__},
        };
        public readonly static string[][] NormalOperatorPrecedence = new[] {
            new[] {"**"},
            new[] {"!"},
            new[] {"*", "/", "%"},
            new[] {"+", "-"},

            new[] {"<<", ">>"},
            new[] {"&"},
            new[] {"|", "^"},
            new[] {">", ">=", "<", "<="},
            new[] {"<=>", "==", "===", "!=", "=~", "!~"},
            new[] {"&&"},
            new[] {"||"},
        };
        public readonly static string[][] LowPriorityOperatorPrecedence = new[] {
            new[] {"not"},
            new[] {"or", "and"},
        };
        public readonly static string[] NonMethodOperators = new[] {
            "or", "and", "&&", "||", "!", "not"
        };
        public readonly static Phase2TokenType[] StartParenthesesTokens = new[] {
            Phase2TokenType.OpenBracket, Phase2TokenType.StartSquare, Phase2TokenType.StartCurly, Phase2TokenType.Pipe
        };
        public readonly static Phase2TokenType[] EndParenthesesTokens = new[] {
            Phase2TokenType.CloseBracket, Phase2TokenType.EndSquare, Phase2TokenType.EndCurly, Phase2TokenType.Pipe
        };

        public class Phase2Token : Phase2Object {
            public readonly Phase2TokenType Type;
            public string? Value;
            public readonly bool FollowsWhitespace;
            public readonly bool FollowedByWhitespace;
            public readonly bool ProcessFormatting;

            public readonly bool IsObjectToken;
            public readonly DynInteger ValueAsInteger;
            public readonly DynFloat ValueAsFloat;

            private readonly Phase1Token? FromPhase1Token;

            public Phase2Token(DebugLocation location, Phase2TokenType type, string? value, Phase1Token? fromPhase1Token = null) : base(location) {
                Type = type;
                Value = value;
                if (fromPhase1Token != null) {
                    FromPhase1Token = fromPhase1Token;
                    FollowsWhitespace = fromPhase1Token.FollowsWhitespace;
                    FollowedByWhitespace = fromPhase1Token.FollowedByWhitespace;
                    ProcessFormatting = fromPhase1Token.ProcessFormatting;
                }

                IsObjectToken = IsObjectToken(this);
                if (Type == Phase2TokenType.Integer) ValueAsInteger = Value!.ParseInteger();
                if (Type == Phase2TokenType.Float) ValueAsFloat = Value!.ParseFloat();
            }
            private string? OneLineValue => Value?.Replace("\n", "\\n").Replace("\r", "\\r");
            public override string Inspect() {
                return Type + (Value != null ? ":" : "") + OneLineValue;
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Type.GetPath()}, {OneLineValue.Serialise()}, {(FromPhase1Token != null ? FromPhase1Token.Serialise() : "null")})";
            }
        }

        // Expressions
        public abstract class Expression : Phase2Object {
            public Expression(DebugLocation location) : base(location) { }
        }
        public class ObjectTokenExpression : Expression {
            public readonly Phase2Token Token;
            public ObjectTokenExpression(Phase2Token objectToken) : base(objectToken.Location) {
                Token = objectToken;
            }
            public override string Inspect() {
                return Token.Inspect();
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Token.Serialise()})";
            }
        }
        public class PathExpression : ObjectTokenExpression {
            public readonly Expression ParentObject;
            public PathExpression(Expression parentObject, Phase2Token objectToken) : base(objectToken) {
                ParentObject = parentObject;
            }
            public override string Inspect() {
                return ParentObject.Inspect() + "." + Token.Inspect();
            }
            public override string Serialise() {
                return $"new {PathToSelf}({ParentObject.Serialise()}, {Token.Serialise()})";
            }
        }
        public class ConstantPathExpression : ObjectTokenExpression {
            public readonly Expression ParentObject;
            public ConstantPathExpression(Expression parentObject, Phase2Token objectToken) : base(objectToken) {
                ParentObject = parentObject;
            }
            public override string Inspect() {
                return ParentObject.Inspect() + "." + ParentObject.Inspect();
            }
            public override string Serialise() {
                return $"new {PathToSelf}({ParentObject.Serialise()}, {Token.Serialise()})";
            }
        }
        public class MethodCallExpression : Expression {
            public Expression MethodPath;
            public readonly List<Expression> Arguments;
            public MethodExpression? OnYield; // do ... end
            public MethodCallExpression(Expression methodPath, List<Expression>? arguments, MethodExpression? onYield = null) : base(methodPath.Location) {
                MethodPath = methodPath;
                Arguments = arguments ?? new List<Expression>();
                OnYield = onYield;
            }
            public override string Inspect() {
                return $"{MethodPath.Inspect()}({Arguments.Inspect()})" + (OnYield != null ? $"{{yield: {OnYield.Inspect()}}}" : "");
            }
            public override string Serialise() {
                return $"new {PathToSelf}({MethodPath.Serialise()}, {Arguments.Serialise()}, {(OnYield != null ? OnYield.Serialise() : "null")})";
            }
        }
        public class RangeExpression : Expression {
            public readonly Expression? Min;
            public readonly Expression? Max;
            public readonly bool IncludesMax;
            public RangeExpression(DebugLocation location, Expression? min, Expression? max, bool includesMax) : base(location) {
                Min = min;
                Max = max;
                IncludesMax = includesMax;
            }
            public override string Inspect() {
                return $"{(Min != null ? Min.Inspect() : "")}{(IncludesMax ? ".." : "...")}{(Max != null ? Max.Inspect() : "")}";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({(Min != null ? Min.Serialise() : "null")}, {(Max != null ? Max.Serialise() : "null")}, {(IncludesMax ? "true" : "false")})";
            }
        }
        public class DefinedExpression : Expression {
            public readonly Expression Expression;
            public DefinedExpression(DebugLocation location, Expression expression) : base(location) {
                Expression = expression;
            }
            public override string Inspect() {
                return "defined? (" + Expression.Inspect() + ")";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Expression.Serialise()})";
            }
        }
        public enum EnvironmentInfoType {
            __LINE__,
        }
        public class EnvironmentInfoExpression : Expression {
            public readonly EnvironmentInfoType Type;
            public EnvironmentInfoExpression(DebugLocation location, EnvironmentInfoType type) : base(location) {
                Type = type;
            }
            public override string Inspect() {
                return Type.ToString();
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location}, {Type.GetPath()})";
            }
        }
        public class MethodArgumentExpression : Expression {
            public readonly Phase2Token ArgumentName;
            public Expression? DefaultValue;
            public SplatType? SplatType;
            public MethodArgumentExpression(Phase2Token argumentName, Expression? defaultValue = null, SplatType? splatType = null) : base(argumentName.Location) {
                ArgumentName = argumentName;
                DefaultValue = defaultValue;
                SplatType = splatType;
                if (ArgumentName.Type != Phase2TokenType.LocalVariableOrMethod) {
                    throw new SyntaxErrorException($"{ArgumentName.Location}: Argument name ('{ArgumentName.Value}') must not be {ArgumentName.Type}");
                }
            }
            public override string Inspect() {
                if (DefaultValue == null) {
                    return ArgumentName.Inspect();
                }
                else {
                    return $"{ArgumentName.Inspect()} = {DefaultValue.Inspect()}";
                }
            }
            public override string Serialise() {
                return $"new {PathToSelf}({ArgumentName.Serialise()}, {(DefaultValue != null ? DefaultValue.Serialise() : "null")}, {(SplatType != null ? SplatType.GetPath() : "null")})";
            }
        }
        public class MethodExpression : Expression {
            public readonly List<Expression> Statements;
            public readonly IntRange ArgumentCount;
            public readonly List<MethodArgumentExpression> Arguments;
            public readonly string? Name;

            public MethodExpression(DebugLocation location, List<Expression> statements, IntRange? argumentCount, List<MethodArgumentExpression> arguments, string? name) : base(location) {
                Statements = statements;
                ArgumentCount = argumentCount ?? new IntRange();
                Arguments = arguments;
                Name = name;
            }
            public override string Inspect() {
                return $"|{Arguments.Inspect()}| {Statements.Inspect()} end";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Statements.Serialise()}, {ArgumentCount.Serialise()}, {Arguments.Serialise()}, {Name.Serialise()})";
            }
            public Method ToMethod(AccessModifier AccessModifier, Module? Parent) {
                Method Method = new(async Input => {
                    return await Input.Script.InternalInterpretAsync(Statements, Input.OnYield);
                }, ArgumentCount, Arguments, accessModifier: AccessModifier, parent: Parent);
                Method.SetName(Name);
                return Method;
            }
            public Method? ToYieldMethod(Script Script, Method? OnYield) {
                Method? Method = Script.ToYieldMethod(new Method(async Input => {
                    return await Input.Script.InternalInterpretAsync(Statements, OnYield);
                }, ArgumentCount, Arguments, accessModifier: AccessModifier.Public));
                Method?.SetName(Name);
                return Method;
            }
        }
        public class SelfExpression : Expression {
            public SelfExpression(DebugLocation location) : base(location) { }
            public override string Inspect() {
                return "self";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()})";
            }
        }
        public class LogicalExpression : Expression {
            public readonly LogicalExpressionType LogicType;
            public readonly Expression Left;
            public readonly Expression Right;
            public LogicalExpression(DebugLocation location, LogicalExpressionType logicType, Expression left, Expression right) : base(location) {
                LogicType = logicType;
                Left = left;
                Right = right;
            }
            public override string Inspect() {
                return $"({Left.Inspect()} {LogicType} {Right.Inspect()})";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Left.Serialise()}, {Right.Serialise()})";
            }
            public enum LogicalExpressionType {
                And,
                Or,
                Xor
            }
        }
        public class NotExpression : Expression {
            public readonly Expression Right;
            public NotExpression(DebugLocation location, Expression right) : base(location) {
                Right = right;
            }
            public override string Inspect() {
                return $"(not {Right.Inspect()})";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Right.Serialise()})";
            }
        }
        public abstract class ConditionalExpression : Expression {
            public readonly bool Inverse;
            public readonly Expression? Condition;
            public readonly List<Expression> Statements;
            public ConditionalExpression(DebugLocation location, Expression? condition, List<Expression> statements, bool inverse = false) : base(location) {
                Inverse = inverse;
                Condition = condition;
                Statements = statements;
            }
        }
        public class IfExpression : ConditionalExpression {
            public IfExpression(DebugLocation location, Expression? condition, List<Expression> statements, bool inverse = false) : base(location, condition, statements, inverse) { }
            public override string Inspect() {
                if (Condition != null) {
                    if (!Inverse) {
                        return $"if {Condition.Inspect()} then {Statements.Inspect()} end";
                    }
                    else {
                        return $"unless {Condition.Inspect()} then {Statements.Inspect()} end";
                    }
                }
                else {
                    return $"else {Statements.Inspect()} end";
                }
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {(Condition != null ? Condition.Serialise() : "null")}, {Statements.Serialise()}, {(Inverse ? "true" : "false")})";
            }
        }
        public class WhileExpression : ConditionalExpression {
            public WhileExpression(DebugLocation location, Expression condition, List<Expression> statements, bool inverse = false) : base(location, condition, statements, inverse) { }
            public override string Inspect() {
                if (Inverse) {
                    return $"until {Condition!.Inspect()} {{" + Statements.Inspect() + "}";
                }
                else {
                    return $"while {Condition!.Inspect()} {{" + Statements.Inspect() + "}";
                }
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Condition!.Serialise()}, {Statements.Serialise()}, {(Inverse ? "true" : "false")})";
            }
        }
        public class WhenExpression : Expression {
            public readonly List<Expression> Conditions;
            public readonly List<Expression> Statements;
            public WhenExpression(DebugLocation location, List<Expression> conditions, List<Expression> statements) : base(location) {
                Conditions = conditions;
                Statements = statements;
            }
            public override string Inspect() {
                if (Conditions.Count != 0) {
                    return $"when {Conditions.Inspect()} then {Statements.Inspect()} end";
                }
                else {
                    return $"else {Statements.Inspect()} end";
                }
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Conditions.Serialise()}, {Statements.Serialise()})";
            }
        }
        public class RescueExpression : Expression {
            public readonly Expression Statement;
            public readonly Expression RescueStatement;
            public RescueExpression(Expression statement, Expression rescueStatement) : base(statement.Location) {
                Statement = statement;
                RescueStatement = rescueStatement;
            }
            public override string Inspect() {
                return $"{Statement.Inspect()} rescue {RescueStatement.Inspect()}";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Statement.Serialise()}, {RescueStatement.Serialise()})";
            }
        }
        public class TernaryExpression : Expression {
            public readonly Expression Condition;
            public readonly Expression ExpressionIfTrue;
            public readonly Expression ExpressionIfFalse;
            public TernaryExpression(DebugLocation location, Expression condition, Expression expressionIfTrue, Expression expressionIfFalse) : base(location) {
                Condition = condition;
                ExpressionIfTrue = expressionIfTrue;
                ExpressionIfFalse = expressionIfFalse;
            }
            public override string Inspect() {
                return $"{Condition.Inspect()} ? {ExpressionIfTrue.Inspect()} : {ExpressionIfFalse.Inspect()}";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Condition.Serialise()}, {ExpressionIfTrue.Serialise()}, {ExpressionIfFalse.Serialise()})";
            }
        }
        public class CaseExpression : Expression {
            public readonly Expression Subject;
            public readonly List<WhenExpression> Branches;
            public CaseExpression(DebugLocation location, Expression subject, List<WhenExpression> branches) : base(location) {
                Subject = subject;
                Branches = branches;
            }
            public override string Inspect() {
                return $"case {Subject.Inspect()}; {Branches.Inspect("; ")}; end";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Subject.Serialise()}, {Branches.Serialise()})";
            }
        }
        public class AssignmentExpression : Expression {
            public ObjectTokenExpression Left;
            public Expression Right;

            readonly string Operator;
            readonly Expression OriginalRight;

            public AssignmentExpression(ObjectTokenExpression left, string op, Expression right) : base(left.Location) {
                Left = left;
                Operator = op;
                Right = right;
                OriginalRight = right;

                // Compound assignment operators
                if (Operator != "=") {
                    if (Operator.Length >= 2) {
                        string ArithmeticOperator = Operator[..^1];
                        Right = new MethodCallExpression(
                            new PathExpression(Left, new Phase2Token(Left.Location, Phase2TokenType.Operator, ArithmeticOperator)),
                            new List<Expression>() {Right}
                        );
                    }
                    else {
                        throw new InternalErrorException($"Unknown assigment operator: '{Operator}'");
                    }
                }
            }
            public override string Inspect() {
                return Left.Inspect() + " " + Operator + " " + Right.Inspect();
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Left.Serialise()}, {Operator.Serialise()}, {OriginalRight.Serialise()})";
            }
        }
        public class MultipleAssignmentExpression : Expression {
            public List<ObjectTokenExpression> Left;
            public List<Expression> Right;

            readonly string Operator;
            readonly List<Expression> OriginalRight;

            public MultipleAssignmentExpression(List<ObjectTokenExpression> left, string op, List<Expression> right) : base(left[0].Location) {
                Left = left;
                Operator = op;
                Right = right;
                OriginalRight = right;

                // Compound assignment operators
                if (Operator != "=") {
                    if (Operator.Length >= 2) {
                        if (Left.Count != Right.Count)
                            throw new SyntaxErrorException($"{Left.Location()}: Compound assignment not valid when assigning to multiple variables");
                        string ArithmeticOperator = Operator[..^1];
                        for (int i = 0; i < Right.Count; i++) {
                            Right[i] = new MethodCallExpression(
                                new PathExpression(Left[i], new Phase2Token(Left[i].Location, Phase2TokenType.Operator, ArithmeticOperator)),
                                new List<Expression>() {Right[i]}
                            );
                        }
                    }
                    else {
                        throw new InternalErrorException($"Unknown assigment operator: '{Operator}'");
                    }
                }
            }
            public override string Inspect() {
                return Left.Inspect() + " " + Operator + " " + Right.Inspect();
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Left.Serialise()}, {Operator.Serialise()}, {OriginalRight.Serialise()})";
            }
        }
        public class DoExpression : Expression {
            public readonly MethodExpression OnYield;
            public readonly bool HighPriority;
            public DoExpression(MethodExpression onYield, bool highPriority) : base(onYield.Location) {
                OnYield = onYield;
                HighPriority = highPriority;
            }
            public override string Inspect() {
                return $"do {OnYield.Inspect()} end";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({OnYield.Serialise()}, {(HighPriority ? "true" : "false")})";
            }
        }
        public class ArrayExpression : Expression {
            public readonly List<Expression> Expressions;
            public ArrayExpression(DebugLocation location, List<Expression> expressions) : base(location) {
                Expressions = expressions;
            }
            public override string Inspect() {
                return $"[{Expressions.Inspect()}]";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Expressions.Serialise()})";
            }
        }
        public class HashExpression : Expression {
            public readonly LockingDictionary<Expression, Expression> Expressions;
            public HashExpression(DebugLocation location, LockingDictionary<Expression, Expression> expressions) : base(location) {
                Expressions = expressions;
            }
            public override string Inspect() {
                return $"{{{Expressions.Inspect()}}}";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Expressions.Serialise()})";
            }
        }
        public class HashArgumentsExpression : Expression {
            public readonly HashExpression HashExpression;
            public HashArgumentsExpression(DebugLocation location, LockingDictionary<Expression, Expression> expressions) : base(location) {
                HashExpression = new(location, expressions);
            }
            public override string Inspect() {
                return HashExpression.Expressions.Inspect();
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {HashExpression.Serialise()})";
            }
        }
        public abstract class Statement : Expression {
            public Statement(DebugLocation location) : base(location) { }
        }
        public class DefineMethodStatement : Statement {
            public readonly ObjectTokenExpression MethodName;
            public readonly MethodExpression MethodExpression;
            public DefineMethodStatement(ObjectTokenExpression methodName, MethodExpression methodExpression) : base(methodName.Location) {
                MethodName = methodName;
                MethodExpression = methodExpression;
            }
            public override string Inspect() {
                return $"def {MethodName.Inspect()}({MethodExpression.Inspect()})";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({MethodName.Serialise()}, {MethodExpression.Serialise()})";
            }
        }
        public class UndefineMethodStatement : Statement {
            public readonly ObjectTokenExpression MethodName;
            public UndefineMethodStatement(DebugLocation location, ObjectTokenExpression methodName) : base(location) {
                MethodName = methodName;
            }
            public override string Inspect() {
                return "undef " + MethodName.Inspect();
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {MethodName.Serialise()})";
            }
        }
        public class DefineClassStatement : Statement {
            public readonly ObjectTokenExpression ClassName;
            public readonly List<Expression> BlockStatements;
            public readonly bool IsModule;
            public readonly ObjectTokenExpression? InheritsFrom;
            public DefineClassStatement(ObjectTokenExpression className, List<Expression> blockStatements, bool isModule, ObjectTokenExpression? inheritsFrom) : base(className.Location) {
                ClassName = className;
                BlockStatements = blockStatements;
                IsModule = isModule;
                InheritsFrom = inheritsFrom;
            }
            public override string Inspect() {
                return "class " + ClassName.Inspect();
            }
            public override string Serialise() {
                return $"new {PathToSelf}({ClassName.Serialise()}, {BlockStatements.Serialise()}, {(IsModule ? "true" : "false")}, {(InheritsFrom != null ? InheritsFrom.Serialise() : "null")})";
            }
        }
        public class YieldStatement : Statement {
            public readonly List<Expression>? YieldValues;
            public YieldStatement(DebugLocation location, List<Expression>? yieldValues = null) : base(location) {
                YieldValues = yieldValues;
            }
            public override string Inspect() {
                if (YieldValues != null) return "yield " + YieldValues.Inspect();
                else return "yield";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {(YieldValues != null ? YieldValues.Serialise() : "null")})";
            }
        }
        public class ReturnStatement : Statement {
            public readonly Expression? ReturnValue;
            public ReturnStatement(DebugLocation location, Expression? returnValue = null) : base(location) {
                ReturnValue = returnValue;
            }
            public override string Inspect() {
                if (ReturnValue != null) return "return " + ReturnValue.Inspect();
                else return "return";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {(ReturnValue != null ? ReturnValue.Serialise() : "null")})";
            }
        }
        public class SuperStatement : Statement {
            public readonly List<Expression>? Arguments;
            public SuperStatement(DebugLocation location, List<Expression>? arguments = null) : base(location) {
                Arguments = arguments;
            }
            public override string Inspect() {
                if (Arguments != null) return "super " + Arguments.Inspect();
                else return "super";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {(Arguments != null ? Arguments.Serialise() : "null")})";
            }
        }
        public class AliasStatement : Statement {
            public readonly ObjectTokenExpression AliasAs;
            public readonly ObjectTokenExpression MethodToAlias;
            public AliasStatement(DebugLocation location, ObjectTokenExpression aliasAs, ObjectTokenExpression methodToAlias) : base(location) {
                AliasAs = aliasAs;
                MethodToAlias = methodToAlias;
            }
            public override string Inspect() {
                return $"alias {AliasAs.Inspect()} {MethodToAlias.Inspect()}";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {AliasAs.Serialise()}, {MethodToAlias.Serialise()})";
            }
        }
        public enum LoopControlType {
            Break,
            Retry,
            Redo,
            Next
        }
        public class LoopControlStatement : Statement {
            public LoopControlType Type;
            public LoopControlStatement(DebugLocation location, LoopControlType type) : base(location) {
                Type = type;
            }
            public override string Inspect() {
                return Type.ToString().ToLower();
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Type.GetPath()}))";
            }
        }
        public abstract class BeginComponentStatement : Statement {
            public readonly List<Expression> Statements;
            public BeginComponentStatement(DebugLocation location, List<Expression> statements) : base(location) {
                Statements = statements;
            }
        }
        public class BeginStatement : BeginComponentStatement {
            public BeginStatement(DebugLocation location, List<Expression> statements) : base(location, statements) { }
            public override string Inspect() {
                return $"begin {Statements.Inspect()} end";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Statements.Serialise()})";
            }
        }
        public class RescueStatement : BeginComponentStatement {
            public readonly ObjectTokenExpression? Exception;
            public readonly Phase2Token? ExceptionVariable;
            public RescueStatement(DebugLocation location, List<Expression> statements, ObjectTokenExpression? exception, Phase2Token? exceptionVariable) : base(location, statements) {
                Exception = exception;
                ExceptionVariable = exceptionVariable;
            }
            public override string Inspect() {
                if (Exception != null && ExceptionVariable != null) {
                    return $"rescue {Exception.Inspect()} => {ExceptionVariable.Inspect()}; {Statements.Inspect()}";
                }
                else if (Exception != null) {
                    return $"rescue {Exception.Inspect()}; {Statements.Inspect()}";
                }
                else if (ExceptionVariable != null) {
                    return $"rescue => {ExceptionVariable.Inspect()}; {Statements.Inspect()}";
                }
                else {
                    return $"rescue; {Statements.Inspect()}";
                }
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Statements.Serialise()}, {(Exception != null ? Exception.Serialise() : "null")}, {(ExceptionVariable != null ? ExceptionVariable.Serialise() : "null")})";
            }
        }
        public class RescueElseStatement : BeginComponentStatement {
            public RescueElseStatement(DebugLocation location, List<Expression> statements) : base(location, statements) { }
            public override string Inspect() {
                return $"else; {Statements.Inspect()}";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Statements.Serialise()})";
            }
        }
        public class EnsureStatement : BeginComponentStatement {
            public EnsureStatement(DebugLocation location, List<Expression> statements) : base(location, statements) { }
            public override string Inspect() {
                return $"ensure; {Statements.Inspect()}";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Statements.Serialise()})";
            }
        }
        public class IfBranchesStatement : Statement {
            public readonly List<IfExpression> Branches;
            public IfBranchesStatement(DebugLocation location, List<IfExpression> branches) : base(location) {
                Branches = branches;
            }
            public override string Inspect() {
                return Branches.Inspect(" ");
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Branches.Serialise()})";
            }
        }
        public class BeginBranchesStatement : Statement {
            public readonly List<BeginComponentStatement> Branches;
            public BeginBranchesStatement(DebugLocation location, List<BeginComponentStatement> branches) : base(location) {
                Branches = branches;
            }
            public override string Inspect() {
                return Branches.Inspect(" ");
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {Branches.Serialise()})";
            }
        }
        public class WhileStatement : Statement {
            public readonly WhileExpression WhileExpression;
            public WhileStatement(WhileExpression whileExpression) : base(whileExpression.Location) {
                WhileExpression = whileExpression;
            }
            public override string Inspect() {
                return $"while {WhileExpression.Condition!.Inspect()} do {WhileExpression.Statements.Inspect()} end";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({WhileExpression.Serialise()})";
            }
        }
        public class ForStatement : Statement {
            public readonly List<MethodArgumentExpression> VariableNames;
            public readonly Expression InExpression;
            public readonly List<Expression> BlockStatements;

            public readonly Method BlockStatementsMethod;

            public ForStatement(DebugLocation location, List<MethodArgumentExpression> variableNames, Expression inExpression, List<Expression> blockStatements) : base(location) {
                VariableNames = variableNames;
                InExpression = inExpression;
                BlockStatements = blockStatements;

                BlockStatementsMethod = ToMethod();
            }
            public override string Inspect() {
                return $"for {VariableNames.Inspect()} in {InExpression.Inspect()} do {BlockStatements.Inspect()} end";
            }
            public override string Serialise() {
                return $"new {PathToSelf}({Location.Serialise()}, {VariableNames.Serialise()}, {InExpression.Serialise()}, {BlockStatements.Serialise()})";
            }
            Method ToMethod() {
                return new Method(async Input => {
                    Redo:
                    try {
                        return await Input.Script.InternalInterpretAsync(BlockStatements);
                    }
                    catch (RedoException) {
                        goto Redo;
                    }
                    catch (NextException) {
                        return Input.Api.Nil;
                    }
                }, null, VariableNames);
            }
        }
        
        public enum SplatType {
            Single,
            Double
        }
        public enum ExpressionsType {
            SingleExpression,
            Statements,
            CommaSeparatedExpressions,
            KeyValueExpressions,
        }
        enum BlockPriority {
            First,
            Second,
        }

        static Phase2Token IdentifierToPhase2(Phase1Token Token, bool FollowsDot) {
            if (Token.Type != Phase1TokenType.Identifier)
                throw new InternalErrorException($"{Token.Location}: Cannot convert identifier to phase 2 for token that is not an identifier");

            if (!FollowsDot) {
                foreach (KeyValuePair<string, Phase2TokenType> Keyword in Keywords) {
                    if (Token.Value == Keyword.Key) {
                        return new Phase2Token(Token.Location, Keyword.Value, null, Token);
                    }
                }
            }

            Phase2TokenType IdentifierType;
            string Identifier;

            if (Token.NonNullValue[0] == ':') {
                IdentifierType = Phase2TokenType.Symbol;
                if (Token.NonNullValue.Length == 0) throw new SyntaxErrorException($"{Token.Location}: Identifier ':' not valid for symbol");
                Identifier = Token.NonNullValue[1..];
            }
            else if (Token.NonNullValue[0] == '$') {
                IdentifierType = Phase2TokenType.GlobalVariable;
                if (Token.NonNullValue.Length == 0) throw new SyntaxErrorException($"{Token.Location}: Identifier '$' not valid for global variable");
                Identifier = Token.NonNullValue[1..];
            }
            else if (Token.NonNullValue.StartsWith("@@")) {
                IdentifierType = Phase2TokenType.ClassVariable;
                if (Token.NonNullValue.Length <= 1) throw new SyntaxErrorException($"{Token.Location}: Identifier '@@' not valid for class variable");
                Identifier = Token.NonNullValue[2..];
            }
            else if (Token.NonNullValue[0] == '@') {
                IdentifierType = Phase2TokenType.InstanceVariable;
                if (Token.NonNullValue.Length == 0) throw new SyntaxErrorException($"{Token.Location}: Identifier '@' not valid for instance variable");
                Identifier = Token.NonNullValue[1..];
            }
            else if (Token.NonNullValue[0].IsAsciiLetterUpper()) {
                IdentifierType = Phase2TokenType.ConstantOrMethod;
                Identifier = Token.NonNullValue;
            }
            else {
                IdentifierType = Phase2TokenType.LocalVariableOrMethod;
                Identifier = Token.NonNullValue;
            }

            if (Identifier.Contains('$') || Identifier.Contains('@')) {
                throw new Exception($"{Token.Location}: Identifier cannot contain $ or @");
            }

            return new Phase2Token(Token.Location, IdentifierType, Identifier, Token);
        }
        public static bool IsObjectToken(Phase2TokenType? Type) {
            return Type is Phase2TokenType.Nil
                or Phase2TokenType.True
                or Phase2TokenType.False
                or Phase2TokenType.String
                or Phase2TokenType.Symbol
                or Phase2TokenType.Integer
                or Phase2TokenType.Float;
        }
        public static bool IsObjectToken(Phase2Token? Token) {
            return Token != null && IsObjectToken(Token.Type);
        }
        public static bool IsVariableToken(Phase2TokenType? Type) {
            return Type is Phase2TokenType.LocalVariableOrMethod
                or Phase2TokenType.GlobalVariable
                or Phase2TokenType.ConstantOrMethod
                or Phase2TokenType.InstanceVariable
                or Phase2TokenType.ClassVariable
                or Phase2TokenType.Symbol
                or Phase2TokenType.Self;
        }
        public static bool IsVariableToken(Phase2Token? Token) {
            return Token != null && IsVariableToken(Token.Type);
        }

        static List<Phase2Token> TokensToPhase2(List<Phase1Token> Tokens) {
            // Phase 1 tokens to phase 2 tokens
            List<Phase2Token> NewTokens = new();
            for (int i = 0; i < Tokens.Count; i++) {
                Phase1Token? LastToken = i - 1 >= 0 ? Tokens[i - 1] : null;
                Phase1Token Token = Tokens[i];

                if (Token.Type == Phase1TokenType.Identifier) {
                    NewTokens.Add(IdentifierToPhase2(Token, LastToken != null && LastToken.Type == Phase1TokenType.Dot));
                }
                else if (Token.Type == Phase1TokenType.Integer) {
                    if (i + 2 < Tokens.Count && Tokens[i + 1].Type == Phase1TokenType.Dot && Tokens[i + 2].Type == Phase1TokenType.Integer) {
                        NewTokens.Add(new Phase2Token(Token.Location, Phase2TokenType.Float, Token.Value + "." + Tokens[i + 2].Value, Token));
                        i += 2;
                    }
                    else {
                        NewTokens.Add(new Phase2Token(Token.Location, Phase2TokenType.Integer, Token.Value, Token));
                    }
                }
                else {
                    NewTokens.Add(new Phase2Token(Token.Location, Token.Type switch {
                        Phase1TokenType.String => Phase2TokenType.String,
                        Phase1TokenType.AssignmentOperator => Phase2TokenType.AssignmentOperator,
                        Phase1TokenType.Operator => Phase2TokenType.Operator,
                        Phase1TokenType.Dot => Phase2TokenType.Dot,
                        Phase1TokenType.DoubleColon => Phase2TokenType.DoubleColon,
                        Phase1TokenType.Comma => Phase2TokenType.Comma,
                        Phase1TokenType.SplatOperator => Phase2TokenType.SplatOperator,
                        Phase1TokenType.OpenBracket => Phase2TokenType.OpenBracket,
                        Phase1TokenType.CloseBracket => Phase2TokenType.CloseBracket,
                        Phase1TokenType.StartCurly => Phase2TokenType.StartCurly,
                        Phase1TokenType.EndCurly => Phase2TokenType.EndCurly,
                        Phase1TokenType.StartSquare => Phase2TokenType.StartSquare,
                        Phase1TokenType.EndSquare => Phase2TokenType.EndSquare,
                        Phase1TokenType.Pipe => Phase2TokenType.Pipe,
                        Phase1TokenType.RightArrow => Phase2TokenType.RightArrow,
                        Phase1TokenType.InclusiveRange => Phase2TokenType.InclusiveRange,
                        Phase1TokenType.ExclusiveRange => Phase2TokenType.ExclusiveRange,
                        Phase1TokenType.TernaryQuestion => Phase2TokenType.TernaryQuestion,
                        Phase1TokenType.TernaryElse => Phase2TokenType.TernaryElse,
                        Phase1TokenType.EndOfStatement => Phase2TokenType.EndOfStatement,
                        _ => throw new InternalErrorException($"{Token.Location}: Tried to convert {Token.Type} from phase 1 to phase 2")
                    }, Token.Value, Token));
                }
            }
            return NewTokens;
        }
        static List<Phase2Object> GetObjectsUntil(List<Phase2Object> Objects, ref int Index, Func<Phase2Object, bool> Condition, bool OneOnly = false) {
            List<Phase2Object> Tokens = new();
            while (Index < Objects.Count) {
                Phase2Object Token = Objects[Index];
                if (Condition(Token)) {
                    break;
                }
                Tokens.Add(Token);
                Index++;
                if (OneOnly) {
                    break;
                }
            }
            return Tokens;
        }
        static List<Expression> BuildArguments(List<Phase2Object> Objects, ref int Index, ArgumentParentheses Parentheses, bool OneOnly) {
            List<Phase2Object> ArgumentObjects;

            // Brackets e.g. puts("hi")
            if (Parentheses == ArgumentParentheses.Brackets) {
                int OpenBracketDepth = 0;
                ArgumentObjects = GetObjectsUntil(Objects, ref Index, Obj => {
                    if (Obj is Phase2Token Tok) {
                        if (Tok.Type == Phase2TokenType.OpenBracket) {
                            OpenBracketDepth++;
                        }
                        else if (Tok.Type == Phase2TokenType.CloseBracket) {
                            if (OpenBracketDepth == 0) return true;
                            OpenBracketDepth--;
                        }
                    }
                    return false;
                });
            }
            // Pipes e.g. |hi|
            else if (Parentheses == ArgumentParentheses.Pipes) {
                ArgumentObjects = GetObjectsUntil(Objects, ref Index, Obj => {
                    if (Obj is Phase2Token Tok) {
                        if (Tok.Type == Phase2TokenType.Pipe) {
                            return true;
                        }
                    }
                    return false;
                });
            }
            // No brackets e.g. puts "hi"
            else {
                ArgumentObjects = GetObjectsUntil(Objects, ref Index, Object => (Object is Phase2Token Token
                    && (Token.Type is Phase2TokenType.EndOfStatement or Phase2TokenType.If or Phase2TokenType.Unless
                    or Phase2TokenType.While or Phase2TokenType.Until)) || Object is Statement || (Object is DoExpression Do && !Do.HighPriority), OneOnly
                );
                Index--;
            }

            List<Expression> Arguments = new();

            int IndexOfRightArrow = ArgumentObjects.FindIndex(Arg => Arg is Phase2Token Tok && Tok.Type == Phase2TokenType.RightArrow);
            // Arguments with double-splat hash arguments
            if (IndexOfRightArrow != -1) {
                // Find index of hash arguments
                int StartHashArgumentsIndex = ArgumentObjects.FindLastIndex(IndexOfRightArrow - 1, Arg => Arg is Phase2Token Tok && Tok.Type == Phase2TokenType.Comma);
                if (StartHashArgumentsIndex == -1)
                    StartHashArgumentsIndex = 0;
                // Get comma-separated arguments
                Arguments = ObjectsToExpressions(ArgumentObjects.GetRange(0, StartHashArgumentsIndex), ExpressionsType.CommaSeparatedExpressions);
                // Get hash arguments
                if (StartHashArgumentsIndex != 0)
                    StartHashArgumentsIndex++;
                List<Expression> HashArguments = ObjectsToExpressions(ArgumentObjects.GetIndexRange(StartHashArgumentsIndex), ExpressionsType.KeyValueExpressions);
                HashArgumentsExpression HashArgumentsExpression = new(HashArguments.Location(), HashArguments.ListAsHash());
                Arguments.Add(HashArgumentsExpression);
            }
            // Only comma-separated arguments
            else {
                Arguments = ObjectsToExpressions(ArgumentObjects, ExpressionsType.CommaSeparatedExpressions);
            }

            return Arguments;
        }
        static List<Expression> ParseArgumentsWithBrackets(List<Phase2Object> Objects, ref int Index, bool OneOnly) {
            Index += 2;
            List<Expression> Arguments = BuildArguments(Objects, ref Index, ArgumentParentheses.Brackets, OneOnly);
            return Arguments;
        }
        static List<Expression>? ParseArgumentsWithoutBrackets(List<Phase2Object> Objects, ref int Index, bool OneOnly) {
            Index++;
            List<Expression> Arguments = BuildArguments(Objects, ref Index, ArgumentParentheses.NoBrackets, OneOnly);
            return Arguments;
        }
        static List<Expression> ParseArgumentsWithPipes(List<Phase2Object> Objects, ref int Index, bool OneOnly) {
            Index += 2;
            List<Expression> Arguments = BuildArguments(Objects, ref Index, ArgumentParentheses.Pipes, OneOnly);
            return Arguments;
        }
        enum ArgumentParentheses {
            Unknown,
            Brackets,
            NoBrackets,
            Pipes
        }
        /// <summary><paramref name="OneOnly"/> only applies if the arguments have no parentheses.</summary>
        static List<Expression>? ParseArguments(List<Phase2Object> Objects, ref int Index, ArgumentParentheses Parentheses = ArgumentParentheses.Unknown, bool OneOnly = false) {
            if (Index + 1 < Objects.Count) {
                Phase2Object NextObject = Objects[Index + 1];
                switch (Parentheses) {
                    // Brackets / No Brackets
                    case ArgumentParentheses.Unknown: {
                        List<Expression>? Arguments = ParseArguments(Objects, ref Index, ArgumentParentheses.Brackets)
                            ?? ParseArguments(Objects, ref Index, ArgumentParentheses.NoBrackets, OneOnly);
                        return Arguments;
                    }
                    // Brackets
                    case ArgumentParentheses.Brackets: {
                        if (NextObject is Phase2Token NextToken && NextToken.Type == Phase2TokenType.OpenBracket && !NextToken.FollowsWhitespace)
                            return ParseArgumentsWithBrackets(Objects, ref Index, OneOnly);
                        else
                            return null;
                    }
                    // No brackets
                    case ArgumentParentheses.NoBrackets: {
                        if (NextObject is Expression && NextObject is not Statement && NextObject is not DoExpression)
                            return ParseArgumentsWithoutBrackets(Objects, ref Index, OneOnly);
                        else if (NextObject is Phase2Token NextToken && NextToken.Type == Phase2TokenType.OpenBracket && NextToken.FollowsWhitespace)
                            return ParseArgumentsWithoutBrackets(Objects, ref Index, OneOnly);
                        else
                            return null;
                    }
                    // Pipes
                    case ArgumentParentheses.Pipes: {
                        return ParseArgumentsWithPipes(Objects, ref Index, OneOnly);
                    }
                }
            }
            return null;
        }
        static List<T> GetCommaSeparatedExpressions<T>(List<Phase2Object> Objects, ref int Index, bool Backwards = false) where T : Expression {
            List<T> Expressions = new();
            bool ExpectingVariable = true;
            bool HandleObject(Phase2Object Object) {
                if (Object is Phase2Token Token2 && Token2.Type == Phase2TokenType.Comma) {
                    if (ExpectingVariable || Expressions.Count == 0) throw new SyntaxErrorException($"{Token2.Location}: Unexpected comma");
                    ExpectingVariable = true;
                }
                else if (Object is T TargetExpression) {
                    Expressions.Add(TargetExpression);
                    ExpectingVariable = false;
                }
                else return true;
                return false;
            }
            if (!Backwards) {
                for (; Index < Objects.Count;) {
                    if (HandleObject(Objects[Index])) break;
                    Objects.RemoveAt(Index);
                }
            }
            else {
                for (; Index >= 0; Index--) {
                    if (HandleObject(Objects[Index])) break;
                    Objects.RemoveAt(Index);
                }
            }
            if (ExpectingVariable) throw new SyntaxErrorException($"{Objects.Location()}: Expected expression");
            return Expressions;
        }
        static List<Expression> GetCommaSeparatedExpressions(List<Phase2Object> Objects, ref int Index, bool Backwards = false) {
            return GetCommaSeparatedExpressions<Expression>(Objects, ref Index, Backwards);
        }
        static List<Phase2Object> SkipParentheses(List<Phase2Object> Phase2Objects, ref int Index) {
            List<Phase2Object> Bite = new();
            int ParenthesesStack = 0;
            for (; Index < Phase2Objects.Count; Index++) {
                Phase2Object UnknownObject = Phase2Objects[Index];
                Phase2TokenType? TokenType = UnknownObject is Phase2Token Token ? Token.Type : null;
                if (TokenType != null) {
                    if (StartParenthesesTokens.Contains(TokenType.Value)) {
                        ParenthesesStack++;
                    }
                    else if (EndParenthesesTokens.Contains(TokenType.Value)) {
                        ParenthesesStack--;
                    }
                }
                Bite.Add(UnknownObject);
                if (ParenthesesStack == 0) break;
            }
            return Bite;
        }
        static ObjectTokenExpression GetMethodName(List<Phase2Object> Phase2Objects, ref int Index) {
            SelfExpression? StartsWithSelf = null;
            bool NextTokenCanBeVariable = true;
            bool NextTokenCanBeDot = false;
            List<Phase2Token> MethodNamePath = new();
            for (Index++; Index < Phase2Objects.Count; Index++) {
                Phase2Object Object = Phase2Objects[Index];

                if (Object is Phase2Token Token) {
                    if (Token.Type == Phase2TokenType.Dot) {
                        if (NextTokenCanBeDot) {
                            NextTokenCanBeVariable = true;
                            NextTokenCanBeDot = false;
                            continue;
                        }
                        else {
                            throw new SyntaxErrorException($"{Token.Location}: Expected expression before and after .");
                        }
                    }
                    else if (Token.IsObjectToken || Token.Type is Phase2TokenType.OpenBracket or Phase2TokenType.SplatOperator or Phase2TokenType.EndOfStatement) {
                        break;
                    }
                    // def variable=
                    else if (Token.Type == Phase2TokenType.AssignmentOperator) {
                        if (Token.Value! == "=" && MethodNamePath.Count >= 1 && IsVariableToken(MethodNamePath[^1])) {
                            MethodNamePath[^1].Value += "=";
                            Index++;
                            break;
                        }
                    }
                    // def +, def -, def *, def /, def %, def **, def ==, def <, def >, def <=, def >=, def <<, def <=>
                    else if (Token.Type == Phase2TokenType.Operator) {
                        if (Token.Value is "+" or "-" or "*" or "/" or "%" or "<" or ">" or "**" or "==" or "<=" or ">=" or "<<" or "<=>") {
                            MethodNamePath.Add(new Phase2Token(Token.Location, Phase2TokenType.LocalVariableOrMethod, Token.Value));
                            Index++;
                            break;
                        }
                    }
                }
                else if (Object is ObjectTokenExpression ObjectToken) {
                    if (IsVariableToken(ObjectToken.Token)) {
                        if (NextTokenCanBeVariable) {
                            MethodNamePath.Add(ObjectToken.Token);
                            NextTokenCanBeVariable = false;
                            NextTokenCanBeDot = true;
                            continue;
                        }
                        else {
                            break;
                        }
                    }
                }
                else if (Object is SelfExpression SelfExpression) {
                    if (MethodNamePath.Count == 0 && StartsWithSelf == null) {
                        StartsWithSelf = SelfExpression;
                        NextTokenCanBeVariable = false;
                        NextTokenCanBeDot = true;
                        continue;
                    }
                }
                throw new SyntaxErrorException($"{Object.Location}: Unexpected token while parsing method path: {Object.Inspect()}");
            }

            // Get path expression from tokens
            ObjectTokenExpression? MethodNamePathExpression = null;
            if (MethodNamePath.Count == 1) {
                if (StartsWithSelf == null) {
                    MethodNamePathExpression = new ObjectTokenExpression(MethodNamePath[0]);
                }
                else {
                    MethodNamePathExpression = new PathExpression(StartsWithSelf, MethodNamePath[0]);
                }
            }
            else if (MethodNamePath.Count == 0) {
                throw new SyntaxErrorException($"{Phase2Objects[Index].Location}: Def keyword must be followed by an identifier (got {Phase2Objects[Index].Inspect()})");
            }
            else {
                int StartLoopIndex = 0;
                if (StartsWithSelf != null) {
                    StartLoopIndex++;
                    MethodNamePathExpression = new PathExpression(StartsWithSelf, MethodNamePath[0]);
                }
                for (int i = StartLoopIndex; i < MethodNamePath.Count; i++) {
                    if (MethodNamePathExpression == null) {
                        MethodNamePathExpression = new ObjectTokenExpression(MethodNamePath[i]);
                    }
                    else {
                        MethodNamePathExpression = new PathExpression(MethodNamePathExpression, MethodNamePath[i]);
                    }
                }
            }
            return MethodNamePathExpression!;
        }
        static List<MethodArgumentExpression> GetMethodArguments(List<Phase2Object> Phase2Objects, ref int Index, bool IsPipes, DebugLocation Location) {
            List<MethodArgumentExpression> MethodArguments = new();
            bool WrappedInBrackets = false;
            bool NextTokenCanBeObject = true;
            bool NextTokenCanBeComma = false;
            SplatType? SplatArgumentType = null;
            int StartIndex = Index;
            for (; Index < Phase2Objects.Count; Index++) {
                Phase2Object Object = Phase2Objects[Index];

                if (Object is Phase2Token Token) {
                    if (Token.Type == Phase2TokenType.Comma) {
                        if (NextTokenCanBeComma) {
                            NextTokenCanBeObject = true;
                            NextTokenCanBeComma = false;
                        }
                        else {
                            throw new SyntaxErrorException($"{Token.Location}: Unexpected comma");
                        }
                    }
                    else if (Token.Type == Phase2TokenType.SplatOperator) {
                        if (NextTokenCanBeObject) {
                            SplatArgumentType = Token.Value!.Length == 1 ? SplatType.Single : SplatType.Double;
                        }
                        else {
                            throw new SyntaxErrorException($"{Token.Location}: Unexpected splat operator");
                        }
                    }
                    else if (Token.Type == Phase2TokenType.AssignmentOperator && Token.Value == "=") {
                        if (MethodArguments.Count == 0) {
                            throw new SyntaxErrorException($"{Token.Location}: Unexpected '=' when parsing arguments");
                        }
                        if (MethodArguments[^1].DefaultValue != null) {
                            throw new SyntaxErrorException($"{Token.Location}: Default value already assigned");
                        }
                        List<Phase2Object> DefaultValueObjects = new();
                        for (Index++; Index < Phase2Objects.Count; Index++) {
                            Phase2Object Obj = Phase2Objects[Index];
                            if (Obj is Phase2Token Tok) {
                                if (Tok.Type is Phase2TokenType.Comma or Phase2TokenType.CloseBracket or Phase2TokenType.Pipe or Phase2TokenType.EndOfStatement) {
                                    Index--;
                                    break;
                                }
                                else if (StartParenthesesTokens.Contains(Tok.Type)) {
                                    // Skip nested parentheses
                                    DefaultValueObjects.AddRange(SkipParentheses(Phase2Objects, ref Index));
                                    continue;
                                }
                            }
                            DefaultValueObjects.Add(Obj);
                        }
                        if (DefaultValueObjects.Count == 0) {
                            throw new SyntaxErrorException($"{Token.Location}: Expected value after '='");
                        }
                        else {
                            if (MethodArguments[^1].SplatType != null) {
                                throw new SyntaxErrorException($"{Token.Location}: Splat arguments cannot have default values");
                            }
                            MethodArguments[^1].DefaultValue = ObjectsToExpression(DefaultValueObjects);
                        }
                    }
                    else if (!IsPipes && Token.Type == Phase2TokenType.OpenBracket) {
                        if (Index == StartIndex) {
                            WrappedInBrackets = true;
                        }
                        else {
                            throw new SyntaxErrorException($"{Token.Location}: Unexpected open bracket in method arguments");
                        }
                    }
                    else if (!IsPipes && Token.Type == Phase2TokenType.CloseBracket) {
                        if (WrappedInBrackets) {
                            break;
                        }
                        else {
                            throw new SyntaxErrorException($"{Token.Location}: Unexpected close bracket in method arguments");
                        }
                    }
                    else if (IsPipes && Token.Type == Phase2TokenType.Pipe) {
                        if (Index != StartIndex) {
                            break;
                        }
                    }
                    else if (Token.Type == Phase2TokenType.EndOfStatement) {
                        if (!WrappedInBrackets && !IsPipes) {
                            break;
                        }
                    }
                    else {
                        throw new SyntaxErrorException($"{Token.Location}: Expected {(NextTokenCanBeObject ? "argument" : "comma")}, got {Token.Inspect()}");
                    }
                }
                else if (Object is ObjectTokenExpression ObjectToken) {
                    if (ObjectToken is not PathExpression && IsVariableToken(ObjectToken.Token)) {
                        if (NextTokenCanBeObject) {
                            MethodArguments.Add(new MethodArgumentExpression(ObjectToken.Token, null, SplatArgumentType));
                            NextTokenCanBeObject = false;
                            NextTokenCanBeComma = true;
                            SplatArgumentType = null;
                        }
                        else {
                            throw new SyntaxErrorException($"{ObjectToken.Location}: Unexpected argument {ObjectToken.Inspect()}");
                        }
                    }
                    else {
                        throw new SyntaxErrorException($"{ObjectToken.Location}: Expected {(NextTokenCanBeObject ? "argument" : "comma")}, got {ObjectToken.Inspect()}");
                    }
                }
            }
            if (!NextTokenCanBeComma && NextTokenCanBeObject && MethodArguments.Count != 0) {
                throw new SyntaxErrorException($"{Location.Line}: Expected argument after comma, got nothing");
            }
            if (SplatArgumentType != null) {
                throw new SyntaxErrorException($"{Location.Line}: Expected argument after splat operator, got nothing");
            }
            return MethodArguments;
        }
        static void CheckMethodNameAndArgumentsValidity(List<MethodArgumentExpression> MethodArguments) {
            {
                bool HasSingleSplat = false;
                bool HasDoubleSplat = false;
                foreach (MethodArgumentExpression MethodArgument in MethodArguments) {
                    if (HasDoubleSplat) {
                        throw new SyntaxErrorException($"{MethodArgument.ArgumentName.Location}: Double splat (**) argument must be the last argument");
                    }
                    if (MethodArgument.SplatType == SplatType.Double) {
                        HasDoubleSplat = true;
                    }
                    else if (MethodArgument.SplatType == SplatType.Single) {
                        if (HasSingleSplat) {
                            throw new SyntaxErrorException($"{MethodArgument.ArgumentName.Location}: Cannot have multiple splat (*) arguments");
                        }
                        HasSingleSplat = true;
                    }
                }
            }
            {
                HashSet<string> ExistingArgumentNames = new();
                foreach (MethodArgumentExpression MethodArgument in MethodArguments) {
                    if (ExistingArgumentNames.Contains(MethodArgument.ArgumentName.Value!)) {
                        throw new SyntaxErrorException($"{MethodArgument.ArgumentName.Location}: Duplicated argument name ('{MethodArgument.ArgumentName.Value!}')");
                    }
                    else {
                        ExistingArgumentNames.Add(MethodArgument.ArgumentName.Value!);
                    }
                }
            }
        }
        static ObjectTokenExpression GetClassName(List<Phase2Object> Phase2Objects, ref int Index, string ObjectType, out ObjectTokenExpression? InheritsFrom) {
            SelfExpression? StartsWithSelf = null;
            bool NextTokenCanBeVariable = true;
            bool NextTokenCanBeDoubleColon = false;
            List<Phase2Token> ClassNamePath = new();
            for (Index++; Index < Phase2Objects.Count; Index++) {
                Phase2Object Object = Phase2Objects[Index];

                if (Object is Phase2Token Token) {
                    if (Token.Type == Phase2TokenType.DoubleColon) {
                        if (NextTokenCanBeDoubleColon) {
                            NextTokenCanBeVariable = true;
                            NextTokenCanBeDoubleColon = false;
                            continue;
                        }
                        else {
                            throw new SyntaxErrorException($"{Token.Location}: Expected expression before and after '.'");
                        }
                    }
                    else if (Token.Type == Phase2TokenType.Operator && Token.Value! == "<") {
                        if (NextTokenCanBeDoubleColon) {
                            ClassNamePath.Add(Token);
                            NextTokenCanBeVariable = true;
                            NextTokenCanBeDoubleColon = false;
                            continue;
                        }
                        else {
                            break;
                        }
                    }
                    else if (Token.Type == Phase2TokenType.EndOfStatement || Token.IsObjectToken) {
                        break;
                    }
                }
                else if (Object is ObjectTokenExpression ObjectToken) {
                    if (IsVariableToken(ObjectToken.Token)) {
                        if (NextTokenCanBeVariable) {
                            ClassNamePath.Add(ObjectToken.Token);
                            NextTokenCanBeVariable = false;
                            NextTokenCanBeDoubleColon = true;
                            continue;
                        }
                        else {
                            break;
                        }
                    }
                }
                else if (Object is SelfExpression SelfExpression) {
                    if (ClassNamePath.Count == 0 && StartsWithSelf == null) {
                        StartsWithSelf = SelfExpression;
                        NextTokenCanBeVariable = false;
                        NextTokenCanBeDoubleColon = true;
                        continue;
                    }
                }
                throw new SyntaxErrorException($"{Object.Location}: Unexpected token while parsing class path: {Object.Inspect()}");
            }

            // Find inheritance operator
            List<Phase2Token>? InheritsFromNamePath = null;
            for (int i = 0; i < ClassNamePath.Count; i++) {
                Phase2Token Object = ClassNamePath[i];

                if (Object is Phase2Token Token) {
                    if (Token.Type == Phase2TokenType.Operator && Token.Value! == "<") {
                        if (i + 1 >= ClassNamePath.Count) {
                            throw new SyntaxErrorException($"{Object.Location}: Expected identifier after '<'");
                        }
                        InheritsFromNamePath = ClassNamePath.GetIndexRange(i + 1);
                        ClassNamePath.RemoveIndexRange(i);
                        break;
                    }
                }
            }

            // Get path expression from tokens
            ObjectTokenExpression GetPathExpressionFromTokens(List<Phase2Token> Tokens, int Index) {
                ObjectTokenExpression? PathExpression = null;
                if (Tokens.Count == 1) {
                    if (StartsWithSelf == null) {
                        PathExpression = new ObjectTokenExpression(Tokens[0]);
                    }
                    else {
                        PathExpression = new PathExpression(StartsWithSelf, Tokens[0]);
                    }
                }
                else if (Tokens.Count == 0) {
                    throw new SyntaxErrorException($"{Phase2Objects[Index].Location}: Class keyword must be followed by an identifier (got {Phase2Objects[Index].Inspect()})");
                }
                else {
                    int StartLoopIndex = 0;
                    if (StartsWithSelf != null) {
                        StartLoopIndex++;
                        PathExpression = new PathExpression(StartsWithSelf, Tokens[0]);
                    }
                    for (int i = StartLoopIndex; i < Tokens.Count; i++) {
                        if (PathExpression == null) {
                            PathExpression = new ObjectTokenExpression(Tokens[i]);
                        }
                        else {
                            PathExpression = new PathExpression(PathExpression, Tokens[i]);
                        }
                    }
                }
                return PathExpression!;
            }
            ObjectTokenExpression? ClassNamePathExpression = GetPathExpressionFromTokens(ClassNamePath, Index);
            if (InheritsFromNamePath != null) {
                InheritsFrom = GetPathExpressionFromTokens(InheritsFromNamePath, Index);
            }
            else {
                InheritsFrom = null;
            }

            // Verify class name is constant
            if (ClassNamePathExpression.Token.Type != Phase2TokenType.ConstantOrMethod) {
                throw new SyntaxErrorException($"{ClassNamePathExpression.Location}: {ObjectType} name must be Constant");
            }

            return ClassNamePathExpression;
        }
        static Expression? EndBlock(Stack<BuildingBlock> CurrentBlocks, bool EndIsCurly) {
            BuildingBlock Block = CurrentBlocks.Pop();

            // End Method Block
            if (Block is BuildingMethod MethodBlock) {
                return new DefineMethodStatement(MethodBlock.MethodName,
                    new MethodExpression(MethodBlock.Location, MethodBlock.Statements, new IntRange(MethodBlock.MinArgumentsCount, MethodBlock.MaxArgumentsCount), MethodBlock.Arguments, MethodBlock.MethodName.Token.Value)
                );
            }
            // End Class/Module Block
            else if (Block is BuildingClass ClassBlock) {
                return new DefineClassStatement(ClassBlock.ClassName, ClassBlock.Statements, ClassBlock.IsModule, ClassBlock.InheritsFrom);
            }
            // End Do Block
            else if (Block is BuildingDo DoBlock) {
                // Verify end type
                if (EndIsCurly && !DoBlock.DoIsCurly) {
                    throw new SyntaxErrorException($"{DoBlock.Location}: Unexpected '}}'; did you mean 'do'?");
                }
                else if (!EndIsCurly && DoBlock.DoIsCurly) {
                    throw new SyntaxErrorException($"{DoBlock.Location}: Unexpected 'do'; did you mean '}}'?");
                }

                MethodExpression OnYield = new(DoBlock.Location, DoBlock.Statements, null, DoBlock.Arguments, null);
                return new DoExpression(OnYield, DoBlock.DoIsCurly);
            }
            // End If Block
            else if (Block is BuildingIfBranches IfBranches) {
                List<IfExpression> IfExpressions = new();
                for (int i = 0; i < IfBranches.Branches.Count; i++) {
                    BuildingIf Branch = IfBranches.Branches[i];
                    if (Branch.Condition == null && i != IfBranches.Branches.Count - 1) {
                        throw new SyntaxErrorException($"{Branch.Location}: Else must be the last branch in an if statement");
                    }
                    IfExpressions.Add(new IfExpression(Branch.Location, Branch.Condition, Branch.Statements, Branch.Inverse));
                }
                return new IfBranchesStatement(IfBranches.Location, IfExpressions);
            }
            // End Begin Block
            else if (Block is BuildingBeginBranches BeginBranches) {
                List<BeginComponentStatement> BeginStatements = new();
                for (int i = 0; i < BeginBranches.Branches.Count; i++) {
                    BuildingBeginComponent Branch = BeginBranches.Branches[i];
                    if (Branch is BuildingBegin) {
                        BeginStatements.Add(new BeginStatement(Branch.Location, Branch.Statements));
                    }
                    else if (Branch is BuildingRescue BuildingRescue) {
                        BeginStatements.Add(new RescueStatement(Branch.Location, Branch.Statements, BuildingRescue.Exception, BuildingRescue.ExceptionVariable));
                    }
                    else if (Branch is BuildingRescueElse) {
                        BeginStatements.Add(new RescueElseStatement(Branch.Location, Branch.Statements));
                    }
                    else if (Branch is BuildingEnsure) {
                        BeginStatements.Add(new EnsureStatement(Branch.Location, Branch.Statements));
                    }
                    else {
                        throw new InternalErrorException($"{Branch.Location}: {Branch.GetType().Name} not handled");
                    }
                }
                return new BeginBranchesStatement(BeginBranches.Location, BeginStatements);
            }
            // End While Block
            else if (Block is BuildingWhile WhileBlock) {
                return new WhileStatement(new WhileExpression(WhileBlock.Location, WhileBlock.Condition!, WhileBlock.Statements, WhileBlock.Inverse));
            }
            // End Case Block
            else if (Block is BuildingCase CaseBlock) {
                List<WhenExpression> WhenExpressions = new();
                for (int i = 0; i < CaseBlock.Branches.Count; i++) {
                    BuildingWhen Branch = CaseBlock.Branches[i];
                    WhenExpressions.Add(new WhenExpression(Branch.Location, Branch.Conditions, Branch.Statements));
                }
                return new CaseExpression(CaseBlock.Location, CaseBlock.Subject, WhenExpressions);
            }
            // End For Block
            else if (Block is BuildingFor ForBlock) {
                return new ForStatement(ForBlock.Location, ForBlock.VariableNames, ForBlock.InExpression, ForBlock.Statements);
            }
            // End Unknown Block (internal error)
            else {
                throw new InternalErrorException($"{Block.Location}: End block not handled for type: {Block.GetType().Name}");
            }
        }
        enum ReturnOrYieldOrSuper {
            Return,
            Yield,
            Super,
        }
        static Expression ParseReturnOrYieldOrSuper(DebugLocation Location, List<Phase2Object> StatementTokens, ref int Index, ReturnOrYieldOrSuper ReturnOrYieldOrSuper) {
            // Get return/yield values
            List<Expression>? ReturnOrYieldValues = ParseArguments(StatementTokens, ref Index);
            // Create yield/return statement
            if (ReturnOrYieldOrSuper == ReturnOrYieldOrSuper.Return) {
                if (ReturnOrYieldValues == null || ReturnOrYieldValues.Count == 0) {
                    return new ReturnStatement(Location);
                }
                else if (ReturnOrYieldValues.Count == 1) {
                    return new ReturnStatement(Location, ReturnOrYieldValues[0]);
                }
                else {
                    return new ReturnStatement(Location, new ArrayExpression(ReturnOrYieldValues.Location(), ReturnOrYieldValues));
                }
            }
            else if (ReturnOrYieldOrSuper == ReturnOrYieldOrSuper.Super) {
                return new SuperStatement(Location, ReturnOrYieldValues);
            }
            else {
                return new YieldStatement(Location, ReturnOrYieldValues);
            }
        }
        static Expression ParseUndef(DebugLocation Location, List<Phase2Object> StatementTokens, ref int Index) {
            // Get undef method name
            List<Expression>? UndefName = ParseArguments(StatementTokens, ref Index);
            // Get method name
            if (UndefName != null) {
                // Create undef statement
                if (UndefName.Count == 1 && UndefName[0].GetType() == typeof(ObjectTokenExpression)) {
                    return new UndefineMethodStatement(Location, (ObjectTokenExpression)UndefName[0]);
                }
                else {
                    throw new SyntaxErrorException($"{Location}: Expected local method name after 'undef', got {UndefName.Inspect()}");
                }
            }
            else {
                throw new SyntaxErrorException($"{Location}: Expected method name after 'undef', got nothing");
            }
        }
        static BuildingIf ParseIf(DebugLocation Location, List<Phase2Object> StatementTokens, ref int Index, bool Inverse = false) {
            // Get condition
            Index++;
            List<Phase2Object> ConditionObjects = GetObjectsUntil(StatementTokens, ref Index, Obj => Obj is Phase2Token Tok && (Tok.Type is Phase2TokenType.Then or Phase2TokenType.EndOfStatement));
            Expression ConditionExpression = ObjectsToExpression(ConditionObjects);

            // Open if block
            return new BuildingIf(Location, ConditionExpression, Inverse);
        }
        static BuildingRescue ParseRescue(DebugLocation Location, List<Phase2Object> StatementTokens, ref int Index) {
            // Get exception and exception variable
            Index++;
            ObjectTokenExpression? ExceptionExpression = null;
            Phase2Token? ExceptionVariable = null;
            {
                // Get exception
                List<Phase2Object> ExceptionObjects = GetObjectsUntil(StatementTokens, ref Index, Obj => Obj is Phase2Token Tok && (Tok.Type is Phase2TokenType.RightArrow or Phase2TokenType.EndOfStatement));
                if (ExceptionObjects.Count != 0) {
                    ExceptionExpression = ObjectsToExpression(ExceptionObjects) as ObjectTokenExpression
                        ?? throw new SyntaxErrorException($"{Location}: Expected exception or end of statement after 'rescue', got '{ExceptionObjects.Inspect()}'");
                }
                // Get right arrow
                if (StatementTokens[Index] is Phase2Token Tok && Tok.Type == Phase2TokenType.RightArrow) {
                    Index++;
                    // Get exception variable after right arrow
                    List<Phase2Object> ExceptionVariableObjects = GetObjectsUntil(StatementTokens, ref Index, Obj => Obj is Phase2Token Tok && (Tok.Type == Phase2TokenType.EndOfStatement));
                    if (ExceptionVariableObjects.Count != 0) {
                        Expression ExceptionVariableObject = ObjectsToExpression(ExceptionVariableObjects);
                        if (ExceptionVariableObject.GetType() == typeof(ObjectTokenExpression)) {
                            ExceptionVariable = ((ObjectTokenExpression)ExceptionVariableObject).Token;
                        }
                        else {
                            throw new SyntaxErrorException($"{Location}: Expected exception name after '=>' after 'rescue', got {ExceptionVariableObject.GetType().Name}");
                        }
                    }
                    else {
                        throw new SyntaxErrorException($"{Location}: Expected exception name after '=>' after 'rescue', got nothing");
                    }
                }
            }

            // Open rescue block
            return new BuildingRescue(Location, ExceptionExpression, ExceptionVariable);
        }
        static BuildingCase ParseCase(DebugLocation Location, List<Phase2Object> StatementTokens, ref int Index) {
            // Get case subject
            Index++;
            List<Phase2Object> CaseExpressionObjects = GetObjectsUntil(StatementTokens, ref Index, Obj => Obj is Phase2Token Tok && Tok.Type is Phase2TokenType.When);
            Index--;
            CaseExpressionObjects.RemoveFromEnd(Obj => Obj is Phase2Token Tok && Tok.Type == Phase2TokenType.EndOfStatement);
            Expression CaseExpression = ObjectsToExpression(CaseExpressionObjects);

            // Open case block
            return new BuildingCase(Location, CaseExpression);
        }
        static BuildingWhen ParseWhen(DebugLocation Location, List<Phase2Object> StatementTokens, ref int Index) {
            // Get condition(s)
            Index++;
            List<Phase2Object> ConditionObjects = GetObjectsUntil(StatementTokens, ref Index, Obj => Obj is Phase2Token Tok && (Tok.Type is Phase2TokenType.Then or Phase2TokenType.EndOfStatement));
            List<Expression> ConditionExpressions = ObjectsToExpressions(ConditionObjects, ExpressionsType.CommaSeparatedExpressions);
            if (ConditionExpressions.Count == 0)
                throw new SyntaxErrorException($"{Location}: Expected condition after 'when'");

            // Open when block
            return new BuildingWhen(Location, ConditionExpressions);
        }
        static BuildingFor ParseFor(DebugLocation Location, List<Phase2Object> StatementTokens, ref int Index) {
            // Get for variables
            List<MethodArgumentExpression> ForVariables = new();
            bool ExpectingVariable = false;
            for (; Index < StatementTokens.Count; Index++) {
                Phase2Object Object = StatementTokens[Index];
                if (Object is Phase2Token Token) {
                    if (Token.Type == Phase2TokenType.Comma) {
                        if (ExpectingVariable || ForVariables.Count == 0) throw new SyntaxErrorException($"{Location}: Unexpected comma");
                        ExpectingVariable = true;
                        continue;
                    }
                    else if (Token.Type == Phase2TokenType.In) {
                        break;
                    }
                }
                else if (Object.GetType() == typeof(ObjectTokenExpression)) {
                    ForVariables.Add(new MethodArgumentExpression(((ObjectTokenExpression)Object).Token));
                    ExpectingVariable = false;
                    continue;
                }
                throw new SyntaxErrorException($"{Location}: Unexpected '{Object.Inspect()}' in for statement");
            }
            if (ExpectingVariable) throw new SyntaxErrorException($"{Location}: Expected identifier after comma");
            if (ForVariables.Count == 0) throw new SyntaxErrorException($"{Location}: Expected variable name after 'for'");
            // Eat in keyword
            if (Index < StatementTokens.Count && StatementTokens[Index] is Phase2Token Tok && Tok.Type == Phase2TokenType.In) {
                // Get in expression
                Index++;
                List<Phase2Object> In = GetObjectsUntil(StatementTokens, ref Index, Obj => Obj is Phase2Token Tok && (Tok.Type is Phase2TokenType.Do or Phase2TokenType.EndOfStatement));
                Expression InExpression = ObjectsToExpression(In);
                // Open for block
                return new BuildingFor(Location, ForVariables, InExpression);
            }
            else {
                throw new SyntaxErrorException($"{Location}: Expected 'in' after 'for'");
            }
        }
        static List<Phase2Object> GetBlocks(List<Phase2Object> ParsedObjects) {
            if (ParsedObjects.Count == 0) return ParsedObjects;
            Stack<BuildingBlock> CurrentBlocks = new();
            Stack<List<Phase2Object>> PendingObjects = new();
            CurrentBlocks.Push(new BuildingBlock(ParsedObjects[0].Location));
            PendingObjects.Push(new());

            void PushBlock(BuildingBlock Block) {
                CurrentBlocks.Push(Block);
                PendingObjects.Push(new());
            }
            void AddPendingObjectAt(int Index) {
                PendingObjects.Peek().Add(ParsedObjects[Index]);
            }
            void AddPendingObject(Phase2Object Object) {
                PendingObjects.Peek().Add(Object);
            }
            Expression? ResolveEndBlock(bool EndIsCurly) {
                List<Expression> Statements = ObjectsToExpressions(PendingObjects.Pop(), ExpressionsType.Statements);
                BuildingBlock CurrentBlock = CurrentBlocks.Peek();
                foreach (Expression Statement in Statements) {
                    CurrentBlock.AddStatement(Statement);
                }
                return EndBlock(CurrentBlocks, EndIsCurly);
            }
            void ResolveStatementsWithoutEndingBlock() {
                List<Expression> Statements = ObjectsToExpressions(PendingObjects.Peek(), ExpressionsType.Statements);
                BuildingBlock CurrentBlock = CurrentBlocks.Peek();
                foreach (Expression Statement in Statements) {
                    CurrentBlock.AddStatement(Statement);
                }
                PendingObjects.Peek().Clear();
            }

            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object? LastUnknownObject = i - 1 >= 0 ? ParsedObjects[i - 1] : null;
                Phase2Object UnknownObject = ParsedObjects[i];
                DebugLocation Location = UnknownObject.Location;

                if (UnknownObject is Phase2Token Token) {
                    switch (Token.Type) {
                        // Def
                        case Phase2TokenType.Def: {
                            // Get method name
                            ObjectTokenExpression MethodName = GetMethodName(ParsedObjects, ref i);

                            // Get method arguments
                            List<MethodArgumentExpression> MethodArguments = GetMethodArguments(ParsedObjects, ref i, false, MethodName.Location);

                            // Check validity
                            CheckMethodNameAndArgumentsValidity(MethodArguments);

                            // Open define method block
                            PushBlock(new BuildingMethod(Location, MethodName, MethodArguments));
                            break;
                        }
                        // Class/Module
                        case Phase2TokenType.Module:
                        case Phase2TokenType.Class: {
                            bool IsModule = Token.Type == Phase2TokenType.Module;
                            string ObjectType = IsModule ? "Module" : "Class";
            
                            // Get class name
                            ObjectTokenExpression ClassName = GetClassName(ParsedObjects, ref i, ObjectType, out ObjectTokenExpression? InheritsFrom);

                            // Open define class block
                            PushBlock(new BuildingClass(Location, ClassName, IsModule, InheritsFrom));
                            break;
                        }
                        // Do
                        case Phase2TokenType.Do: {
                            i++;

                            // Get do |arguments|
                            List<MethodArgumentExpression> DoArguments;
                            if (ParsedObjects[i] is Phase2Token Tok && Tok.Type == Phase2TokenType.Pipe) {
                                DoArguments = GetMethodArguments(ParsedObjects, ref i, true, Token.Location);
                                // Add end of statement after pipe arguments
                                ParsedObjects.Insert(i + 1, new Phase2Token(Location, Phase2TokenType.EndOfStatement, null));
                            }
                            else {
                                i--;
                                DoArguments = new();
                            }

                            // Open do block
                            PushBlock(new BuildingDo(Location, DoArguments, false));
                            break;
                        }
                        // While / Until
                        case Phase2TokenType.While:
                        case Phase2TokenType.Until: {
                            if (LastUnknownObject != null && !(LastUnknownObject is Phase2Token Tok && Tok.Type == Phase2TokenType.EndOfStatement)) {
                                // Format is statement while/until expression; handle later
                                AddPendingObjectAt(i);
                                break;
                            }

                            i++;
                            List<Phase2Object> Condition = GetObjectsUntil(ParsedObjects, ref i, Obj =>
                                Obj is Phase2Token Tok && (Tok.Type is Phase2TokenType.EndOfStatement or Phase2TokenType.Do));

                            // Open while/until block
                            if (Token.Type == Phase2TokenType.While) {
                                PushBlock(new BuildingWhile(Token.Location, ObjectsToExpression(Condition)));
                            }
                            else {
                                PushBlock(new BuildingWhile(Token.Location, ObjectsToExpression(Condition), true));
                            }
                            break;
                        }
                        // For
                        case Phase2TokenType.For: {
                            i++;
                            
                            // Open for block
                            PushBlock(ParseFor(Location, ParsedObjects, ref i));
                            break;
                        }
                        // If / Unless
                        case Phase2TokenType.If:
                        case Phase2TokenType.Unless: {
                            if (LastUnknownObject != null && !(LastUnknownObject is Phase2Token Tok && Tok.Type == Phase2TokenType.EndOfStatement)) {
                                // Format is statement if/unless expression; handle later
                                AddPendingObjectAt(i);
                                break;
                            }

                            BuildingIf BuildingIf = ParseIf(Token.Location, ParsedObjects, ref i, Token.Type == Phase2TokenType.Unless);
                            PushBlock(new BuildingIfBranches(Token.Location, new List<BuildingIf>() {BuildingIf}));
                            break;
                        }
                        // Elsif
                        case Phase2TokenType.Elsif: {
                            if (CurrentBlocks.TryPeek(out BuildingBlock? Block) && Block is BuildingIfBranches IfBlock) {
                                ResolveStatementsWithoutEndingBlock();
                                IfBlock.Branches.Add(ParseIf(Token.Location, ParsedObjects, ref i));
                            }
                            else {
                                throw new SyntaxErrorException($"{Token.Location}: 'Elsif' must follow 'if'");
                            }
                            break;
                        }
                        // Else
                        case Phase2TokenType.Else: {
                            if (CurrentBlocks.TryPeek(out BuildingBlock? Block) && Block is BuildingIfBranches IfBlock) {
                                ResolveStatementsWithoutEndingBlock();
                                IfBlock.Branches.Add(new BuildingIf(Token.Location, null));
                            }
                            else if (CurrentBlocks.TryPeek(out BuildingBlock? Block2) && Block2 is BuildingBeginBranches BeginBlock) {
                                ResolveStatementsWithoutEndingBlock();
                                if (BeginBlock.Branches[^1] is not BuildingRescue) {
                                    throw new SyntaxErrorException($"{Token.Location}: 'Else' in 'begin' block must follow 'rescue', not '{BeginBlock.Branches[^1].GetType().Name}'");
                                }
                                BeginBlock.Branches.Add(new BuildingRescueElse(Token.Location));
                            }
                            else if (CurrentBlocks.TryPeek(out BuildingBlock? Block3) && Block3 is BuildingCase CaseBlock) {
                                ResolveStatementsWithoutEndingBlock();
                                if (CaseBlock.Branches.Count == 0 || CaseBlock.Branches[^1].Conditions.Count == 0) {
                                    throw new SyntaxErrorException($"{Token.Location}: 'Else' in 'case' block must follow 'when', not 'case'");
                                }
                                CaseBlock.Branches.Add(new BuildingWhen(Token.Location, new List<Expression>()));
                            }
                            else {
                                throw new SyntaxErrorException($"{Token.Location}: 'Else' must follow 'if', 'begin' or 'case'");
                            }
                            break;
                        }
                        // Begin
                        case Phase2TokenType.Begin: {
                            BuildingBegin BuildingBegin = new(Token.Location);
                            PushBlock(new BuildingBeginBranches(Token.Location, new List<BuildingBeginComponent>() {BuildingBegin}));
                            break;
                        }
                        // Rescue
                        case Phase2TokenType.Rescue: {
                            if (LastUnknownObject != null && !(LastUnknownObject is Phase2Token Tok && Tok.Type == Phase2TokenType.EndOfStatement)) {
                                // Format is statement rescue statement; handle later
                                AddPendingObjectAt(i);
                                break;
                            }

                            if (CurrentBlocks.TryPeek(out BuildingBlock? Block) && Block is BuildingBeginBranches BeginBlock) {
                                ResolveStatementsWithoutEndingBlock();
                                if (BeginBlock.Branches[^1] is not (BuildingBegin or BuildingRescue)) {
                                    throw new SyntaxErrorException($"{Token.Location}: 'Rescue' must follow 'begin' or 'rescue', not '{BeginBlock.Branches[^1].GetType().Name}'");
                                }
                                BeginBlock.Branches.Add(ParseRescue(Token.Location, ParsedObjects, ref i));
                            }
                            else {
                                throw new SyntaxErrorException($"{Token.Location}: 'Rescue' must follow 'begin'");
                            }
                            break;
                        }
                        // Ensure
                        case Phase2TokenType.Ensure: {
                            if (CurrentBlocks.TryPeek(out BuildingBlock? Block) && Block is BuildingBeginBranches BeginBlock) {
                                ResolveStatementsWithoutEndingBlock();
                                if (BeginBlock.Branches[^1] is not (BuildingBegin or BuildingRescue or BuildingRescueElse)) {
                                    throw new SyntaxErrorException($"{Token.Location}: 'Ensure' must follow 'begin', 'rescue' or 'else', not '{BeginBlock.Branches[^1].GetType().Name}'");
                                }
                                BeginBlock.Branches.Add(new BuildingEnsure(Token.Location));
                            }
                            else {
                                throw new SyntaxErrorException($"{Token.Location}: 'Ensure' must follow 'begin'");
                            }
                            break;
                        }
                        // Case
                        case Phase2TokenType.Case: {
                            PushBlock(ParseCase(Token.Location, ParsedObjects, ref i));
                            break;
                        }
                        // When
                        case Phase2TokenType.When: {
                            if (CurrentBlocks.TryPeek(out BuildingBlock? Block) && Block is BuildingCase CaseBlock) {
                                ResolveStatementsWithoutEndingBlock();
                                if (CaseBlock.Branches.Count != 0 && CaseBlock.Branches[^1].Conditions.Count == 0) {
                                    throw new SyntaxErrorException($"{Token.Location}: 'When' must not follow 'else'");
                                }
                                CaseBlock.Branches.Add(ParseWhen(Token.Location, ParsedObjects, ref i));
                            }
                            else {
                                throw new SyntaxErrorException($"{Token.Location}: 'When' must follow 'case'");
                            }
                            break;
                        }
                        // End
                        case Phase2TokenType.End: {
                            if (CurrentBlocks.Count == 1) {
                                throw new SyntaxErrorException($"{Token.Location}: Unexpected 'end' statement");
                            }
                            Expression? EndedExpression = ResolveEndBlock(false);
                            if (EndedExpression != null) AddPendingObject(EndedExpression);
                            break;
                        }
                        // {
                        case Phase2TokenType.StartCurly: {
                            if (LastUnknownObject is null or Statement or not Expression) {
                                // {} is hash; handle later
                                PushBlock(new BuildingHash(Location));
                                break;
                            }
                            i++;
                            // Get do |arguments|
                            List<MethodArgumentExpression> DoArguments;
                            if (ParsedObjects[i] is Phase2Token Tok && Tok.Type == Phase2TokenType.Pipe) {
                                DoArguments = GetMethodArguments(ParsedObjects, ref i, true, Token.Location);
                                // Add end of statement after pipe arguments
                                ParsedObjects.Insert(i + 1, new Phase2Token(Location, Phase2TokenType.EndOfStatement, null));
                            }
                            else {
                                i--;
                                DoArguments = new();
                            }
                            // Open do block
                            PushBlock(new BuildingDo(Location, DoArguments, true));
                            break;
                        }
                        // }
                        case Phase2TokenType.EndCurly: {
                            if (CurrentBlocks.Count == 1) {
                                throw new SyntaxErrorException($"{Token.Location}: Unexpected '}}'");
                            }
                            else if (CurrentBlocks.Peek() is BuildingHash) {
                                // Get objects inside {}
                                List<Phase2Object> HashContents = PendingObjects.Pop();
                                HashContents.RemoveFromEnd(Item => Item is Phase2Token Tok && Tok.Type == Phase2TokenType.EndOfStatement);
                                // Get items to put in hash
                                List<Expression> HashItemsList = ObjectsToExpressions(HashContents, ExpressionsType.KeyValueExpressions);
                                // Split items into key value pairs
                                LockingDictionary<Expression, Expression> HashItems = HashItemsList.ListAsHash();
                                // Create hash expression and add to expressions
                                CurrentBlocks.Pop();
                                PendingObjects.Peek().Add(new HashExpression(Token.Location, HashItems));
                                break;
                            }
                            Expression? EndedExpression = ResolveEndBlock(true);
                            if (EndedExpression != null) AddPendingObject(EndedExpression);
                            break;
                        }
                        // Other
                        default: {
                            AddPendingObjectAt(i);
                            break;
                        }
                    }
                }
                else {
                    AddPendingObjectAt(i);
                }
            }

            if (CurrentBlocks.Count != 1) {
                throw new SyntaxErrorException($"{CurrentBlocks.Peek().Location}: {(CurrentBlocks.Count == 2 ? "Block was" : $"{CurrentBlocks.Count - 1} blocks were")} never closed with an end statement");
            }

            BuildingBlock TopBlock = CurrentBlocks.Pop();
            ParsedObjects = new List<Phase2Object>(TopBlock.Statements);

            List<Phase2Object> TopPendingObjects = PendingObjects.Pop();
            ParsedObjects.AddRange(TopPendingObjects);

            return ParsedObjects;
        }

        public static List<Expression> ObjectsToExpressions(List<Phase2Object> Phase2Objects, ExpressionsType ExpressionsType) {
            if (Phase2Objects.Count == 0) return new List<Expression>(0);
            List<Phase2Object> ParsedObjects = new(Phase2Objects); // Preserve the original list

            // Object Tokens and Self
            for (int i = 0; i < ParsedObjects.Count; i++) {
                if (ParsedObjects[i] is Phase2Token Token) {
                    // self
                    if (Token.Type == Phase2TokenType.Self) {
                        ParsedObjects[i] = new SelfExpression(Token.Location);
                    }
                    // Token -> ObjectTokenExpression
                    else if (IsVariableToken(Token) || Token.IsObjectToken) {
                        ParsedObjects[i] = new ObjectTokenExpression(Token);
                    }
                }
            }

            // Environment Info
            for (int i = 0; i < ParsedObjects.Count; i++) {
                if (ParsedObjects[i] is Phase2Token Token) {
                    if (Token.Type == Phase2TokenType.__LINE__) {
                        ParsedObjects[i] = new EnvironmentInfoExpression(Token.Location, EnvironmentInfoType.__LINE__);
                    }
                }
            }

            // Get Blocks
            ParsedObjects = GetBlocks(ParsedObjects);

            // Method calls (any)
            void ParseMethodCall(ObjectTokenExpression MethodName, int MethodNameIndex, bool WrappedInBrackets) {
                // Parse arguments
                int EndOfArgumentsIndex = MethodNameIndex;
                List<Expression>? Arguments = ParseArguments(ParsedObjects, ref EndOfArgumentsIndex, WrappedInBrackets ? ArgumentParentheses.Brackets : ArgumentParentheses.NoBrackets);

                // Add method call
                if (Arguments != null) {
                    ParsedObjects.RemoveIndexRange(MethodNameIndex, EndOfArgumentsIndex);
                    MethodCallExpression MethodCall = new(MethodName, Arguments);
                    ParsedObjects.Insert(MethodNameIndex, MethodCall);
                }
            }
            // Method calls (with brackets)
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object UnknownObject = ParsedObjects[i];

                if (UnknownObject is ObjectTokenExpression ObjectToken && (ObjectToken.Token.Type is Phase2TokenType.LocalVariableOrMethod or Phase2TokenType.ConstantOrMethod)) {
                    if (i + 1 < ParsedObjects.Count && ParsedObjects[i + 1] is Phase2Token NextToken && NextToken.Type == Phase2TokenType.OpenBracket) {
                        ParseMethodCall(ObjectToken, i, true);
                    }
                }
            }

            // Brackets
            {
                Stack<int> BracketsStack = new();
                for (int i = 0; i < ParsedObjects.Count; i++) {
                    Phase2Object Object = ParsedObjects[i];

                    if (Object is Phase2Token Token) {
                        if (Token.Type == Phase2TokenType.OpenBracket) {
                            BracketsStack.Push(i);
                        }
                        else if (Token.Type == Phase2TokenType.CloseBracket) {
                            if (BracketsStack.TryPop(out int OpenBracketIndex)) {
                                Expression BracketsExpression = ObjectsToExpression(ParsedObjects.GetIndexRange(OpenBracketIndex + 1, i - 1));
                                ParsedObjects.RemoveIndexRange(OpenBracketIndex, i);
                                ParsedObjects.Insert(OpenBracketIndex, BracketsExpression);
                                i = OpenBracketIndex;
                            }
                            else {
                                throw new SyntaxErrorException($"{Token.Location}: Unexpected close bracket");
                            }
                        }
                    }
                }
                if (BracketsStack.TryPop(out int RemainingOpenBracketIndex)) {
                    throw new SyntaxErrorException($"{ParsedObjects[RemainingOpenBracketIndex].Location}: Unclosed bracket");
                }
            }

            // Do expressions
            void HandleDoExpression(ref int Index) {
                DoExpression DoExpression = (DoExpression)ParsedObjects[Index];

                Phase2Object? LastObject = Index - 1 >= 0 ? ParsedObjects[Index - 1] : null;

                if (LastObject != null) {
                    // Set on yield for previously known method call
                    if (LastObject is MethodCallExpression LastMethodCallExpression) {
                        LastMethodCallExpression.OnYield = DoExpression.OnYield;
                    }
                    // Set on yield for LocalVariableOrMethod/ConstantOrMethod which we now know is a method call
                    else if (LastObject is ObjectTokenExpression LastObjectTokenExpression) {
                        if (LastObjectTokenExpression.Token.Type is Phase2TokenType.LocalVariableOrMethod or Phase2TokenType.ConstantOrMethod) {
                            // Create method call from LocalVariableOrMethod/ConstantOrMethod
                            MethodCallExpression DeducedMethodCallExpression = new(LastObjectTokenExpression, null, DoExpression.OnYield);
                            ParsedObjects[Index - 1] = DeducedMethodCallExpression;
                        }
                        else {
                            throw new SyntaxErrorException($"{DoExpression.Location}: Do block must follow method call, not {LastObjectTokenExpression.Token.Type}");
                        }
                    }
                    else {
                        throw new SyntaxErrorException($"{DoExpression.Location}: Do block must follow method call, not {LastObject.GetType().Name}");
                    }
                }
                else {
                    throw new SyntaxErrorException($"{DoExpression.Location}: Do block must follow method call");
                }

                // Remove handled DoExpression
                ParsedObjects.RemoveAt(Index);
                Index--;
            }
            // Do { ... }
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object Object = ParsedObjects[i];

                if (Object is DoExpression DoExpression && DoExpression.HighPriority) {
                    HandleDoExpression(ref i);
                }
            }

            // Paths, Arrays & Indexers
            void ParsePathsArraysAndIndexers(bool HandleIndexEquals) {
                Stack<int> SquareBracketsStack = new();
                for (int i = 0; i < ParsedObjects.Count; i++) {
                    Phase2Object? LastObject = i - 1 >= 0 ? ParsedObjects[i - 1] : null;
                    Phase2Object Object = ParsedObjects[i];
                    Phase2Object? NextObject = i + 1 < ParsedObjects.Count ? ParsedObjects[i + 1] : null;
                    Phase2Object? NextNextObject = i + 2 < ParsedObjects.Count ? ParsedObjects[i + 2] : null;

                    if (Object is Phase2Token Token) {
                        // Path or Constant Path
                        if (Token.Type is Phase2TokenType.Dot or Phase2TokenType.DoubleColon) {
                            if (LastObject != null) {
                                // e.g. A + . + b = A.b
                                if (LastObject is Expression LastExpression && NextObject is ObjectTokenExpression NextToken) {
                                    if (!(NextToken.Token.Type == Phase2TokenType.ConstantOrMethod
                                        || (Token.Type == Phase2TokenType.Dot && NextToken.Token.Type == Phase2TokenType.LocalVariableOrMethod)))
                                    {
                                        throw new SyntaxErrorException($"{NextToken.Location}: Expected identifier after '.', got {NextToken.Inspect()}");
                                    }

                                    ParsedObjects.RemoveRange(i - 1, 3);
                                    ParsedObjects.Insert(i - 1, Token.Type == Phase2TokenType.Dot
                                        ? new PathExpression(LastExpression, NextToken.Token)
                                        : new ConstantPathExpression(LastExpression, NextToken.Token));
                                    i -= 2;
                                }
                                // e.g. A + . + b() = A.b()
                                else if (LastObject is Expression LastObjectExpression && NextObject is MethodCallExpression NextMethodCall) {
                                    if (NextMethodCall.MethodPath is PathExpression) {
                                        throw new InternalErrorException($"{NextMethodCall.MethodPath.Location}: Method call name following '.' should not be path expression (got {NextMethodCall.MethodPath.Inspect()})");
                                    }
                                    else if (NextMethodCall.MethodPath is not ObjectTokenExpression) {
                                        throw new RuntimeException($"{NextMethodCall.MethodPath.Location}: Method call name following '.' should be object token expression (got {NextMethodCall.MethodPath.Inspect()})");
                                    }
                                    ParsedObjects.RemoveRange(i - 1, 2);
                                    NextMethodCall.MethodPath = new PathExpression(LastObjectExpression, ((ObjectTokenExpression)NextMethodCall.MethodPath).Token);
                                    i -= 2;
                                }
                                else {
                                    if (LastObject is not Expression) {
                                        throw new SyntaxErrorException($"{Token.Location}: Expected expression before '{Token.Value!}' (got {LastObject.Inspect()})");
                                    }
                                    else {
                                        throw new SyntaxErrorException($"{Token.Location}: Expected identifier after '{Token.Value!}' (got {(NextObject != null ? NextObject.Inspect() : "nothing")})");
                                    }
                                }
                            }
                            else {
                                throw new SyntaxErrorException($"{Token.Location}: Expected a value before '{Token.Value!}'");
                            }
                        }

                        // Arrays & Indexers
                        else if (Token.Type == Phase2TokenType.StartSquare) {
                            SquareBracketsStack.Push(i);
                        }
                        else if (Token.Type == Phase2TokenType.EndSquare) {
                            if (SquareBracketsStack.TryPop(out int OpenBracketIndex)) {
                                // Check whether [] is array or indexer
                                Expression? IsIndexer = null;
                                if (OpenBracketIndex - 1 >= 0) {
                                    Phase2Object OriginalLastObject = ParsedObjects[OpenBracketIndex - 1];
                                    if (OriginalLastObject is Expression OriginalLastExpression && OriginalLastObject is not Statement) {
                                        if (OriginalLastExpression is not ObjectTokenExpression || !((Phase2Token)ParsedObjects[OpenBracketIndex]).FollowsWhitespace) {
                                            IsIndexer = OriginalLastExpression;
                                        }
                                    }
                                }
                                // Check whether [] is []=
                                Expression? IsIndexEquals = null;
                                if (IsIndexer != null && NextObject is Phase2Token NextToken && NextToken.Type == Phase2TokenType.AssignmentOperator && NextToken.Value == "=") {
                                    if (!HandleIndexEquals) {
                                        // Handle index equals later (low priority)
                                        continue;
                                    }
                                    if (NextNextObject is Expression IndexAssignmentValue) {
                                        IsIndexEquals = IndexAssignmentValue;
                                        // Remove equals and value
                                        ParsedObjects.RemoveRange(i + 1, 2);
                                    }
                                    else {
                                        throw new SyntaxErrorException($"{Token.Location}: Expected value after []=, got '{NextNextObject?.Inspect()}'");
                                    }
                                }

                                // Get objects enclosed in []
                                List<Phase2Object> EnclosedObjects = ParsedObjects.GetIndexRange(OpenBracketIndex + 1, i - 1);
                                // Remove objects from objects list
                                ParsedObjects.RemoveIndexRange(OpenBracketIndex, i);

                                // Indexer
                                if (IsIndexer != null) {
                                    // Get index
                                    List<Expression> Index = ObjectsToExpressions(EnclosedObjects, ExpressionsType.CommaSeparatedExpressions);
                                    // []=
                                    if (IsIndexEquals != null) {
                                        // Create index equals expression
                                        Index.Add(IsIndexEquals);
                                        ParsedObjects.Insert(OpenBracketIndex, new MethodCallExpression(
                                            new PathExpression(IsIndexer, new Phase2Token(ParsedObjects[OpenBracketIndex - 1].Location, Phase2TokenType.LocalVariableOrMethod, "[]=")),
                                            Index
                                        ));
                                    }
                                    // []
                                    else {
                                        // Create indexer expression
                                        ParsedObjects.Insert(OpenBracketIndex, new MethodCallExpression(
                                            new PathExpression(IsIndexer, new Phase2Token(ParsedObjects[OpenBracketIndex - 1].Location, Phase2TokenType.LocalVariableOrMethod, "[]")),
                                            Index
                                        ));
                                    }
                                    // Remove object before []
                                    ParsedObjects.RemoveAt(OpenBracketIndex - 1);
                                    OpenBracketIndex--;
                                }
                                // Array
                                else {
                                    // Get items to put in array
                                    List<Expression> ArrayItems = ObjectsToExpressions(EnclosedObjects, ExpressionsType.CommaSeparatedExpressions);
                                    // Create array expression
                                    ParsedObjects.Insert(OpenBracketIndex, new ArrayExpression(Token.Location, ArrayItems));
                                }
                                // Move on
                                i = OpenBracketIndex;
                            }
                            else {
                                throw new SyntaxErrorException($"{Token.Location}: Unexpected square close bracket");
                            }
                        }
                        else if (StartParenthesesTokens.Contains(Token.Type)) {
                            // Skip nested parentheses
                            SkipParentheses(ParsedObjects, ref i);
                        }
                    }
                }
                // Check for unclosed arrays/indexers
                if (SquareBracketsStack.TryPop(out int RemainingOpenBracketIndex)) {
                    throw new SyntaxErrorException($"{ParsedObjects[RemainingOpenBracketIndex].Location}: Unclosed square bracket");
                }
            }
            ParsePathsArraysAndIndexers(false);

            // Unary
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object UnknownObject = ParsedObjects[i];
                Phase2Object? LastUnknownObject = i - 1 >= 0 ? ParsedObjects[i - 1] : null;
                Phase2Object? NextUnknownObject = i + 1 < ParsedObjects.Count ? ParsedObjects[i + 1] : null;

                if (UnknownObject is Phase2Token Token) {
                    if (Token.Type == Phase2TokenType.Operator && (Token.Value! == "+" || Token.Value! == "-")) {
                        // Get next object token
                        ObjectTokenExpression? NextObjectToken = null;
                        if (NextUnknownObject is ObjectTokenExpression NextExpression) {
                            if (NextExpression is PathExpression NextPathExpression) {
                                if (NextPathExpression.ParentObject is ObjectTokenExpression NextPathObjectExpression) {
                                    NextObjectToken = NextPathObjectExpression;
                                }
                            }
                            else if (NextExpression is ObjectTokenExpression NextObjectExpression) {
                                NextObjectToken = NextObjectExpression;
                            }
                        }
                        else if (NextUnknownObject is MethodCallExpression NextMethodExpression) {
                            if (NextMethodExpression.MethodPath is PathExpression NextMethodPathExpression) {
                                if (NextMethodPathExpression.ParentObject is ObjectTokenExpression NextMethodPathObjectExpression) {
                                    NextObjectToken = NextMethodPathObjectExpression;
                                }
                            }
                            else if (NextMethodExpression.MethodPath is ObjectTokenExpression NextMethodObjectExpression) {
                                NextObjectToken = NextMethodObjectExpression;
                            }
                        }

                        // Add method call expression for unary operator
                        if (NextObjectToken != null) {
                            /*If previous object is not an expression, then it's unary.
                              If previous object is a method and there's a space before the operator and no space after the operator, then it's unary.
                              Otherwise, it's add/sub.*/
                            ObjectTokenExpression? LastObjectToken = LastUnknownObject as ObjectTokenExpression;
                            bool LastObjectIsMethod = (LastObjectToken != null && LastObjectToken.Token.Type is Phase2TokenType.LocalVariableOrMethod or Phase2TokenType.ConstantOrMethod) || LastUnknownObject is MethodCallExpression;
                            bool SpaceBeforeOperator = Token.FollowsWhitespace;
                            bool SpaceAfterOperator = Token.FollowedByWhitespace;
                            if ((LastObjectIsMethod && SpaceBeforeOperator && !SpaceAfterOperator) || LastUnknownObject is not Expression) {
                                ParsedObjects.RemoveRange(i, 2);
                                ParsedObjects.Insert(i, new MethodCallExpression(
                                    new PathExpression(NextObjectToken, new Phase2Token(NextObjectToken.Location, Phase2TokenType.LocalVariableOrMethod, Token.Value! + "@")),
                                    null
                                ));
                            }
                        }
                    }
                }
            }

            // Ranges
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object UnknownObject = ParsedObjects[i];
                Phase2Object? LastUnknownObject = i - 1 >= 0 ? ParsedObjects[i - 1] : null;
                Phase2Object? NextUnknownObject = i + 1 < ParsedObjects.Count ? ParsedObjects[i + 1] : null;

                if (UnknownObject is Phase2Token Token) {
                    if (Token.Type == Phase2TokenType.InclusiveRange || Token.Type == Phase2TokenType.ExclusiveRange) {
                        Expression? Min = LastUnknownObject != null && LastUnknownObject is Expression LastExpression ? LastExpression : null;
                        Expression? Max = NextUnknownObject != null && NextUnknownObject is Expression NextExpression ? NextExpression : null;

                        if (Min != null || Max != null) {
                            if (Min != null && Max != null) {
                                i--;
                                ParsedObjects.RemoveRange(i, 3);
                            }
                            else if (Min != null) {
                                i--;
                                ParsedObjects.RemoveRange(i, 2);
                            }
                            else if (Max != null) {
                                ParsedObjects.RemoveRange(i, 2);
                            }
                            ParsedObjects.Insert(i, new RangeExpression(Token.Location, Min, Max, Token.Type == Phase2TokenType.InclusiveRange));
                        }
                        else {
                            throw new SyntaxErrorException($"{Token.Location}: Range operator '{Token.Value!}' must be next to at least one expression (got {LastUnknownObject?.Inspect()} and {NextUnknownObject?.Inspect()})");
                        }
                    }
                }
            }

            // Defined?
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object UnknownObject = ParsedObjects[i];

                if (UnknownObject is Phase2Token Token) {
                    // defined?
                    if (Token.Type == Phase2TokenType.Defined) {
                        int EndOfArgumentsIndex = i;
                        List<Expression>? Arguments = ParseArguments(ParsedObjects, ref EndOfArgumentsIndex, OneOnly: true);
                        if (Arguments != null) {
                            if (Arguments.Count != 1) {
                                throw new SyntaxErrorException($"{Token.Location}: Expected a single argument after defined?");
                            }
                            ParsedObjects.RemoveIndexRange(i, EndOfArgumentsIndex);
                            ParsedObjects.Insert(i, new DefinedExpression(Token.Location, Arguments[0]));
                        }
                        else {
                            throw new SyntaxErrorException($"{Token.Location}: Expected expression after defined?");
                        }
                    }
                }
            }

            // Operators (any)
            void HandleOperators(string[][] Precedence) {
                foreach (string[] Operators in Precedence) {
                    for (int i = 0; i < ParsedObjects.Count; i++) {
                        Phase2Object UnknownObject = ParsedObjects[i];
                        Phase2Object? LastUnknownObject = i - 1 >= 0 ? ParsedObjects[i - 1] : null;
                        Phase2Object? NextUnknownObject = i + 1 < ParsedObjects.Count ? ParsedObjects[i + 1] : null;

                        if (UnknownObject is Phase2Token Token) {
                            if (Token.Type == Phase2TokenType.Operator && Operators.Contains(Token.Value!)) {
                                if (Token.Value is "not" or "!") {
                                    if (NextUnknownObject is Expression NextExpression) {
                                        ParsedObjects.RemoveRange(i, 2);
                                        ParsedObjects.Insert(i, new NotExpression(Token.Location, NextExpression));
                                    }
                                    else {
                                        throw new SyntaxErrorException($"{Token.Location}: Operator '{Token.Value!}' must be before an expression (got {NextUnknownObject?.Inspect()})");
                                    }
                                }
                                else if (LastUnknownObject is Expression LastExpression && NextUnknownObject is Expression NextExpression) {
                                    i--;
                                    ParsedObjects.RemoveRange(i, 3);
                                    if (NonMethodOperators.Contains(Token.Value!)) {
                                        LogicalExpression.LogicalExpressionType LogicType = Token.Value! switch {
                                            "and" or "&&" => LogicalExpression.LogicalExpressionType.And,
                                            "or" or "||" => LogicalExpression.LogicalExpressionType.Or,
                                            "^" => LogicalExpression.LogicalExpressionType.Xor,
                                            _ => throw new InternalErrorException($"{Token.Location}: Unhandled logic expression type: '{Token.Value!}'")
                                        };
                                        ParsedObjects.Insert(i, new LogicalExpression(LastExpression.Location, LogicType, LastExpression, NextExpression));
                                    }
                                    else {
                                        ParsedObjects.Insert(i, new MethodCallExpression(
                                            new PathExpression(LastExpression, new Phase2Token(Token.Location, Phase2TokenType.LocalVariableOrMethod, Token.Value!)),
                                            new List<Expression>() { NextExpression })
                                        );
                                    }
                                }
                                else {
                                    throw new SyntaxErrorException($"{Token.Location}: Operator '{Token.Value!}' must be between two expressions (got {LastUnknownObject?.Inspect()} and {NextUnknownObject?.Inspect()})");
                                }
                            }
                        }
                    }
                }
            }
            // Operators (normal priority)
            HandleOperators(NormalOperatorPrecedence);

            // Alias
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object UnknownObject = ParsedObjects[i];
                Phase2Object? NextUnknownObject = i + 1 < ParsedObjects.Count ? ParsedObjects[i + 1] : null;
                Phase2Object? NextNextUnknownObject = i + 2 < ParsedObjects.Count ? ParsedObjects[i + 2] : null;

                if (UnknownObject is Phase2Token Token) {
                    // alias
                    if (Token.Type == Phase2TokenType.Alias) {
                        if (NextUnknownObject is ObjectTokenExpression NextObjectToken) {
                            if (NextNextUnknownObject is ObjectTokenExpression NextNextObjectToken) {
                                ParsedObjects.RemoveRange(i, 3);
                                ParsedObjects.Insert(i, new AliasStatement(Token.Location, NextObjectToken, NextNextObjectToken));
                            }
                            else {
                                throw new SyntaxErrorException($"{NextUnknownObject.Location}: Expected method to alias after method alias, got '{NextNextUnknownObject?.Inspect()}'");
                            }
                        }
                        else {
                            throw new SyntaxErrorException($"{UnknownObject.Location}: Expected method alias after 'alias', got '{NextUnknownObject?.Inspect()}'");
                        }
                    }
                }
            }

            // Method calls (no brackets)
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object UnknownObject = ParsedObjects[i];

                if (UnknownObject is ObjectTokenExpression ObjectToken && (ObjectToken.Token.Type is Phase2TokenType.LocalVariableOrMethod or Phase2TokenType.ConstantOrMethod)) {
                    ParseMethodCall(ObjectToken, i, false);
                }
            }

            // Do ... end
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object Object = ParsedObjects[i];

                if (Object is DoExpression DoExpression && !DoExpression.HighPriority) {
                    HandleDoExpression(ref i);
                }
            }

            // Ternary
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object? LastUnknownObject = i - 1 >= 0 ? ParsedObjects[i - 1] : null;
                Phase2Object UnknownObject = ParsedObjects[i];
                Phase2Object? NextUnknownObject = i + 1 < ParsedObjects.Count ? ParsedObjects[i + 1] : null;
                Phase2Object? NextNextUnknownObject = i + 2 < ParsedObjects.Count ? ParsedObjects[i + 2] : null;
                Phase2Object? NextNextNextUnknownObject = i + 3 < ParsedObjects.Count ? ParsedObjects[i + 3] : null;

                if (UnknownObject is Phase2Token Token && Token.Type is Phase2TokenType.TernaryQuestion) {
                    if (LastUnknownObject is Expression Condition) {
                        if (NextUnknownObject is Expression ExpressionIfTrue) {
                            if (NextNextUnknownObject is Phase2Token Tok && Tok.Type == Phase2TokenType.TernaryElse) {
                                if (NextNextNextUnknownObject is Expression ExpressionIfFalse) {
                                    // Remove five objects
                                    i--;
                                    ParsedObjects.RemoveRange(i, 5);
                                    // Insert ternary expression
                                    ParsedObjects.Insert(i, new TernaryExpression(Token.Location, Condition, ExpressionIfTrue, ExpressionIfFalse));
                                }
                                else {
                                    throw new SyntaxErrorException($"{Token.Location}: Expected expression after ':', got '{NextNextNextUnknownObject?.Inspect()}'");
                                }
                            }
                            else {
                                throw new SyntaxErrorException($"{Token.Location}: Expected ':', got '{NextUnknownObject?.Inspect()}'");
                            }
                        }
                        else {
                            throw new SyntaxErrorException($"{Token.Location}: Expected expression after '?', got '{NextUnknownObject?.Inspect()}'");
                        }
                    }
                    else {
                        throw new SyntaxErrorException($"{Token.Location}: Invalid ternary condition");
                    }
                }
            }

            // Operators (low priority)
            HandleOperators(LowPriorityOperatorPrecedence);

            // []=
            ParsePathsArraysAndIndexers(true);

            // Assignment
            for (int i = ParsedObjects.Count - 1; i >= 0; i--) {
                Phase2Object UnknownObject = ParsedObjects[i];
                Phase2Object? LastUnknownObject = i - 1 >= 0 ? ParsedObjects[i - 1] : null;
                Phase2Object? LastLastUnknownObject = i - 2 >= 0 ? ParsedObjects[i - 2] : null;
                Phase2Object? NextUnknownObject = i + 1 < ParsedObjects.Count ? ParsedObjects[i + 1] : null;

                if (UnknownObject is Phase2Token Token) {
                    if (Token.Type == Phase2TokenType.AssignmentOperator) {
                        if (LastUnknownObject != null && NextUnknownObject != null && LastUnknownObject is ObjectTokenExpression LastExpression && NextUnknownObject is Expression NextExpression) {
                            // Check assignment is not multiple assignment
                            if (LastLastUnknownObject == null || !(LastLastUnknownObject is Phase2Token LastLastToken && LastLastToken.Type == Phase2TokenType.Comma)) {
                                // Remove objects
                                i--;
                                ParsedObjects.RemoveRange(i, 3);
                                // Create assignment expression
                                ParsedObjects.Insert(i, new AssignmentExpression(
                                    LastExpression,
                                    Token.Value!,
                                    NextExpression
                                ));
                            }
                        }
                        else {
                            throw new SyntaxErrorException($"{Token.Location}: Assignment operator '{Token.Value}' must be between two expressions (got {(LastUnknownObject != null ? $"'{LastUnknownObject?.Inspect()}'" : "nothing")} and {(NextUnknownObject != null ? $"'{NextUnknownObject?.Inspect()}'" : "nothing")})");
                        }
                    }
                }
            }

            // Multiple Assignment
            for (int i = ParsedObjects.Count - 1; i >= 0; i--) {
                Phase2Object UnknownObject = ParsedObjects[i];

                if (UnknownObject is Phase2Token Token && Token.Type == Phase2TokenType.AssignmentOperator) {
                    // Remove assignment operator token
                    ParsedObjects.RemoveAt(i);
                    // Get assignment variables
                    i--;
                    List<ObjectTokenExpression> AssignmentVariables = GetCommaSeparatedExpressions<ObjectTokenExpression>(ParsedObjects, ref i, Backwards: true);
                    AssignmentVariables.Reverse();
                    // Get assignment values
                    i++;
                    List<Expression> AssignmentValues = GetCommaSeparatedExpressions(ParsedObjects, ref i);
                    // Check variables and values match up
                    if (AssignmentVariables.Count != AssignmentValues.Count && !(AssignmentVariables.Count > 1 && AssignmentValues.Count == 1))
                        throw new SyntaxErrorException($"{Token.Location}: Number of assignment variables is different to number of assignment values ({AssignmentVariables.Count} vs {AssignmentValues.Count})");
                    // Create multiple assignment expression
                    ParsedObjects.Insert(i, new MultipleAssignmentExpression(
                        AssignmentVariables,
                        Token.Value!,
                        AssignmentValues
                    ));
                }
            }
            
            // Return / Yield / Super / Break / Undef
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object UnknownObject = ParsedObjects[i];
                
                if (UnknownObject is Phase2Token Token) {
                    switch (Token.Type) {
                        // Return
                        case Phase2TokenType.Return: {
                            int StartIndex = i;
                            ParsedObjects[StartIndex] = ParseReturnOrYieldOrSuper(Token.Location, ParsedObjects, ref i, ReturnOrYieldOrSuper.Return);
                            ParsedObjects.RemoveIndexRange(StartIndex + 1, i);
                            break;
                        }
                        // Yield
                        case Phase2TokenType.Yield: {
                            int StartIndex = i;
                            ParsedObjects[StartIndex] = ParseReturnOrYieldOrSuper(Token.Location, ParsedObjects, ref i, ReturnOrYieldOrSuper.Yield);
                            ParsedObjects.RemoveIndexRange(StartIndex + 1, i);
                            break;
                        }
                        // Super
                        case Phase2TokenType.Super: {
                            int StartIndex = i;
                            ParsedObjects[StartIndex] = ParseReturnOrYieldOrSuper(Token.Location, ParsedObjects, ref i, ReturnOrYieldOrSuper.Super);
                            ParsedObjects.RemoveIndexRange(StartIndex + 1, i);
                            break;
                        }
                        // Break
                        case Phase2TokenType.Break: {
                            ParsedObjects[i] = new LoopControlStatement(Token.Location, LoopControlType.Break);
                            break;
                        }
                        // Retry
                        case Phase2TokenType.Retry: {
                            ParsedObjects[i] = new LoopControlStatement(Token.Location, LoopControlType.Retry);
                            break;
                        }
                        // Redo
                        case Phase2TokenType.Redo: {
                            ParsedObjects[i] = new LoopControlStatement(Token.Location, LoopControlType.Redo);
                            break;
                        }
                        // Next
                        case Phase2TokenType.Next: {
                            ParsedObjects[i] = new LoopControlStatement(Token.Location, LoopControlType.Next);
                            break;
                        }
                        // Undef
                        case Phase2TokenType.Undef: {
                            int StartIndex = i;
                            ParsedObjects[StartIndex] = ParseUndef(Token.Location, ParsedObjects, ref i);
                            ParsedObjects.RemoveIndexRange(StartIndex + 1, i);
                            break;
                        }
                    }
                }
            }

            // Statement if/unless/while/until condition or statement rescue statement
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object UnknownObject = ParsedObjects[i];

                if (UnknownObject is Phase2Token Token) {
                    if (Token.Type is Phase2TokenType.If or Phase2TokenType.Unless or Phase2TokenType.While or Phase2TokenType.Until or Phase2TokenType.Rescue) {
                        if (i - 1 >= 0 && ParsedObjects[i - 1] is Expression Statement) {
                            if (i + 1 < ParsedObjects.Count && ParsedObjects[i + 1] is Expression Condition) {
                                // Remove three expressions
                                i--;
                                ParsedObjects.RemoveRange(i, 3);
                                // Get statement & conditional expression
                                if (Token.Type == Phase2TokenType.Rescue) {
                                    // Insert rescue expression
                                    ParsedObjects.Insert(i, new RescueExpression(Condition, Statement));
                                }
                                else {
                                    List<Expression> ConditionalStatement = new() { Statement };
                                    ConditionalExpression ConditionalExpression;
                                    if (Token.Type == Phase2TokenType.If) {
                                        ConditionalExpression = new IfExpression(Statement.Location, Condition, ConditionalStatement);
                                    }
                                    else if (Token.Type == Phase2TokenType.Unless) {
                                        ConditionalExpression = new IfExpression(Statement.Location, Condition, ConditionalStatement, true);
                                    }
                                    else if (Token.Type == Phase2TokenType.While) {
                                        ConditionalExpression = new WhileExpression(Statement.Location, Condition, ConditionalStatement);
                                    }
                                    else if (Token.Type == Phase2TokenType.Until) {
                                        ConditionalExpression = new WhileExpression(Statement.Location, Condition, ConditionalStatement, true);
                                    }
                                    else {
                                        throw new SyntaxErrorException($"{Statement.Location}: Unrecognised token for one-line statement: '{Token.Type}'");
                                    }
                                    // Insert conditional expression
                                    ParsedObjects.Insert(i, ConditionalExpression);
                                }
                            }
                            else {
                                throw new SyntaxErrorException($"{Statement.Location}: Expected condition after '{Token.Type}'");
                            }
                        }
                        else {
                            throw new InternalErrorException($"{Token.Location}: Unhandled {Token.Type} statement");
                        }
                    }
                }
            }

            // Cast objects to expressions
            {
                List<Expression> Expressions = new();

                // Remove EndOfStatements
                // ParsedObjects = ParsedObjects.Where(Object => !(Object is Phase2Token Tok && Tok.Type == Phase2TokenType.EndOfStatement)).ToList();

                // Single expression
                if (ExpressionsType == ExpressionsType.SingleExpression) {
                    // ParsedObjects.RemoveFromEnd(Item => Item is Phase2Token Tok && Tok.Type == Phase2TokenType.EndOfStatement);
                    if (ParsedObjects.Count == 1 && ParsedObjects[0] is Expression SingleExpression) {
                        Expressions.Add(SingleExpression);
                    }
                    else if (ParsedObjects.Count == 0) {
                        throw new SyntaxErrorException($"{ParsedObjects.Location()}: Expected single expression, got nothing");
                    }
                    else {
                        throw new SyntaxErrorException($"{ParsedObjects.Location()}: Expected single expression, got '{ParsedObjects.Inspect()}'");
                    }
                }
                // Statements / expressions
                else if (ExpressionsType == ExpressionsType.Statements) {
                    for (int i = 0; i < ParsedObjects.Count; i++) {
                        Phase2Object ParsedObject = ParsedObjects[i];

                        if (ParsedObject is Expression ParsedExpression) {
                            Expressions.Add(ParsedExpression);
                        }
                        else if (ParsedObject is Phase2Token ParsedToken && ParsedToken.Type == Phase2TokenType.EndOfStatement) {
                        }
                        else {
                            throw new SyntaxErrorException($"{ParsedObject.Location}: Expected expression, got {ParsedObject.Inspect()}");
                        }
                    }
                }
                // Key value (hash) expressions
                else if (ExpressionsType == ExpressionsType.KeyValueExpressions) {
                    int Phase = 0; // 0: Accept item only, 1: Expect right arrow, 2: Expect item, 3: Accept comma or end only
                    DebugLocation? SyntaxErrorLocation = null;

                    for (int i = 0; i < ParsedObjects.Count; i++) {
                        Phase2Object ParsedObject = ParsedObjects[i];
                        SyntaxErrorLocation = null;

                        if (ParsedObject is Statement ParsedStatement) {
                            throw new SyntaxErrorException($"{ParsedObject.Location}: Unexpected statement: {ParsedStatement.Inspect()}");
                        }
                        else if (ParsedObject is Expression ParsedExpression && (Phase is 0 or 2)) {
                            Expressions.Add(ParsedExpression);
                            Phase++;
                            if (Phase == 1) {
                                SyntaxErrorLocation = ParsedExpression.Location;
                            }
                        }
                        else if (ParsedObject is Phase2Token ParsedComma && ParsedComma.Type == Phase2TokenType.Comma && Phase == 3) {
                            Phase = 0;
                        }
                        else if (ParsedObject is Phase2Token ParsedRightArrow && ParsedRightArrow.Type == Phase2TokenType.RightArrow && Phase == 1) {
                            Phase = 2;
                            SyntaxErrorLocation = ParsedRightArrow.Location;
                        }
                        else {
                            throw new SyntaxErrorException($"{ParsedObject.Location}: Unexpected '{ParsedObject.Inspect()}'");
                        }
                    }

                    if (Phase == 1) {
                        throw new SyntaxErrorException($"{SyntaxErrorLocation}: Expected value after key");
                    }
                    else if (Phase == 2) {
                        throw new SyntaxErrorException($"{SyntaxErrorLocation}: Expected value after right arrow");
                    }
                }
                // Comma-separated expressions
                else {
                    bool AcceptComma = false;
                    DebugLocation? CommaLocation = null;

                    for (int i = 0; i < ParsedObjects.Count; i++) {
                        Phase2Object ParsedObject = ParsedObjects[i];

                        if (ParsedObject is Statement ParsedStatement) {
                            throw new SyntaxErrorException($"{ParsedObject.Location}: Unexpected statement: {ParsedStatement.Inspect()}");
                        }
                        else if (ParsedObject is Expression ParsedExpression && !AcceptComma) {
                            Expressions.Add(ParsedExpression);
                            AcceptComma = true;
                            CommaLocation = null;
                        }
                        else if (ParsedObject is Phase2Token ParsedToken && ParsedToken.Type == Phase2TokenType.Comma && AcceptComma) {
                            AcceptComma = false;
                            CommaLocation = ParsedToken.Location;
                        }
                        else if (ParsedObject is Phase2Token OtherParsedToken && OtherParsedToken.Type == Phase2TokenType.EndOfStatement) {
                        }
                        else {
                            throw new SyntaxErrorException($"{ParsedObject.Location}: Unexpected '{ParsedObject.Inspect()}'");
                        }
                    }
                    
                    if (CommaLocation != null && Expressions.Count == 0) {
                        throw new SyntaxErrorException($"{CommaLocation}: Expected expression before comma");
                    }
                }

                return Expressions;
            }
        }
        public static Expression ObjectsToExpression(List<Phase2Object> Phase2Objects) {
            List<Expression> Expressions = ObjectsToExpressions(Phase2Objects, ExpressionsType.SingleExpression);
            if (Expressions.Count != 1)
                throw new InternalErrorException($"{Phase2Objects.Location()}: ObjectsToExpression should return single expression, got {Expressions.Count} expressions ({Expressions.Inspect()})");
            return Expressions[0];
        }
        public static List<Expression> ObjectsToExpressions(List<Phase1Token> Phase1Tokens, ExpressionsType ExpressionsType) {
            List<Phase2Token> Phase2Tokens = TokensToPhase2(Phase1Tokens);
            return ObjectsToExpressions(Phase2Tokens, ExpressionsType);
        }
        public static List<Expression> ObjectsToExpressions(List<Phase2Token> Phase2Tokens, ExpressionsType ExpressionsType) {
            List<Phase2Object> Phase2Objects = new(Phase2Tokens);
            return ObjectsToExpressions(Phase2Objects, ExpressionsType);
        }

        class BuildingBlock {
            public readonly DebugLocation Location;
            public readonly List<Expression> Statements = new();
            public BuildingBlock(DebugLocation location) {
                Location = location;
            }
            public virtual void AddStatement(Expression Statement) {
                Statements.Add(Statement);
            }
        }
        class BuildingMethod : BuildingBlock {
            public readonly ObjectTokenExpression MethodName;
            public readonly List<MethodArgumentExpression> Arguments;
            public BuildingMethod(DebugLocation location, ObjectTokenExpression methodName, List<MethodArgumentExpression> arguments) : base(location) {
                MethodName = methodName;
                Arguments = arguments;
            }
            public int MinArgumentsCount {
                get {
                    int MinCount = 0;
                    for (int i = 0; i < Arguments.Count; i++) {
                        if (Arguments[i].DefaultValue != null) {
                            break;
                        }
                        else if (Arguments[i].SplatType != null) {
                            continue;
                        }
                        MinCount++;
                    }
                    return MinCount;
                }
            }
            public int? MaxArgumentsCount {
                get {
                    int IndexOfSplat = Arguments.FindIndex(arg => arg.SplatType != null);
                    if (IndexOfSplat == -1)
                        return Arguments.Count;
                    else
                        return null;
                }
            }
        }
        class BuildingClass : BuildingBlock {
            public readonly ObjectTokenExpression ClassName;
            public bool IsModule;
            public ObjectTokenExpression? InheritsFrom;
            public BuildingClass(DebugLocation location, ObjectTokenExpression className, bool isModule, ObjectTokenExpression? inheritsFrom) : base(location) {
                ClassName = className;
                IsModule = isModule;
                InheritsFrom = inheritsFrom;
            }
        }
        class BuildingDo : BuildingBlock {
            public readonly List<MethodArgumentExpression> Arguments;
            public readonly bool DoIsCurly;
            public BuildingDo(DebugLocation location, List<MethodArgumentExpression> arguments, bool doIsCurly) : base(location) {
                Arguments = arguments;
                DoIsCurly = doIsCurly;
            }
        }
        class BuildingHash : BuildingBlock {
            public BuildingHash(DebugLocation location) : base(location) { }
        }
        abstract class BuildingConditional : BuildingBlock {
            public readonly bool Inverse;
            public readonly Expression? Condition;
            public BuildingConditional(DebugLocation location, Expression? condition, bool inverse = false) : base(location) {
                Inverse = inverse;
                Condition = condition;
            }
        }
        class BuildingIf : BuildingConditional {
            public BuildingIf(DebugLocation location, Expression? condition, bool inverse = false) : base(location, condition, inverse) { }
        }
        class BuildingWhile : BuildingConditional {
            public BuildingWhile(DebugLocation location, Expression condition, bool inverse = false) : base(location, condition, inverse) { }
        }
        class BuildingIfBranches : BuildingBlock {
            public readonly List<BuildingIf> Branches;
            public BuildingIfBranches(DebugLocation location, List<BuildingIf> branches) : base(location) {
                Branches = branches;
            }
            public override void AddStatement(Expression Statement) {
                Branches[^1].Statements.Add(Statement);
            }
        }
        class BuildingBeginComponent : BuildingBlock {
            public BuildingBeginComponent(DebugLocation location) : base(location) { }
        }
        class BuildingBegin : BuildingBeginComponent {
            public BuildingBegin(DebugLocation location) : base(location) { }
        }
        class BuildingRescue : BuildingBeginComponent {
            public readonly ObjectTokenExpression? Exception;
            public readonly Phase2Token? ExceptionVariable;
            public BuildingRescue(DebugLocation location, ObjectTokenExpression? exception, Phase2Token? exceptionVariable) : base(location) {
                Exception = exception;
                ExceptionVariable = exceptionVariable;
            }
        }
        class BuildingRescueElse : BuildingBeginComponent {
            public BuildingRescueElse(DebugLocation location) : base(location) { }
        }
        class BuildingEnsure : BuildingBeginComponent {
            public BuildingEnsure(DebugLocation location) : base(location) { }
        }
        class BuildingBeginBranches : BuildingBlock {
            public readonly List<BuildingBeginComponent> Branches;
            public BuildingBeginBranches(DebugLocation location, List<BuildingBeginComponent> branches) : base(location) {
                Branches = branches;
            }
            public override void AddStatement(Expression Statement) {
                Branches[^1].Statements.Add(Statement);
            }
        }
        class BuildingCase : BuildingBlock {
            public readonly Expression Subject;
            public readonly List<BuildingWhen> Branches = new();
            public BuildingCase(DebugLocation location, Expression subject) : base(location) {
                Subject = subject;
            }
            public override void AddStatement(Expression Statement) {
                if (Branches.Count != 0) {
                    Branches[^1].Statements.Add(Statement);
                }
                else {
                    throw new SyntaxErrorException($"{Location}: Unexpected '{Statement.Inspect()}'");
                }
            }
        }
        class BuildingWhen : BuildingBlock {
            public readonly List<Expression> Conditions;
            public BuildingWhen(DebugLocation location, List<Expression> conditions) : base(location) {
                Conditions = conditions;
            }
        }
        class BuildingFor : BuildingBlock {
            public readonly List<MethodArgumentExpression> VariableNames;
            public readonly Expression InExpression;
            public BuildingFor(DebugLocation location, List<MethodArgumentExpression> variableNames, Expression inExpression) : base(location) {
                VariableNames = variableNames;
                InExpression = inExpression;
            }
        }
    }
}
