using static Embers.Interpreter;
using static Embers.Phase1;

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

            // Temporary
            Dot,
            DoubleColon,
            Comma,
            SplatOperator,
            OpenBracket,
            CloseBracket,
            StartCurly,
            EndCurly,
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
            {"yield", Phase2TokenType.Yield}
        };
        public readonly static string[][] OperatorPrecedence = new[] {
            new[] {"**"},
            new[] {"*", "/", "%"},
            new[] {"+", "-"},

            new[] {"&"},
            new[] {"|", "^"},
            new[] {">", ">=", "<", "<="},
            new[] {"<=>", "==", "===", "!=", "=~", "!~"},
            new[] {"&&"},
            new[] {"||"},
            new[] {"or", "and"},
        };
        public readonly static string[] NonMethodOperators = new[] {
            "or", "and", "&&", "||"
        };

        public class Phase2Token : Phase2Object {
            public readonly Phase2TokenType Type;
            public readonly string? Value;
            public readonly bool FollowsWhitespace;
            public readonly bool ProcessFormatting;

            public readonly bool IsObjectToken;
            public readonly long ValueAsLong;
            public readonly double ValueAsDouble;

            public Phase2Token(DebugLocation location, Phase2TokenType type, string? value, Phase1Token? fromPhase1Token = null) : base(location) {
                Type = type;
                Value = value;
                if (fromPhase1Token != null) {
                    FollowsWhitespace = fromPhase1Token.FollowsWhitespace;
                    ProcessFormatting = fromPhase1Token.ProcessFormatting;
                }

                IsObjectToken = IsObjectToken(this);
                if (Type == Phase2TokenType.Integer) ValueAsLong = long.Parse(Value!);
                if (Type == Phase2TokenType.Float) ValueAsDouble = double.Parse(Value!);
            }
            public override string Inspect() {
                return Type + (Value != null ? ":" : "") + Value;
            }
            public override string Serialise() {
                return $"new Phase2Token({Location.Serialise()}, Phase2TokenType.{Type}, \"{Value}\", {(FollowsWhitespace ? "true" : "false")})";
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
                return $"new ObjectTokenExpression({Token.Serialise()})";
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
                return $"new PathExpression({ParentObject.Serialise()}, {Token.Serialise()})";
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
                return $"new ConstantPathExpression({ParentObject.Serialise()}, {Token.Serialise()})";
            }
        }
        public class MethodCallExpression : Expression {
            public ObjectTokenExpression MethodPath;
            public readonly List<Expression> Arguments;
            public MethodExpression? OnYield; // do ... end
            public MethodCallExpression(ObjectTokenExpression methodPath, List<Expression>? arguments, MethodExpression? onYield = null) : base(methodPath.Location) {
                MethodPath = methodPath;
                Arguments = arguments ?? new List<Expression>();
                OnYield = onYield;
            }
            public override string Inspect() {
                return $"{MethodPath.Inspect()}({Arguments.Inspect()})" + (OnYield != null ? $"{{yield: {OnYield.Inspect()}}}" : "");
            }
            public override string Serialise() {
                return $"new MethodCallExpression({MethodPath.Serialise()}, {Arguments.Serialise()}, {(OnYield != null ? OnYield.Serialise() : "null")})";
            }
        }
        public class DefinedExpression : Expression {
            public readonly Expression Expression;
            public DefinedExpression(Expression expression) : base(expression.Location) {
                Expression = expression;
            }
            public override string Inspect() {
                return "defined? (" + Expression.Inspect() + ")";
            }
            public override string Serialise() {
                return $"new DefinedExpression({Expression.Serialise()})";
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
                return $"new MethodArgumentExpression({ArgumentName.Serialise()}, {(DefaultValue != null ? DefaultValue.Serialise() : "null")})";
            }
        }
        public class MethodExpression : Expression {
            public readonly List<Expression> Statements;
            public readonly IntRange ArgumentCount;
            public readonly List<MethodArgumentExpression> Arguments;

            public readonly Method Method;

            public MethodExpression(DebugLocation location, List<Expression> statements, IntRange? argumentCount, List<MethodArgumentExpression> arguments) : base(location) {
                Statements = statements;
                ArgumentCount = argumentCount ?? new IntRange();
                Arguments = arguments;

                Method = ToMethod();
            }
            public MethodExpression(DebugLocation location, List<Expression> statements, Range argumentCount, List<MethodArgumentExpression> arguments) : base(location) {
                Statements = statements;
                ArgumentCount = new IntRange(argumentCount);
                Arguments = arguments;

                Method = ToMethod();
            }
            public override string Inspect() {
                return $"method with {ArgumentCount} arguments";
            }
            public override string Serialise() {
                return $"new MethodExpression({Location.Serialise()}, {Statements.Serialise()}, {ArgumentCount.Serialise()}, {Arguments.Serialise()})";
            }
            Method ToMethod() {
                return new Method(async Input => {
                    return await Input.Interpreter.InterpretAsync(Statements, Input.OnYield);
                }, ArgumentCount, Arguments);
            }
        }
        public class SelfExpression : Expression {
            public SelfExpression(DebugLocation location) : base(location) { }
            public override string Inspect() {
                return "self";
            }
            public override string Serialise() {
                return $"new SelfExpression({Location.Serialise()})";
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
                return $"new LogicalExpression({Location.Serialise()}, {Left.Serialise()}, {Right.Serialise()})";
            }
            public enum LogicalExpressionType {
                And,
                Or,
                Xor
            }
        }
        public abstract class ConditionalExpression : Expression {
            public readonly Expression? Condition;
            public readonly List<Expression> Statements;
            public ConditionalExpression(DebugLocation location, Expression? condition, List<Expression> statements) : base(location) {
                Condition = condition;
                Statements = statements;
            }
        }
        public class IfExpression : ConditionalExpression {
            public IfExpression(DebugLocation location, Expression? condition, List<Expression> statements) : base(location, condition, statements) { }
            public override string Inspect() {
                return (Condition != null ? $"if {Condition.Inspect()} " : "else ") + "{" + Statements.Inspect() + "}";
            }
            public override string Serialise() {
                return $"new ShortIfExpression({Location.Serialise()}, {(Condition != null ? Condition.Serialise() : "null")}, {Statements.Serialise()})";
            }
        }
        public class WhileExpression : ConditionalExpression {
            public WhileExpression(DebugLocation location, Expression condition, List<Expression> statements) : base(location, condition, statements) { }
            public override string Inspect() {
                return $"while {Condition!.Inspect()} {{" + Statements.Inspect() + "}";
            }
            public override string Serialise() {
                return $"new WhileExpression({Location.Serialise()}, {Condition!.Serialise()}, {Statements.Serialise()})";
            }
        }
        public class ListExpression : Expression {
            public readonly List<Expression> Expressions;
            public ListExpression(DebugLocation location, List<Expression> expressions) : base(location) {
                Expressions = expressions;
            }
            public override string Inspect() {
                return Expressions.Inspect();
            }
            public override string Serialise() {
                return $"new ListExpression({Location.Serialise()}, {Expressions.Serialise()})";
            }
        }
        public class AssignmentExpression : Expression {
            public ObjectTokenExpression Left;
            public Expression Right;

            readonly string Operator;

            public AssignmentExpression(ObjectTokenExpression left, string op, Expression right) : base(left.Location) {
                Left = left;
                Operator = op;
                Right = right;

                // Compound assignment operators
                if (Operator != "=") {
                    if (Operator.Length >= 2) {
                        string ArithmeticOperator = Operator[..^1];
                        Right = new MethodCallExpression(
                            new PathExpression(Left, new Phase2Token(Left.Location, Phase2TokenType.Operator, ArithmeticOperator)),
                            new List<Expression>() { Right }
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
                return $"new AssignmentExpression({Left.Serialise()}, \"{Operator}\", {Right.Serialise()})";
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
                return $"new DoExpression({OnYield.Serialise()})";
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
                return $"new DefineMethodStatement({MethodName.Serialise()}, {MethodExpression.Serialise()})";
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
                return $"new UndefineMethodStatement({Location.Serialise()}, {MethodName.Serialise()})";
            }
        }
        public class DefineClassStatement : Statement {
            public readonly ObjectTokenExpression ClassName;
            public readonly List<Expression> BlockStatements;
            public readonly bool IsModule;
            public DefineClassStatement(ObjectTokenExpression className, List<Expression> blockStatements, bool isModule) : base(className.Location) {
                ClassName = className;
                BlockStatements = blockStatements;
                IsModule = isModule;
            }
            public override string Inspect() {
                return "class " + ClassName.Inspect();
            }
            public override string Serialise() {
                return $"new DefineClassStatement({ClassName.Serialise()}, {BlockStatements.Serialise()})";
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
                return $"new YieldStatement({Location.Serialise()}, {(YieldValues != null ? YieldValues.Serialise() : "null")})";
            }
        }
        public class ReturnStatement : Statement {
            public readonly List<Expression>? ReturnValues;
            public ReturnStatement(DebugLocation location, List<Expression>? returnValues = null) : base(location) {
                ReturnValues = returnValues;
            }
            public override string Inspect() {
                if (ReturnValues != null) return "return " + ReturnValues.Inspect();
                else return "return";
            }
            public override string Serialise() {
                return $"new ReturnStatement({Location.Serialise()}, {(ReturnValues != null ? ReturnValues.Serialise() : "null")})";
            }
        }
        public class IfBranchesStatement : Statement {
            public List<IfExpression> Branches;
            public IfBranchesStatement(DebugLocation location, List<IfExpression> branches) : base(location) {
                Branches = branches;
            }
            public override string Inspect() {
                return Branches.Inspect(" ");
            }
            public override string Serialise() {
                return $"new IfBranchesStatement({Location.Serialise()}, {Branches.Serialise()})";
            }
        }
        
        /*class Phase2ObjectsSnippet {
            public readonly List<Phase2Object> Objects;
            public readonly List<Phase2Object> AllObjects;
            public int PositionInAllObjects;
            public Phase2ObjectsSnippet(List<Phase2Object> objects, List<Phase2Object> allObjects, int positionInAllObjects) {
                Objects = objects;
                AllObjects = allObjects;
                PositionInAllObjects = positionInAllObjects;
            }
            public static implicit operator List<Phase2Object>(Phase2ObjectsSnippet Snippet) {
                return Snippet.Objects;
            }
            public static implicit operator Phase2ObjectsSnippet(List<Phase2Object> Objects) {
                return new Phase2ObjectsSnippet(Objects, Objects, 0);
            }
        }*/
        public class ListAddress<T> {
            public List<T> List;
            public int Index;
            public ListAddress(List<T> list, int index) {
                List = list;
                Index = index;
            }
            public T Get => List[Index];
            public T Set(T Value) => List[Index] = Value;
        }
        public enum SplatType {
            Single,
            Double
        }
        public enum ExpressionsType {
            Statements,
            CommaSeparatedExpressions,
            SingleExpression
        }
        enum BlockPriority {
            First,
            Second,
        }

        static Phase2Token IdentifierToPhase2(Phase1Token Token) {
            if (Token.Type != Phase1TokenType.Identifier)
                throw new InternalErrorException($"{Token.Location}: Cannot convert identifier to phase 2 for token that is not an identifier");

            foreach (KeyValuePair<string, Phase2TokenType> Keyword in Keywords) {
                if (Token.Value == Keyword.Key) {
                    return new Phase2Token(Token.Location, Keyword.Value, null, Token);
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
            else if (char.IsAsciiLetterUpper(Token.NonNullValue[0])) {
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
        /*static List<List<Phase2Object>> SplitObjects(List<Phase2Object> Objects, Phase2TokenType SplitToken, out List<string?> SplitCharas, bool CanStartWithHaveMultipleInARow) {
            List<List<Phase2Object>> SplitObjects = new();
            List<Phase2Object> CurrentObjects = new();

            SplitCharas = new List<string?>();

            foreach (Phase2Object Object in Objects) {
                if (Object is Phase2Token Token && Token.Type == SplitToken) {
                    if (CurrentObjects.Count != 0) {
                        SplitObjects.Add(CurrentObjects);
                        CurrentObjects = new List<Phase2Object>();
                        SplitCharas.Add(Token.Value);
                    }
                    else if (!CanStartWithHaveMultipleInARow) {
                        throw new SyntaxErrorException($"{Token.Location}: Cannot start with {SplitToken} or have multiple of them next to each other");
                    }
                }
                else {
                    CurrentObjects.Add(Object);
                }
            }
            if (CurrentObjects.Count != 0) {
                SplitObjects.Add(CurrentObjects);
            }
            return SplitObjects;
        }*/
        public static bool IsObjectToken(Phase2TokenType? Type) {
            return Type == Phase2TokenType.Nil
                || Type == Phase2TokenType.True
                || Type == Phase2TokenType.False
                || Type == Phase2TokenType.String
                || Type == Phase2TokenType.Integer
                || Type == Phase2TokenType.Float;
        }
        public static bool IsObjectToken(Phase2Token? Token) {
            return Token != null && IsObjectToken(Token.Type);
        }
        public static bool IsVariableToken(Phase2TokenType? Type) {
            return Type == Phase2TokenType.LocalVariableOrMethod
                || Type == Phase2TokenType.GlobalVariable
                || Type == Phase2TokenType.ConstantOrMethod
                || Type == Phase2TokenType.InstanceVariable
                || Type == Phase2TokenType.ClassVariable
                || Type == Phase2TokenType.Symbol
                || Type == Phase2TokenType.Self;
        }
        public static bool IsVariableToken(Phase2Token? Token) {
            return Token != null && IsVariableToken(Token.Type);
        }
        public static bool IsStartBlockToken(Phase2TokenType? Type) {
            return Type == Phase2TokenType.Def
                || Type == Phase2TokenType.Module
                || Type == Phase2TokenType.Class
                || Type == Phase2TokenType.Do
                || Type == Phase2TokenType.StartCurly
                || Type == Phase2TokenType.If
                || Type == Phase2TokenType.While;
        }
        public static bool IsStartBlockToken(Phase2Token? Token) {
            return Token != null && IsStartBlockToken(Token.Type);
        }

        static List<Phase2Token> TokensToPhase2(List<Phase1Token> Tokens) {
            // Phase 1 tokens to phase 2 tokens
            List<Phase2Token> NewTokens = new();
            for (int i = 0; i < Tokens.Count; i++) {
                Phase1Token Token = Tokens[i];

                if (Token.Type == Phase1TokenType.Identifier) {
                    NewTokens.Add(IdentifierToPhase2(Token));
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
                    NewTokens.Add(Token.Type switch {
                        Phase1TokenType.String => new Phase2Token(Token.Location, Phase2TokenType.String, Token.Value, Token),
                        Phase1TokenType.AssignmentOperator => new Phase2Token(Token.Location, Phase2TokenType.AssignmentOperator, Token.Value, Token),
                        Phase1TokenType.Operator => new Phase2Token(Token.Location, Phase2TokenType.Operator, Token.Value, Token),
                        Phase1TokenType.Dot => new Phase2Token(Token.Location, Phase2TokenType.Dot, Token.Value, Token),
                        Phase1TokenType.DoubleColon => new Phase2Token(Token.Location, Phase2TokenType.DoubleColon, Token.Value, Token),
                        Phase1TokenType.Comma => new Phase2Token(Token.Location, Phase2TokenType.Comma, Token.Value, Token),
                        Phase1TokenType.SplatOperator => new Phase2Token(Token.Location, Phase2TokenType.SplatOperator, Token.Value, Token),
                        Phase1TokenType.OpenBracket => new Phase2Token(Token.Location, Phase2TokenType.OpenBracket, Token.Value, Token),
                        Phase1TokenType.CloseBracket => new Phase2Token(Token.Location, Phase2TokenType.CloseBracket, Token.Value, Token),
                        Phase1TokenType.StartCurly => new Phase2Token(Token.Location, Phase2TokenType.StartCurly, Token.Value, Token),
                        Phase1TokenType.EndCurly => new Phase2Token(Token.Location, Phase2TokenType.EndCurly, Token.Value, Token),
                        Phase1TokenType.EndOfStatement => new Phase2Token(Token.Location, Phase2TokenType.EndOfStatement, Token.Value, Token),
                        _ => throw new InternalErrorException($"{Token.Location}: Conversion of {Token.Type} from phase 1 to phase 2 not supported")
                    });
                }
            }
            return NewTokens;
        }
        static List<Phase2Object> GetObjectsUntil(List<Phase2Object> Objects, ref int Index, Func<Phase2Object, bool> Condition) {
            List<Phase2Object> Tokens = new();
            while (Index < Objects.Count) {
                Phase2Object Token = Objects[Index];
                if (Condition(Token)) {
                    break;
                }
                Tokens.Add(Token);
                Index++;
            }
            return Tokens;
        }
        static List<Expression> BuildArguments(List<Phase2Object> Objects, ref int Index, bool WrappedInBrackets) {
            List<Phase2Object> ArgumentObjects;

            // Brackets e.g. puts("hi")
            if (WrappedInBrackets) {
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
            // No brackets e.g. puts "hi"
            else {
                ArgumentObjects = GetObjectsUntil(Objects, ref Index, Object => (Object is Phase2Token Token
                    && Token.Type == Phase2TokenType.EndOfStatement) || Object is Statement || Object is DoExpression);
                Index--;
            }

            List<Expression> Arguments = ObjectsToExpressions(ArgumentObjects, ExpressionsType.CommaSeparatedExpressions);
            return Arguments;
        }
        static List<Expression> ParseArgumentsWithBrackets(List<Phase2Object> Objects, ref int Index) {
            Index += 2;
            List<Expression> Arguments = BuildArguments(Objects, ref Index, true);
            return Arguments;
        }
        static List<Expression> ParseArgumentsWithoutBrackets(List<Phase2Object> Objects, ref int Index) {
            Index++;
            List<Expression> Arguments = BuildArguments(Objects, ref Index, false);
            return Arguments;
        }
        static List<Expression>? ParseArguments(List<Phase2Object> Objects, ref int Index) {
            if (Index + 1 < Objects.Count) {
                Phase2Object NextObject = Objects[Index + 1];
                if (NextObject is Phase2Token NextToken) {
                    if (NextToken.Type == Phase2TokenType.OpenBracket) {
                        if (!NextToken.FollowsWhitespace) {
                            return ParseArgumentsWithBrackets(Objects, ref Index);
                        }
                        else {
                            return ParseArgumentsWithoutBrackets(Objects, ref Index);
                        }
                    }
                }
                else if (NextObject is Expression && NextObject is not Statement) {
                    return ParseArgumentsWithoutBrackets(Objects, ref Index);
                }
            }
            return null;
        }
        static ObjectTokenExpression GetMethodName(List<Phase2Object> Phase2Objects, ref int Index) {
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
                    else if (Token.IsObjectToken || Token.Type == Phase2TokenType.OpenBracket
                        || Token.Type == Phase2TokenType.SplatOperator || Token.Type == Phase2TokenType.EndOfStatement)
                    {
                        break;
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
                throw new SyntaxErrorException($"{Object.Location}: Unexpected token while parsing method path: {Object.Inspect()}");
            }
            // Remove method name path tokens and replace with a path expression
            ObjectTokenExpression? MethodNamePathExpression = null;
            if (MethodNamePath.Count == 1) {
                MethodNamePathExpression = new ObjectTokenExpression(MethodNamePath[0]);
            }
            else if (MethodNamePath.Count == 0) {
                throw new SyntaxErrorException($"{Phase2Objects[Index].Location}: Def keyword must be followed by an identifier (got {Phase2Objects[Index].Inspect()})");
            }
            else {
                for (int i = 0; i < MethodNamePath.Count - 1; i++) {
                    MethodNamePathExpression = new PathExpression(new ObjectTokenExpression(MethodNamePath[i]), MethodNamePath[i + 1]);
                }
            }
            return MethodNamePathExpression!;
        }
        static List<MethodArgumentExpression> GetMethodArguments(List<Phase2Object> Phase2Objects, ref int Index, ObjectTokenExpression MethodName) {
            List<MethodArgumentExpression> MethodArguments = new();
            bool WrappedInBrackets = false;
            bool NextTokenCanBeObject = true;
            bool NextTokenCanBeComma = false;
            SplatType? SplatArgumentType = null;
            int StartIndex = Index;
            for (; Index < Phase2Objects.Count; Index++) {
                Phase2Object Object = Phase2Objects[Index];

                if (Object is Phase2Token ObjectToken) {
                    if (ObjectToken.Type == Phase2TokenType.Comma) {
                        if (NextTokenCanBeComma) {
                            NextTokenCanBeObject = true;
                            NextTokenCanBeComma = false;
                        }
                        else {
                            throw new SyntaxErrorException($"{ObjectToken.Location}: Unexpected comma");
                        }
                    }
                    else if (ObjectToken.Type == Phase2TokenType.SplatOperator) {
                        if (NextTokenCanBeObject) {
                            SplatArgumentType = ObjectToken.Value!.Length == 1 ? SplatType.Single : SplatType.Double;
                        }
                        else {
                            throw new SyntaxErrorException($"{ObjectToken.Location}: Unexpected splat operator");
                        }
                    }
                    else if (ObjectToken.Type == Phase2TokenType.AssignmentOperator && ObjectToken.Value == "=") {
                        if (MethodArguments.Count == 0) {
                            throw new SyntaxErrorException($"{ObjectToken.Location}: Unexpected '=' when parsing arguments");
                        }
                        if (MethodArguments[^1].DefaultValue != null) {
                            throw new SyntaxErrorException($"{ObjectToken.Location}: Default value already assigned");
                        }
                        List<Phase2Object> DefaultValueObjects = new();
                        for (Index++; Index < Phase2Objects.Count; Index++) {
                            if (Phase2Objects[Index] is Phase2Token Token
                                && (Token.Type == Phase2TokenType.Comma || Token.Type == Phase2TokenType.CloseBracket || Token.Type == Phase2TokenType.EndOfStatement))
                            {
                                Index--;
                                break;
                            }
                            else {
                                DefaultValueObjects.Add(Phase2Objects[Index]);
                            }
                        }
                        if (DefaultValueObjects.Count == 0) {
                            throw new SyntaxErrorException($"{ObjectToken.Location}: Expected value after '='");
                        }
                        else {
                            if (MethodArguments[^1].SplatType != null) {
                                throw new SyntaxErrorException($"{ObjectToken.Location}: Splat arguments cannot have default values");
                            }
                            MethodArguments[^1].DefaultValue = ObjectsToExpression(DefaultValueObjects);
                        }
                    }
                    else if (IsVariableToken(ObjectToken)) {
                        if (NextTokenCanBeObject) {
                            MethodArguments.Add(new MethodArgumentExpression(ObjectToken, null, SplatArgumentType));
                            NextTokenCanBeObject = false;
                            NextTokenCanBeComma = true;
                            SplatArgumentType = null;
                        }
                        else {
                            throw new SyntaxErrorException($"{ObjectToken.Location}: Unexpected argument {ObjectToken.Inspect()}");
                        }
                    }
                    else if (ObjectToken.Type == Phase2TokenType.OpenBracket) {
                        if (Index == StartIndex) {
                            WrappedInBrackets = true;
                        }
                        else {
                            throw new SyntaxErrorException($"{ObjectToken.Location}: Unexpected open bracket in method arguments");
                        }
                    }
                    else if (ObjectToken.Type == Phase2TokenType.CloseBracket) {
                        if (WrappedInBrackets) {
                            break;
                        }
                        else {
                            throw new SyntaxErrorException($"{ObjectToken.Location}: Unexpected close bracket in method arguments");
                        }
                    }
                    else if (ObjectToken.Type == Phase2TokenType.EndOfStatement) {
                        if (!WrappedInBrackets) {
                            break;
                        }
                    }
                    else {
                        throw new SyntaxErrorException($"{ObjectToken.Location}: Expected {(NextTokenCanBeObject ? "argument" : "comma")}, got {ObjectToken.Inspect()}");
                    }
                }
            }
            if (!NextTokenCanBeComma && NextTokenCanBeObject && MethodArguments.Count != 0) {
                throw new SyntaxErrorException($"{MethodName.Token.Location.Line}: Expected argument after comma, got nothing");
            }
            if (SplatArgumentType != null) {
                throw new SyntaxErrorException($"{MethodName.Token.Location.Line}: Expected argument after splat operator, got nothing");
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
        static ObjectTokenExpression GetClassName(List<Phase2Object> Phase2Objects, ref int Index, string ObjectType) {
            bool NextTokenCanBeVariable = true;
            bool NextTokenCanBeDoubleColon = false;
            List<Phase2Token> ClassNamePath = new();
            for (Index++; Index < Phase2Objects.Count; Index++) {
                Phase2Object Object = Phase2Objects[Index];

                if (Object is Phase2Token ObjectToken) {
                    if (ObjectToken.Type == Phase2TokenType.DoubleColon) {
                        if (NextTokenCanBeDoubleColon) {
                            NextTokenCanBeVariable = true;
                            NextTokenCanBeDoubleColon = false;
                        }
                        else {
                            throw new SyntaxErrorException($"{ObjectToken.Location}: Expected expression before and after '.'");
                        }
                    }
                    else if (IsVariableToken(ObjectToken)) {
                        if (NextTokenCanBeVariable) {
                            ClassNamePath.Add(ObjectToken);
                            NextTokenCanBeVariable = false;
                            NextTokenCanBeDoubleColon = true;
                        }
                        else {
                            break;
                        }
                    }
                    else if (ObjectToken.IsObjectToken) {
                        break;
                    }
                    else {
                        throw new SyntaxErrorException($"{ObjectToken.Location}: Unexpected token while parsing class path: {ObjectToken.Inspect()}");
                    }
                }
            }
            // Remove class name path tokens and replace with a path expression
            ObjectTokenExpression? ClassNamePathExpression = null;
            if (ClassNamePath.Count == 1) {
                ClassNamePathExpression = new ObjectTokenExpression(ClassNamePath[0]);
            }
            else if (ClassNamePath.Count == 0) {
                throw new SyntaxErrorException($"{Phase2Objects[Index].Location}: Class keyword must be followed by an identifier (got {Phase2Objects[Index].Inspect()})");
            }
            else {
                for (int i = 0; i < ClassNamePath.Count - 1; i++) {
                    ClassNamePathExpression = new PathExpression(new ObjectTokenExpression(ClassNamePath[i]), ClassNamePath[i + 1]);
                }
            }

            // Verify class name is constant
            if (ClassNamePathExpression!.Token.Type != Phase2TokenType.ConstantOrMethod) {
                throw new SyntaxErrorException($"{ClassNamePathExpression.Location}: {ObjectType} name must be Constant");
            }

            return ClassNamePathExpression;
        }
        static Expression? EndBlock(Stack<BuildingBlock> CurrentBlocks, bool EndIsCurly) {
            BuildingBlock Block = CurrentBlocks.Pop();

            // End Method Block
            if (Block is BuildingMethod MethodBlock) {
                return new DefineMethodStatement(MethodBlock.MethodName,
                    new MethodExpression(MethodBlock.Location, MethodBlock.Statements, new IntRange(MethodBlock.MinArgumentsCount, MethodBlock.MaxArgumentsCount), MethodBlock.Arguments)
                );
            }
            // End Class/Module Block
            else if (Block is BuildingClass ClassBlock) {
                return new DefineClassStatement(ClassBlock.ClassName, ClassBlock.Statements, ClassBlock.IsModule);
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

                MethodExpression OnYield = new(DoBlock.Location, DoBlock.Statements, null, DoBlock.Arguments);
                return new DoExpression(OnYield, DoBlock.DoIsCurly);

                /*// Get last block
                BuildingBlock LastBlock = CurrentBlocks.Peek();
                if (LastBlock != null && LastBlock.Statements.Count != 0) {
                    // Get last expression in last statement
                    Expression LastStatement = LastBlock.Statements[^1];

                    MethodExpression OnYield = new(DoBlock.Location, DoBlock.Statements, null, DoBlock.Arguments);

                    // Set on yield for previously known method call
                    if (LastStatement is MethodCallExpression LastMethodCallExpression) {
                        LastMethodCallExpression.OnYield = OnYield;
                        return null;
                    }
                    // Set on yield for LocalVariableOrMethod/ConstantOrMethod which we now know is a method call
                    else if (LastStatement is ObjectTokenExpression LastObjectTokenExpression) {
                        if (LastObjectTokenExpression.Token.Type == Phase2TokenType.LocalVariableOrMethod || LastObjectTokenExpression.Token.Type == Phase2TokenType.ConstantOrMethod) {
                            // Create method call from LocalVariableOrMethod/ConstantOrMethod
                            MethodCallExpression DeducedMethodCallExpression = new(LastObjectTokenExpression, null, OnYield);
                            LastBlock.Statements[^1] = DeducedMethodCallExpression;
                            return null;
                        }
                        else {
                            throw new SyntaxErrorException($"{DoBlock.Location}: Do block must follow method call, not {LastObjectTokenExpression.Token.Type}");
                        }
                    }
                    else {
                        throw new SyntaxErrorException($"{DoBlock.Location}: Do block must follow method call, not {LastStatement.GetType().Name}");
                    }
                }
                else {
                    throw new SyntaxErrorException($"{DoBlock.Location}: Do block must follow method call");
                }*/
            }
            // End If Block
            else if (Block is BuildingIfBranches IfBranches) {
                List<IfExpression> IfExpressions = new();
                for (int i = 0; i < IfBranches.Branches.Count; i++) {
                    BuildingIf Branch = IfBranches.Branches[i];
                    if (Branch.Condition == null && i != IfBranches.Branches.Count - 1) {
                        throw new SyntaxErrorException($"{Branch.Location}: Else must be the last branch in an if statement");
                    }
                    IfExpressions.Add(new IfExpression(Branch.Location, Branch.Condition, Branch.Statements));
                }
                return new IfBranchesStatement(IfBranches.Location, IfExpressions);
            }
            // End Unknown Block (internal error)
            else {
                throw new InternalErrorException($"{Block.Location}: End block not handled for type: {Block.GetType().Name}");
            }
        }
        static Expression ParseReturnOrYield(DebugLocation Location, List<Phase2Object> StatementTokens, ref int Index, bool IsReturn) {
            // Get return/yield values
            List<Expression>? ReturnOrYieldValues = ParseArguments(StatementTokens, ref Index);
            // Create yield/return statement
            return IsReturn ? new ReturnStatement(Location, ReturnOrYieldValues) : new YieldStatement(Location, ReturnOrYieldValues);
        }
        static Expression ParseUndef(DebugLocation Location, List<Phase2Object> StatementTokens, ref int Index) {
            // Get undef method name
            List<Expression>? UndefName = ParseArguments(StatementTokens, ref Index);
            // Get method name
            if (UndefName != null) {
                // Create undef statement
                if (UndefName.Count == 1 && UndefName[0] is ObjectTokenExpression MethodName && MethodName is not PathExpression) {
                    return new UndefineMethodStatement(Location, MethodName);
                }
                else {
                    throw new SyntaxErrorException($"{Location}: Expected local method name after 'undef', got {UndefName.Inspect()}");
                }
            }
            else {
                throw new SyntaxErrorException($"{Location}: Expected method name after 'undef', got nothing");
            }
        }
        static BuildingIf ParseIf(DebugLocation Location, List<Phase2Object> StatementTokens, ref int Index)
        {
            // Get condition
            for (Index++; Index < StatementTokens.Count; Index++) {
                if (StatementTokens[Index] is Phase2Token Token) {
                    if (Token.Type == Phase2TokenType.EndOfStatement || Token.Type == Phase2TokenType.Then) {
                        break;
                    }
                }
            }
            List<Phase2Object> Condition = StatementTokens.GetIndexRange(1, Index - 1);
            Expression ConditionExpression = ObjectsToExpression(Condition);

            // Open if block
            return new BuildingIf(Location, ConditionExpression);
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
                foreach (Expression Statement in Statements) {
                    CurrentBlocks.Peek().AddStatement(Statement);
                }
                return EndBlock(CurrentBlocks, EndIsCurly);
            }

            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object UnknownObject = ParsedObjects[i];
                DebugLocation Location = UnknownObject.Location;

                if (UnknownObject is Phase2Token Token) {
                    switch (Token.Type) {
                        // Def
                        case Phase2TokenType.Def: {
                            // Get method name
                            ObjectTokenExpression MethodName = GetMethodName(ParsedObjects, ref i);

                            // Get method arguments
                            List<MethodArgumentExpression> MethodArguments = GetMethodArguments(ParsedObjects, ref i, MethodName);

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
                            ObjectTokenExpression ClassName = GetClassName(ParsedObjects, ref i, ObjectType);

                            // Open define class block
                            PushBlock(new BuildingClass(Location, ClassName, IsModule));
                            break;
                        }
                        // Do
                        case Phase2TokenType.Do: {
                            // ResolveAllPending();

                            // Get do |arguments|
                            // To-do

                            // Open do block
                            PushBlock(new BuildingDo(Location, new(), false));
                            break;
                        }
                        // End
                        case Phase2TokenType.End: {
                            if (CurrentBlocks.Count == 1) {
                                throw new SyntaxErrorException($"{Token.Location}: Unexpected end statement");
                            }
                            Expression? EndedExpression = ResolveEndBlock(false);
                            if (EndedExpression != null) AddPendingObject(EndedExpression);
                            break;
                        }
                        // {
                        case Phase2TokenType.StartCurly: {
                            // Get do |arguments|
                            // To-do

                            // Open do block
                            PushBlock(new BuildingDo(Location, new(), true));
                            break;
                        }
                        // }
                        case Phase2TokenType.EndCurly: {
                            if (CurrentBlocks.Count == 1) {
                                throw new SyntaxErrorException($"{Token.Location}: Unexpected '}}'");
                            }
                            Expression? EndedExpression = ResolveEndBlock(true);
                            if (EndedExpression != null) AddPendingObject(EndedExpression);
                            break;
                        }
                        // Return
                        case Phase2TokenType.Return: {
                            AddPendingObject(ParseReturnOrYield(Token.Location, ParsedObjects, ref i, true));
                            break;
                        }
                        // Yield
                        case Phase2TokenType.Yield: {
                            AddPendingObject(ParseReturnOrYield(Token.Location, ParsedObjects, ref i, false));
                            break;
                        }
                        // Undef
                        case Phase2TokenType.Undef: {
                            AddPendingObject(ParseUndef(Token.Location, ParsedObjects, ref i));
                            break;
                        }
                        // If
                        case Phase2TokenType.If: {
                            BuildingIf BuildingIf = ParseIf(Token.Location, ParsedObjects, ref i);
                            PushBlock(new BuildingIfBranches(Token.Location, new List<BuildingIf>() {BuildingIf}));
                            break;
                        }
                        // Elsif
                        case Phase2TokenType.Elsif: {
                            if (CurrentBlocks.TryPeek(out BuildingBlock? Block) && Block is BuildingIfBranches IfBlock) {
                                IfBlock.Branches.Add(ParseIf(Token.Location, ParsedObjects, ref i));
                            }
                            else {
                                throw new SyntaxErrorException($"{Token.Location}: Elsif must follow if");
                            }
                            break;
                        }
                        // Else
                        case Phase2TokenType.Else: {
                            if (CurrentBlocks.TryPeek(out BuildingBlock? Block) && Block is BuildingIfBranches IfBlock) {
                                IfBlock.Branches.Add(new BuildingIf(Token.Location, null));
                            }
                            else {
                                throw new SyntaxErrorException($"{Token.Location}: Else must follow if");
                            }
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
                throw new SyntaxErrorException($"{CurrentBlocks.Peek().Location}: Block was never closed with an end statement");
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

            // Get Blocks
            ParsedObjects = GetBlocks(ParsedObjects);

            // Method calls (any)
            void ParseMethodCall(ObjectTokenExpression MethodName, int MethodNameIndex, bool WrappedInBrackets) {
                // Parse arguments
                int EndOfArgumentsIndex = MethodNameIndex;
                List<Expression>? Arguments = ParseArguments(ParsedObjects, ref EndOfArgumentsIndex);

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

                if (UnknownObject is ObjectTokenExpression ObjectToken && (ObjectToken.Token.Type == Phase2TokenType.LocalVariableOrMethod || ObjectToken.Token.Type == Phase2TokenType.ConstantOrMethod)) {
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

            // Paths
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object? LastObject = i - 1 >= 0 ? ParsedObjects[i - 1] : null;
                Phase2Object Object = ParsedObjects[i];
                Phase2Object? NextObject = i + 1 < ParsedObjects.Count ? ParsedObjects[i + 1] : null;

                if (Object is Phase2Token Token) {
                    // Path or Constant Path
                    if (Token.Type == Phase2TokenType.Dot || Token.Type == Phase2TokenType.DoubleColon) {
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
                                    throw new InternalErrorException($"Method call name following '.' should not be path expression (got {NextMethodCall.MethodPath.Inspect()})");
                                }
                                ParsedObjects.RemoveRange(i - 1, 2);
                                NextMethodCall.MethodPath = new PathExpression(LastObjectExpression, NextMethodCall.MethodPath.Token);
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
                        if (LastObjectTokenExpression.Token.Type == Phase2TokenType.LocalVariableOrMethod || LastObjectTokenExpression.Token.Type == Phase2TokenType.ConstantOrMethod) {
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

            // Operators
            foreach (string[] Operators in OperatorPrecedence) {
                for (int i = 0; i < ParsedObjects.Count; i++) {
                    Phase2Object UnknownObject = ParsedObjects[i];
                    Phase2Object? LastUnknownObject = i - 1 >= 0 ? ParsedObjects[i - 1] : null;
                    Phase2Object? NextUnknownObject = i + 1 < ParsedObjects.Count ? ParsedObjects[i + 1] : null;

                    if (UnknownObject is Phase2Token Token) {
                        if (Token.Type == Phase2TokenType.Operator && Operators.Contains(Token.Value!)) {
                            if (LastUnknownObject != null && NextUnknownObject != null && LastUnknownObject is Expression LastExpression && NextUnknownObject is Expression NextExpression) {
                                i--;
                                ParsedObjects.RemoveRange(i, 3);

                                if (NonMethodOperators.Contains(Token.Value!)) {
                                    LogicalExpression.LogicalExpressionType LogicType = Token.Value switch {
                                        "and" or "&&" => LogicalExpression.LogicalExpressionType.And,
                                        "or" or "||" => LogicalExpression.LogicalExpressionType.Or,
                                        "^" => LogicalExpression.LogicalExpressionType.Xor,
                                        _ => throw new InternalErrorException($"Unhandled logic expression type: '{Token.Value}'")
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

            // Defined?
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object UnknownObject = ParsedObjects[i];

                if (UnknownObject is Phase2Token Token) {
                    // defined?
                    if (Token.Type == Phase2TokenType.Defined) {
                        int EndOfArgumentsIndex = i;
                        List<Expression>? Arguments = ParseArguments(ParsedObjects, ref EndOfArgumentsIndex);
                        if (Arguments != null) {
                            if (Arguments.Count != 1) {
                                throw new SyntaxErrorException($"{Token.Location}: Expected a single argument after defined?");
                            }
                            ParsedObjects.RemoveRange(i, EndOfArgumentsIndex - i);
                            ParsedObjects.Insert(i, new DefinedExpression(Arguments[0]));
                        }
                        else {
                            throw new SyntaxErrorException($"{Token.Location}: Expected expression after defined?");
                        }
                    }
                }
            }

            // Method calls (no brackets)
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object UnknownObject = ParsedObjects[i];

                if (UnknownObject is ObjectTokenExpression ObjectToken && (ObjectToken.Token.Type == Phase2TokenType.LocalVariableOrMethod || ObjectToken.Token.Type == Phase2TokenType.ConstantOrMethod)) {
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

            // Assignment
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object UnknownObject = ParsedObjects[i];
                Phase2Object? LastUnknownObject = i - 1 >= 0 ? ParsedObjects[i - 1] : null;
                Phase2Object? NextUnknownObject = i + 1 < ParsedObjects.Count ? ParsedObjects[i + 1] : null;

                if (UnknownObject is Phase2Token Token) {
                    if (Token.Type == Phase2TokenType.AssignmentOperator) {
                        if (LastUnknownObject != null && NextUnknownObject != null && LastUnknownObject is ObjectTokenExpression LastExpression && NextUnknownObject is Expression NextExpression) {
                            i--;
                            ParsedObjects.RemoveRange(i, 3);

                            ParsedObjects.Insert(i, new AssignmentExpression(
                                LastExpression,
                                Token.Value!,
                                NextExpression
                            ));
                        }
                        else {
                            throw new SyntaxErrorException($"{Token.Location}: Assignment operator '{Token.Value}' must be between two expressions (got {(LastUnknownObject != null ? $"'{LastUnknownObject?.Inspect()}'" : "nothing")} and {(NextUnknownObject != null ? $"'{NextUnknownObject?.Inspect()}'" : "nothing")})");
                        }
                    }
                }
            }

            // Statement if/while condition
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object UnknownObject = ParsedObjects[i];

                if (UnknownObject is Phase2Token Token) {
                    if (Token.Type == Phase2TokenType.If || Token.Type == Phase2TokenType.While) {
                        if (i - 1 >= 0 && ParsedObjects[i - 1] is Expression Statement) {
                            if (i + 1 < ParsedObjects.Count && ParsedObjects[i + 1] is Expression Condition) {
                                // Remove three expressions
                                i--;
                                ParsedObjects.RemoveRange(i, 3);
                                // Get statement & conditional expression
                                List<Expression> ConditionalStatement = new() { Statement };
                                ConditionalExpression ConditionalExpression;
                                if (Token.Type == Phase2TokenType.If) {
                                    ConditionalExpression = new IfExpression(Statement.Location, Condition, ConditionalStatement);
                                }
                                else {
                                    ConditionalExpression = new WhileExpression(Statement.Location, Condition, ConditionalStatement);
                                }
                                // Insert conditional expression
                                ParsedObjects.Insert(i, ConditionalExpression);
                            }
                            else {
                                throw new SyntaxErrorException("Expected condition after 'if'");
                            }
                        }
                        else {
                            throw new InternalErrorException("Unhandled 'if' statement");
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
                            throw new SyntaxErrorException($"{ParsedObject.Location}: Expected expression, got {ParsedObject.Inspect()} (in {ParsedObjects.Inspect()})");
                        }
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
                        else {
                            throw new SyntaxErrorException($"{ParsedObject.Location}: Unexpected '{ParsedObject.Inspect()}'");
                        }
                    }
                    
                    if (AcceptComma == false && Expressions.Count != 0) {
                        throw new SyntaxErrorException($"{CommaLocation}: Expected expression after comma");
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
        /*static Expression TokenListToExpression(List<Phase1Token> Tokens) {
            List<Phase2Object> Phase2Objects = TokensToPhase2(Tokens);
            Expression Expression = ObjectsToExpression(Phase2Objects);
            return Expression;
        }*/

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
            public BuildingClass(DebugLocation location, ObjectTokenExpression className, bool isModule) : base(location) {
                ClassName = className;
                IsModule = isModule;
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
        class BuildingIfBranches : BuildingBlock {
            public readonly List<BuildingIf> Branches;
            public BuildingIfBranches(DebugLocation location, List<BuildingIf> branches) : base(location) {
                Branches = branches;
            }
            public override void AddStatement(Expression Statement) {
                Branches[^1].Statements.Add(Statement);
            }
        }
        class BuildingIf : BuildingBlock {
            public readonly Expression? Condition;
            public BuildingIf(DebugLocation location, Expression? condition) : base(location) {
                Condition = condition;
            }
        }
    }
    public static class Extensions {
        public static List<T> GetIndexRange<T>(this List<T> List, int StartIndex, int EndIndex) {
            if (StartIndex > EndIndex)
                return new List<T>();
            return List.GetRange(StartIndex, EndIndex - StartIndex + 1);
        }
        public static List<T> GetIndexRange<T>(this List<T> List, int StartIndex) {
            int EndIndex = List.Count - 1;
            if (StartIndex > EndIndex)
                return new List<T>();
            return List.GetRange(StartIndex, EndIndex - StartIndex + 1);
        }
        public static void RemoveIndexRange<T>(this List<T> List, int StartIndex, int EndIndex) {
            if (StartIndex <= EndIndex)
                List.RemoveRange(StartIndex, EndIndex - StartIndex + 1);
        }
        public static void RemoveIndexRange<T>(this List<T> List, int StartIndex) {
            int EndIndex = List.Count - 1;
            if (StartIndex <= EndIndex)
                List.RemoveRange(StartIndex, EndIndex - StartIndex + 1);
        }
        public static void RemoveFromEnd<T>(this List<T> List, Func<T, bool> While, bool RemoveOneOnly = false) {
            for (int i = List.Count - 1; i >= 0; i--) {
                if (While(List[i])) {
                    List.RemoveAt(i);
                    if (RemoveOneOnly) break;
                }
                else {
                    break;
                }
            }
        }
        public static string Serialise<T>(this List<T> List) where T : Phase2.Phase2Object {
            string Serialised = $"new List<{typeof(T).Name}>() {{";
            bool IsFirst = true;
            foreach (T Item in List) {
                if (IsFirst) IsFirst = false;
                else Serialised += ", ";
                Serialised += Item.Serialise();
            }
            return Serialised + "}";
        }
        public static string Inspect<T>(this List<T>? List, string Separator = ", ") where T : Phase2.Phase2Object {
            string ListInspection = "";
            if (List != null) {
                foreach (Phase2.Phase2Object Object in List) {
                    if (ListInspection.Length != 0)
                        ListInspection += Separator;
                    ListInspection += Object.Inspect();
                }
            }
            return ListInspection;
        }
        public static DebugLocation Location<T>(this List<T> List) where T : Phase2.Phase2Object {
            if (List.Count != 0)
                return List[0].Location;
            else
                return new DebugLocation();
        }
        public static void CopyTo<TKey, TValue>(this Dictionary<TKey, TValue> Origin, Dictionary<TKey, TValue> Target) where TKey : notnull {
            foreach (KeyValuePair<TKey, TValue> Pair in Origin) {
                Target.Add(Pair.Key, Pair.Value);
            }
        }
        public static void CopyTo<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> Origin, Dictionary<TKey, TValue> Target) where TKey : notnull {
            foreach (KeyValuePair<TKey, TValue> Pair in Origin) {
                Target.Add(Pair.Key, Pair.Value);
            }
        }
    }
}
