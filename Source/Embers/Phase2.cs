using static Embers.Interpreter;
using static Embers.Phase1;

namespace Embers
{
    public static class Phase2
    {
        public abstract class Phase2Object {
            public abstract string Inspect();
            public abstract string Serialise();
        }

        public enum Phase2TokenType {
            LocalVariableOrMethod,
            GlobalVariable,
            ConstantOrMethod,
            InstanceVariable,
            ClassVariable,

            Integer,
            Float,
            String,

            AssignmentOperator,
            ArithmeticOperator,

            // Keywords
            Alias, And, Begin, Break, Case, Class, Def, Defined, Do, Else, Elsif, End, Ensure, False, For, If, In, Module, Next, Nil, Not, Or, Redo, Rescue, Retry, Return, Self, Super, Then, True, Undef, Unless, Until, When, While, Yield,

            // Temporary
            Dot,
            DoubleColon,
            Comma,
            OpenBracket,
            CloseBracket,
            EndOfStatement,
        }
        public readonly static Dictionary<string, Phase2TokenType> Keywords = new() {
            {"alias", Phase2TokenType.Alias},
            {"and", Phase2TokenType.And},
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
            {"not", Phase2TokenType.Not},
            {"or", Phase2TokenType.Or},
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
        public readonly static string[][] ArithmeticOperatorPrecedence = new[] {
            new[] {"**"},
            new[] {"*", "/", "%"},
            new[] {"+", "-"}
        };

        public class Phase2Token : Phase2Object {
            public readonly Phase2TokenType Type;
            public readonly string? Value;
            public readonly bool FollowsWhitespace;
            public Phase2Token(Phase2TokenType type, string? value, bool followsWhitespace = false) {
                Type = type;
                Value = value;
                FollowsWhitespace = followsWhitespace;
            }
            public override string Inspect() {
                return $"{Type}{(Value != null ? ":" : "")}{Value?.Replace("\n", "\\n")}";
            }
            public override string Serialise() {
                return $"new Phase2Token(Phase2TokenType.{Type}, \"{Value}\", {(FollowsWhitespace ? "true" : "false")})";
            }
        }

        public abstract class Expression : Phase2Object { }
        /*public class ValueExpression : Expression {
            public List<Phase2Token> Path;
            public ValueExpression(List<Phase2Token> path) {
                Path = path;
            }
            public ValueExpression(Phase2Token path) {
                Path = new List<Phase2Token>() {path};
            }
            public Phase2TokenType? MainType {
                // get { return Path[^1] is Phase2Token Token ? Token.Type : null; }
                get { return Path[^1].Type; }
            }
            public override string Inspect() {
                return InspectList(Path, ".");
            }
        }*/
        public class ObjectTokenExpression : Expression {
            public readonly Phase2Token Token;
            public ObjectTokenExpression(Phase2Token objectToken) {
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
            public readonly Expression RootObject;
            public ConstantPathExpression(Expression rootObject, Phase2Token objectToken) : base(objectToken) {
                RootObject = rootObject;
            }
            public override string Inspect() {
                return RootObject.Inspect() + "." + RootObject.Inspect();
            }
            public override string Serialise() {
                return $"new ConstantPathExpression({RootObject.Serialise()}, {Token.Serialise()})";
            }
        }
        /*public class ArithmeticExpression : Expression {
            public Expression Left;
            public string Operator;
            public Expression Right;
            public ArithmeticExpression(Expression left, string op, Expression right) {
                Left = left;
                Operator = op;
                Right = right;
            }
            public override string Inspect() {
                return "(" + Left.Inspect() + " " + Operator + " " + Right.Inspect() + ")";
            }
        }*/
        public class MethodCallExpression : Expression {
            public readonly ObjectTokenExpression MethodPath;
            public readonly List<Expression> Arguments;
            public MethodExpression? OnYield; // do ... end
            public MethodCallExpression(ObjectTokenExpression methodPath, List<Expression>? arguments, MethodExpression? onYield = null) {
                MethodPath = methodPath;
                Arguments = arguments ?? new List<Expression>();
                OnYield = onYield;
            }
            public override string Inspect() {
                return $"{MethodPath.Inspect()}({InspectList(Arguments)})";
            }
            public override string Serialise() {
                return $"new MethodCallExpression({MethodPath.Serialise()}, {Arguments.Serialise()}, {(OnYield != null ? OnYield.Serialise() : "null")})";
            }
        }
        public class DefinedExpression : Expression {
            public readonly Expression Expression;
            public DefinedExpression(Expression expression) {
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
            public MethodArgumentExpression(Phase2Token argumentName, Expression? defaultValue = null) {
                ArgumentName = argumentName;
                DefaultValue = defaultValue;
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
            public readonly List<Statement> Statements;
            public readonly IntRange ArgumentCount;
            public readonly List<MethodArgumentExpression> Arguments;
            public MethodExpression(List<Statement> statements, IntRange? argumentCount, List<MethodArgumentExpression> arguments) {
                Statements = statements;
                ArgumentCount = argumentCount ?? new IntRange();
                Arguments = arguments;
            }
            public MethodExpression(List<Statement> statements, Range argumentCount, List<MethodArgumentExpression> arguments) {
                Statements = statements;
                ArgumentCount = new IntRange(argumentCount);
                Arguments = arguments;
            }
            public override string Inspect() {
                return $"method with {ArgumentCount} arguments";
            }
            public override string Serialise() {
                return $"new MethodExpression({Statements.Serialise()}, {ArgumentCount.Serialise()}, {Arguments.Serialise()})";
            }
            public Method ToMethod() {
                return new Method(async Input => {
                    return await Input.Interpreter.InterpretAsync(Statements);
                }, ArgumentCount, Arguments);
            }
        }

        public abstract class Statement : Expression { }
        public class ExpressionStatement : Statement {
            public Expression Expression;
            public ExpressionStatement(Expression expression) {
                Expression = expression;
            }
            public override string Inspect() {
                return Expression.Inspect();
            }
            public override string Serialise() {
                return $"new ExpressionStatement({Expression.Serialise()})";
            }
        }
        public class AssignmentStatement : Statement {
            public Expression Left;
            public readonly string Operator;
            public Expression Right;
            public AssignmentStatement(Expression left, string op, Expression right) {
                Left = left;
                Operator = op;
                Right = right;
            }
            public override string Inspect() {
                return Left.Inspect() + " " + Operator + " " + Right.Inspect();
            }
            public override string Serialise() {
                return $"new AssignmentStatement({Left.Serialise()}, \"{Operator}\", {Right.Serialise()})";
            }
        }
        public class DefineMethodStatement : Statement {
            public readonly ObjectTokenExpression MethodName;
            public readonly MethodExpression Method;
            public DefineMethodStatement(ObjectTokenExpression methodName, MethodExpression method) {
                MethodName = methodName;
                Method = method;
            }
            public override string Inspect() {
                return "def " + MethodName.Inspect();
            }
            public override string Serialise() {
                return $"new DefineMethodStatement({MethodName.Serialise()}, {Method.Serialise()})";
            }
        }
        public class UndefineMethodStatement : Statement {
            public readonly ObjectTokenExpression MethodName;
            public UndefineMethodStatement(ObjectTokenExpression methodName) {
                MethodName = methodName;
            }
            public override string Inspect() {
                return "undef " + MethodName.Inspect();
            }
            public override string Serialise() {
                return $"new UndefineMethodStatement({MethodName.Serialise()})";
            }
        }
        public class DefineClassStatement : Statement {
            public readonly ObjectTokenExpression ClassName;
            public readonly List<Statement> BlockStatements;
            public DefineClassStatement(ObjectTokenExpression className, List<Statement> blockStatements) {
                ClassName = className;
                BlockStatements = blockStatements;
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
            public YieldStatement(List<Expression>? yieldValues = null) {
                YieldValues = yieldValues;
            }
            public override string Inspect() {
                if (YieldValues != null) return "yield " + InspectList(YieldValues);
                else return "yield";
            }
            public override string Serialise() {
                return $"new YieldStatement({(YieldValues != null ? YieldValues.Serialise() : "null")})";
            }
        }
        public class ReturnStatement : Statement {
            public readonly List<Expression>? ReturnValues;
            public ReturnStatement(List<Expression>? returnValues = null) {
                ReturnValues = returnValues;
            }
            public override string Inspect() {
                if (ReturnValues != null) return "return " + InspectList(ReturnValues);
                else return "return";
            }
            public override string Serialise() {
                return $"new ReturnStatement({(ReturnValues != null ? ReturnValues.Serialise() : "null")})";
            }
        }

        static Phase2Token IdentifierToPhase2(Phase1Token Token) {
            if (Token.Type != Phase1TokenType.Identifier)
                throw new InternalErrorException("Cannot convert identifier to phase 2 for token that is not an identifier");

            foreach (KeyValuePair<string, Phase2TokenType> Keyword in Keywords) {
                if (Token.Value == Keyword.Key) {
                    return new Phase2Token(Keyword.Value, null, Token.FollowsWhitespace);
                }
            }

            Phase2TokenType IdentifierType;
            string Identifier;

            if (Token.NonNullValue[0] == '$') {
                IdentifierType = Phase2TokenType.GlobalVariable;
                if (Token.NonNullValue.Length == 0) throw new SyntaxErrorException("Identifier '$' not valid for global variable");
                Identifier = Token.NonNullValue[1..];
            }
            else if (Token.NonNullValue.StartsWith("@@")) {
                IdentifierType = Phase2TokenType.ClassVariable;
                if (Token.NonNullValue.Length <= 1) throw new SyntaxErrorException("Identifier '@@' not valid for class variable");
                Identifier = Token.NonNullValue[2..];
            }
            else if (Token.NonNullValue[0] == '@') {
                IdentifierType = Phase2TokenType.InstanceVariable;
                if (Token.NonNullValue.Length == 0) throw new SyntaxErrorException("Identifier '@' not valid for instance variable");
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
                throw new Exception("Identifier cannot contain $ or @");
            }

            return new Phase2Token(IdentifierType, Identifier, Token.FollowsWhitespace);
        }
        static List<List<Phase2Object>> SplitObjects(List<Phase2Object> Objects, Phase2TokenType SplitToken, out List<string?> SplitCharas, bool CanStartWithHaveMultipleInARow) {
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
                        throw new SyntaxErrorException($"Cannot start with {SplitToken} or have multiple of them next to each other");
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
        }
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
                || Type == Phase2TokenType.ClassVariable;
        }
        public static bool IsVariableToken(Phase2Token? Token) {
            return Token != null && IsVariableToken(Token.Type);
        }
        static string InspectList<T>(List<T>? List, string Separator = ", ") where T : Phase2Object {
            string ListInspection = "";
            if (List != null) {
                foreach (Phase2Object Object in List) {
                    if (ListInspection.Length != 0)
                        ListInspection += Separator;
                    ListInspection += Object.Inspect();
                }
            }
            return ListInspection;
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
                        NewTokens.Add(new Phase2Token(Phase2TokenType.Float, Token.Value + "." + Tokens[i + 2].Value, Token.FollowsWhitespace));
                        i += 2;
                    }
                    else {
                        NewTokens.Add(new Phase2Token(Phase2TokenType.Integer, Token.Value, Token.FollowsWhitespace));
                    }
                }
                else {
                    NewTokens.Add(Token.Type switch {
                        Phase1TokenType.String => new Phase2Token(Phase2TokenType.String, Token.Value, Token.FollowsWhitespace),
                        Phase1TokenType.AssignmentOperator => new Phase2Token(Phase2TokenType.AssignmentOperator, Token.Value, Token.FollowsWhitespace),
                        Phase1TokenType.ArithmeticOperator => new Phase2Token(Phase2TokenType.ArithmeticOperator, Token.Value, Token.FollowsWhitespace),
                        Phase1TokenType.Dot => new Phase2Token(Phase2TokenType.Dot, Token.Value, Token.FollowsWhitespace),
                        Phase1TokenType.DoubleColon => new Phase2Token(Phase2TokenType.DoubleColon, Token.Value, Token.FollowsWhitespace),
                        Phase1TokenType.Comma => new Phase2Token(Phase2TokenType.Comma, Token.Value, Token.FollowsWhitespace),
                        Phase1TokenType.OpenBracket => new Phase2Token(Phase2TokenType.OpenBracket, Token.Value, Token.FollowsWhitespace),
                        Phase1TokenType.CloseBracket => new Phase2Token(Phase2TokenType.CloseBracket, Token.Value, Token.FollowsWhitespace),
                        Phase1TokenType.EndOfStatement => new Phase2Token(Phase2TokenType.EndOfStatement, Token.Value, Token.FollowsWhitespace),
                        _ => throw new InternalErrorException($"Conversion of {Token.Type} from phase 1 to phase 2 not supported")
                    });
                }
            }
            return NewTokens;
        }
        static List<Phase2Object> GetTokensUntil(List<Phase2Object> Objects, ref int Index, Func<Phase2Object, bool> Condition) {
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
            List<Expression> Arguments = new();

            bool AcceptArgument = true;
            while (Index < Objects.Count) {
                int AddArgument(int Index) {
                    List<Phase2Object> Argument = GetTokensUntil(Objects, ref Index, obj =>
                        obj is Phase2Token tok && (
                            tok.Type == Phase2TokenType.Comma
                            || tok.Type == Phase2TokenType.CloseBracket
                            || tok.Type == Phase2TokenType.Do
                        ));
                    Arguments.Add(ObjectsToExpression(Argument));
                    Index--;
                    AcceptArgument = false;
                    return Index;
                }
                if (Objects[Index] is Phase2Token Token) {
                    if (Token.Type == Phase2TokenType.Comma) {
                        if (AcceptArgument)
                            throw new SyntaxErrorException("Expected argument before ','");
                        AcceptArgument = true;
                    }
                    else if (Token.Type == Phase2TokenType.CloseBracket) {
                        if (WrappedInBrackets)
                            Index++;
                        break;
                    }
                    else if (Token.Type == Phase2TokenType.Do) {
                        break;
                    }
                    else {
                        Index = AddArgument(Index);
                    }
                }
                else {
                    Index = AddArgument(Index);
                }
                Index++;
            }
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
                    else {
                        // Might be redundant.
                        return ParseArgumentsWithoutBrackets(Objects, ref Index);
                    }
                }
                else {
                    return ParseArgumentsWithoutBrackets(Objects, ref Index);
                }
            }
            return null;
        }
        static List<Expression> ObjectsToExpressions(List<Phase2Object> Phase2Objects) {
            List<Phase2Object> ParsedObjects = new(Phase2Objects); // Preserve the original list

            // Brackets
            

            // Paths
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object? LastObject = i - 1 >= 0 ? ParsedObjects[i - 1] : null;
                Phase2Object Object = ParsedObjects[i];
                Phase2Object? NextObject = i + 1 < ParsedObjects.Count ? ParsedObjects[i + 1] : null;

                if (Object is Phase2Token Token) {
                    if (Token.Type == Phase2TokenType.Dot) {
                        if (LastObject != null) {
                            if (LastObject is Expression LastExpression && NextObject is Phase2Token NextToken) {
                                ParsedObjects.RemoveRange(i - 1, 3);
                                ParsedObjects.Insert(i - 1, new PathExpression(LastExpression, NextToken));
                                i -= 2;
                            }
                            else {
                                throw new SyntaxErrorException("Expected expression before and after .");
                            }
                        }
                        else {
                            throw new SyntaxErrorException("Expected a value before .");
                        }
                    }
                    else if (IsVariableToken(Token) || IsObjectToken(Token)) {
                        ParsedObjects.RemoveAt(i);
                        ParsedObjects.Insert(i, new ObjectTokenExpression(Token));
                    }
                }
            }

            // Arithmetic operators
            foreach (string[] Operators in ArithmeticOperatorPrecedence) {
                for (int i = 0; i < ParsedObjects.Count; i++) {
                    Phase2Object UnknownToken = ParsedObjects[i];
                    Phase2Object? LastUnknownToken = i - 1 >= 0 ? ParsedObjects[i - 1] : null;
                    Phase2Object? NextUnknownToken = i + 1 < ParsedObjects.Count ? ParsedObjects[i + 1] : null;

                    if (UnknownToken is Phase2Token Token) {
                        if (Token.Type == Phase2TokenType.ArithmeticOperator && Operators.Contains(Token.Value!)) {
                            if (LastUnknownToken != null && NextUnknownToken != null && LastUnknownToken is Expression LastExpression && NextUnknownToken is Expression NextExpression) {
                                i--;
                                ParsedObjects.RemoveRange(i, 3);
                                ParsedObjects.Insert(i, new MethodCallExpression(
                                    new PathExpression(LastExpression, new Phase2Token(Phase2TokenType.LocalVariableOrMethod, "+")),
                                    new List<Expression>() {NextExpression})
                                );
                            }
                            else {
                                throw new SyntaxErrorException($"Arithmetic operator must be between two expressions (got {LastUnknownToken?.Inspect()} and {NextUnknownToken?.Inspect()})");
                            }
                        }
                    }
                }
            }

            // Defined?
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object UnknownToken = ParsedObjects[i];

                if (UnknownToken is Phase2Token Token) {
                    // defined?
                    if (Token.Type == Phase2TokenType.Defined) {
                        int EndOfArgumentsIndex = i;
                        List<Expression>? Arguments = ParseArguments(ParsedObjects, ref EndOfArgumentsIndex);
                        if (Arguments != null) {
                            if (Arguments.Count != 1) {
                                throw new Exception("might wanna evaluate this expr");
                            }
                            ParsedObjects.RemoveRange(i, EndOfArgumentsIndex - i);
                            ParsedObjects.Insert(i, new DefinedExpression(Arguments[0]));
                        }
                        else {
                            throw new SyntaxErrorException("Expected expression after defined?");
                        }
                    }
                }
            }

            // Method calls
            for (int i = 0; i < ParsedObjects.Count; i++) {
                Phase2Object UnknownToken = ParsedObjects[i];

                if (UnknownToken is ObjectTokenExpression ObjectToken && (ObjectToken.Token.Type == Phase2TokenType.LocalVariableOrMethod || ObjectToken.Token.Type == Phase2TokenType.ConstantOrMethod)) {
                    // Parse arguments
                    int EndOfArgumentsIndex = i;
                    List<Expression>? Arguments = ParseArguments(ParsedObjects, ref EndOfArgumentsIndex);

                    // Add method call
                    if (Arguments != null) {
                        ParsedObjects.RemoveRange(i, EndOfArgumentsIndex - i);
                        ParsedObjects.Insert(i, new MethodCallExpression(ObjectToken, Arguments));
                    }
                }
            }

            // Convert objects to expressions
            List<Expression> Expressions = new();
            foreach (Phase2Object ParsedObject in ParsedObjects) {
                if (ParsedObject is Expression ParsedExpression)
                    Expressions.Add(ParsedExpression);
                else
                    throw new InternalErrorException($"Parsed objects should all be expressions (one was {ParsedObject.GetType().Name} {ParsedObject.Inspect()})");
            }
            return Expressions;
        }
        static Expression ObjectsToExpression(List<Phase2Object> Phase2Objects) {
            List<Expression> Expressions = ObjectsToExpressions(Phase2Objects);
            if (Expressions.Count != 1)
                throw new InternalErrorException($"Parsed objects should result in a single object (got {Expressions.Count} objects ({InspectList(Expressions)}))");
            return Expressions[0];
        }
        /*static Expression TokenListToExpression(List<Phase1Token> Tokens) {
            List<Phase2Object> Phase2Objects = TokensToPhase2(Tokens);
            Expression Expression = ObjectsToExpression(Phase2Objects);
            return Expression;
        }*/

        static AssignmentStatement? ParseAssignmentStatement(List<Phase2Object> StatementTokens) {
            List<List<Phase2Object>> Expressions = SplitObjects(StatementTokens, Phase2TokenType.AssignmentOperator, out List<string?> AssignmentOperators, false);
            if (Expressions.Count == 1) return null;

            // a = b
            List<Phase2Object> Right = Expressions[^1];
            string Operator = AssignmentOperators[^1] ?? throw new InternalErrorException("Assignment operator was null");
            List<Phase2Object> Left = Expressions[^2];

            AssignmentStatement? CurrentAssignment = new(ObjectsToExpression(Left), Operator, ObjectsToExpression(Right));

            Expressions.RemoveRange(Expressions.Count - 2, 2);
            AssignmentOperators.RemoveAt(AssignmentOperators.Count - 1);

            // a = b = c
            while (Expressions.Count != 0) {
                List<Phase2Object> Left2 = Expressions[^1];
                string Operator2 = AssignmentOperators[^1] ?? throw new InternalErrorException("Assignment operator was null");

                Expressions.RemoveAt(Expressions.Count - 1);
                AssignmentOperators.RemoveAt(AssignmentOperators.Count - 1);

                CurrentAssignment = new AssignmentStatement(ObjectsToExpression(Left2), Operator2, CurrentAssignment);
            }

            return CurrentAssignment;
        }
        static BuildingMethod ParseStartDefineMethod(List<Phase2Object> StatementTokens) {
            if (StatementTokens.Count == 1)
                throw new SyntaxErrorException("Def keyword must be followed by an identifier (got nothing)");
                                
            // Get def statement (e.g my_method(arg1, arg2))
            int EndOfDef = StatementTokens.FindIndex(o => o is Phase2Token tok && tok.Type == Phase2TokenType.CloseBracket);
            if (EndOfDef == -1) EndOfDef = StatementTokens.Count - 1;
            List<Phase2Object> DefObjects = StatementTokens.GetIndexRange(1, EndOfDef);

            // Check for remaining arguments (internal error)
            if (EndOfDef + 1 > StatementTokens.Count) {
                List<Phase2Object> RemainingArguments = StatementTokens.GetIndexRange(EndOfDef + 1);
                throw new InternalErrorException($"There shouldn't be any remaining arguments after DefObjects (got {InspectList(RemainingArguments)})");
            }

            // Get method name
            {
                bool NextTokenCanBeVariable = true;
                bool NextTokenCanBeDot = false;
                List<Phase2Token> MethodNamePath = new();
                for (int i = 0; i < DefObjects.Count; i++) {
                    Phase2Object? LastObject = i - 1 >= 0 ? DefObjects[i - 1] : null;
                    Phase2Object Object = DefObjects[i];
                    Phase2Object? NextObject = i + 1 < DefObjects.Count ? DefObjects[i + 1] : null;

                    if (Object is Phase2Token ObjectToken) {
                        if (ObjectToken.Type == Phase2TokenType.Dot) {
                            if (NextTokenCanBeDot) {
                                NextTokenCanBeVariable = true;
                                NextTokenCanBeDot = false;
                            }
                            else {
                                throw new SyntaxErrorException("Expected expression before and after .");
                            }
                        }
                        else if (IsVariableToken(ObjectToken)) {
                            if (NextTokenCanBeVariable) {
                                MethodNamePath.Add(ObjectToken);
                                NextTokenCanBeVariable = false;
                                NextTokenCanBeDot = true;
                            }
                            else {
                                break;
                            }
                        }
                        else if (ObjectToken.Type == Phase2TokenType.OpenBracket) {
                            break;
                        }
                        else if (IsObjectToken(ObjectToken)) {
                            break;
                        }
                        else {
                            throw new SyntaxErrorException($"Unexpected token when parsing method path: {ObjectToken.Inspect()}");
                        }
                    }
                }
                // Remove method name path tokens and replace with a path expression
                DefObjects.RemoveRange(0, MethodNamePath.Count + MethodNamePath.Count - 1);
                ObjectTokenExpression? MethodNamePathExpression = null;
                if (MethodNamePath.Count == 1) {
                    MethodNamePathExpression = new ObjectTokenExpression(MethodNamePath[0]);
                }
                else {
                    for (int i = 0; i < MethodNamePath.Count; i++) {
                        MethodNamePathExpression = new PathExpression(new ObjectTokenExpression(MethodNamePath[i]), MethodNamePath[i + 1]);
                    }
                }
                DefObjects.Insert(0, MethodNamePathExpression!);
            }
            ObjectTokenExpression MethodName = DefObjects[0] as ObjectTokenExpression ?? throw new SyntaxErrorException($"Def keyword must be followed by an identifier (got {DefObjects[0].Inspect()})");

            // Get method arguments
            List<MethodArgumentExpression> MethodArguments = new();
            {
                bool WrappedInBrackets = false;
                bool NextTokenCanBeObject = true;
                bool NextTokenCanBeComma = false;
                for (int i = 1; i < DefObjects.Count; i++) {
                    Phase2Object Object = DefObjects[i];

                    if (Object is Phase2Token ObjectToken) {
                        if (ObjectToken.Type == Phase2TokenType.Comma) {
                            if (NextTokenCanBeComma) {
                                NextTokenCanBeObject = true;
                                NextTokenCanBeComma = false;
                            }
                            else {
                                throw new SyntaxErrorException($"Expected comma");
                            }
                        }
                        else if (ObjectToken.Type == Phase2TokenType.AssignmentOperator && ObjectToken.Value == "=") {
                            if (MethodArguments.Count == 0) {
                                throw new SyntaxErrorException("Unexpected '=' when parsing arguments");
                            }
                            if (MethodArguments[^1].DefaultValue != null) {
                                throw new SyntaxErrorException("Default value already assigned");
                            }
                            List<Phase2Object> DefaultValueObjects = new();
                            for (i++; i < DefObjects.Count; i++) {
                                if (DefObjects[i] is Phase2Token Token
                                    && (Token.Type == Phase2TokenType.Comma || Token.Type == Phase2TokenType.CloseBracket || Token.Type == Phase2TokenType.EndOfStatement))
                                {
                                    i--;
                                    break;
                                }
                                else {
                                    DefaultValueObjects.Add(DefObjects[i]);
                                }
                            }
                            if (DefaultValueObjects.Count == 0) {
                                throw new SyntaxErrorException("Expected value after '='");
                            }
                            else {
                                MethodArguments[^1].DefaultValue = ObjectsToExpression(DefaultValueObjects);
                            }
                        }
                        else if (IsVariableToken(ObjectToken)) {
                            if (NextTokenCanBeObject) {
                                MethodArguments.Add(new MethodArgumentExpression(ObjectToken));
                                NextTokenCanBeObject = false;
                                NextTokenCanBeComma = true;
                            }
                            else {
                                throw new SyntaxErrorException($"Unexpected argument {ObjectToken.Inspect()}");
                            }
                        }
                        else if (ObjectToken.Type == Phase2TokenType.OpenBracket) {
                            if (i == 1) {
                                WrappedInBrackets = true;
                            }
                            else {
                                throw new SyntaxErrorException("Unexpected open bracket in method arguments");
                            }
                        }
                        else if (ObjectToken.Type == Phase2TokenType.CloseBracket) {
                            if (WrappedInBrackets) {
                                break;
                            }
                            else {
                                throw new SyntaxErrorException("Unexpected close bracket in method arguments");
                            }
                        }
                        else {
                            throw new SyntaxErrorException($"Expected {(NextTokenCanBeObject ? "argument" : "comma")}, got {ObjectToken.Inspect()}");
                        }
                    }
                }
                if (!NextTokenCanBeComma && NextTokenCanBeObject && DefObjects.Count != 1) {
                    throw new SyntaxErrorException("Expected value after comma, got nothing");
                }
            }

            // Open define method block
            return new BuildingMethod(MethodName, MethodArguments);
        }
        static BuildingClass ParseStartDefineClass(List<Phase2Object> StatementTokens) {
            if (StatementTokens.Count == 1)
                throw new SyntaxErrorException("Class keyword must be followed by an identifier (got nothing)");
            
            // Get class name
            {
                bool NextTokenCanBeVariable = true;
                bool NextTokenCanBeDoubleColon = false;
                List<Phase2Token> ClassNamePath = new();
                for (int i = 1; i < StatementTokens.Count; i++) {
                    // Phase2Object? LastObject = i - 1 >= 1 ? StatementTokens[i - 1] : null;
                    Phase2Object Object = StatementTokens[i];
                    // Phase2Object? NextObject = i + 1 < StatementTokens.Count ? StatementTokens[i + 1] : null;

                    if (Object is Phase2Token ObjectToken) {
                        if (ObjectToken.Type == Phase2TokenType.DoubleColon) {
                            if (NextTokenCanBeDoubleColon) {
                                NextTokenCanBeVariable = true;
                                NextTokenCanBeDoubleColon = false;
                            }
                            else {
                                throw new SyntaxErrorException("Expected expression before and after .");
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
                        else if (ObjectToken.Type == Phase2TokenType.OpenBracket) {
                            break;
                        }
                        else if (IsObjectToken(ObjectToken)) {
                            break;
                        }
                        else {
                            throw new SyntaxErrorException($"Unexpected token when parsing method path: {ObjectToken.Inspect()}");
                        }
                    }
                }
                // Remove method name path tokens and replace with a path expression
                StatementTokens.RemoveRange(0, ClassNamePath.Count + ClassNamePath.Count - 1);
                ObjectTokenExpression? ClassNamePathExpression = null;
                if (ClassNamePath.Count == 1) {
                    ClassNamePathExpression = new ObjectTokenExpression(ClassNamePath[0]);
                }
                else {
                    for (int i = 0; i < ClassNamePath.Count; i++) {
                        ClassNamePathExpression = new PathExpression(new ObjectTokenExpression(ClassNamePath[i]), ClassNamePath[i + 1]);
                    }
                }
                StatementTokens.Insert(0, ClassNamePathExpression!);
            }
            ObjectTokenExpression ClassName = StatementTokens[0] as ObjectTokenExpression ?? throw new SyntaxErrorException($"Class keyword must be followed by an identifier (got {StatementTokens[0].Inspect()})");
            if (ClassName.Token.Type != Phase2TokenType.ConstantOrMethod) {
                throw new SyntaxErrorException("Class name must be Constant");
            }

            // Open define class block
            return new BuildingClass(ClassName);
        }
        static Statement? ParseEndStatement(Stack<BuildingBlock> BlockStackInfo) {
            BuildingBlock Block = BlockStackInfo.Pop();

            // End Method Block
            if (Block is BuildingMethod MethodBlock) {
                /*return new DefineMethodStatement(MethodBlock.MethodName,
                    new Method(async Input => {
                        // Set OnYield function
                        Func<Instances, Task>? OnYield = null;
                        if (Input.OnYield != null)
                            OnYield = (YieldArgs) => {
                                return Input.OnYield.Call(Input.Interpreter, Input.Instance, YieldArgs);
                            };
                        // Interpret method call
                        return await Input.Interpreter.InterpretAsync(MethodBlock.Statements, OnYield);
                    }, MethodBlock.RequiredArgumentsCount..MethodBlock.Arguments.Count, MethodBlock.Arguments)
                );*/
                return new DefineMethodStatement(MethodBlock.MethodName,
                    new MethodExpression(MethodBlock.Statements, MethodBlock.RequiredArgumentsCount..MethodBlock.Arguments.Count, MethodBlock.Arguments)
                );
            }
            // End Class Block
            else if (Block is BuildingClass ClassBlock) {
                return new DefineClassStatement(ClassBlock.ClassName, ClassBlock.Statements);
            }
            // End Do Block
            else if (Block is BuildingDo DoBlock) {
                // Get last block
                BuildingBlock LastBlock = BlockStackInfo.Peek();
                if (LastBlock != null && LastBlock.Statements.Count != 0) {
                    // Get last expression in last statement
                    Statement LastStatement = LastBlock.Statements[^1];
                    Expression LastExpression;
                    {
                        if (LastStatement is AssignmentStatement LastAssignmentStatement) {
                            LastExpression = LastAssignmentStatement.Right;
                        }
                        else if (LastStatement is ExpressionStatement LastExpressionStatement) {
                            LastExpression = LastExpressionStatement.Expression;
                        }
                        else {
                            throw new SyntaxErrorException($"Do block must follow method call, not {LastStatement.GetType().Name}");
                        }
                    }

                    // Get on yield method
                    /*Method? OnYield = new(async Input => {
                        return await Input.Interpreter.InterpretAsync(DoBlock.Statements);
                    }, null, DoBlock.Arguments);*/
                    MethodExpression OnYield = new(DoBlock.Statements, null, DoBlock.Arguments);

                    // Set on yield for already known method call
                    if (LastExpression is MethodCallExpression LastMethodCallExpression) {
                        LastMethodCallExpression.OnYield = OnYield;
                        return null;
                    }
                    // Set on yield for LocalVariableOrMethod/ConstantOrMethod which we now know is definitely a method call
                    else if (LastExpression is ObjectTokenExpression LastObjectTokenExpression) {
                        if (LastObjectTokenExpression.Token.Type == Phase2TokenType.LocalVariableOrMethod || LastObjectTokenExpression.Token.Type == Phase2TokenType.ConstantOrMethod) {
                            // Create method call from LocalVariableOrMethod/ConstantOrMethod
                            MethodCallExpression DeducedMethodCallExpression = new(LastObjectTokenExpression, null, OnYield);
                            if (LastStatement is AssignmentStatement LastAssignmentStatement) {
                                LastAssignmentStatement.Right = DeducedMethodCallExpression;
                            }
                            else if (LastStatement is ExpressionStatement LastExpressionStatement) {
                                LastExpressionStatement.Expression = DeducedMethodCallExpression;
                            }
                            return null;
                        }
                        else {
                            throw new SyntaxErrorException($"Do block must follow method call, not {LastObjectTokenExpression.Token.Type}");
                        }
                    }
                    else {
                        throw new SyntaxErrorException($"Do block must follow method call, not {LastExpression.GetType().Name}");
                    }
                }
                else {
                    throw new SyntaxErrorException("Do block must follow method call");
                }
            }
            // End Unknown Block (internal error)
            else {
                throw new InternalErrorException($"End block not handled for type: {Block.GetType().Name}");
            }
        }
        static Statement ParseReturnOrYield(List<Phase2Object> StatementTokens, bool IsReturn) {
            // Get return/yield values
            int EndOfValuesIndex = 0;
            List<Expression>? ReturnOrYieldValues = ParseArguments(StatementTokens, ref EndOfValuesIndex);
            // Create yield/return statement
            return IsReturn ? new ReturnStatement(ReturnOrYieldValues) : new YieldStatement(ReturnOrYieldValues);
        }

        class BuildingBlock {
            public List<Statement> Statements = new();
        }
        class BuildingMethod : BuildingBlock {
            public readonly ObjectTokenExpression MethodName;
            public readonly List<MethodArgumentExpression> Arguments;
            public BuildingMethod(ObjectTokenExpression methodName, List<MethodArgumentExpression> arguments) {
                MethodName = methodName;
                Arguments = arguments;
            }
            public int RequiredArgumentsCount {
                get {
                    for (int i = 0; i < Arguments.Count; i++) {
                        if (Arguments[i].DefaultValue != null) {
                            return i;
                        }
                    }
                    return Arguments.Count;
                }
            }
        }
        class BuildingClass : BuildingBlock {
            public readonly ObjectTokenExpression ClassName;
            public BuildingClass(ObjectTokenExpression className) {
                ClassName = className;
            }
        }
        class BuildingDo : BuildingBlock {
            public readonly List<MethodArgumentExpression> Arguments;
            public BuildingDo(List<MethodArgumentExpression> arguments) {
                Arguments = arguments;
            }
        }
        public static List<Statement> GetStatements(List<Phase2Object> Phase2Objects) {
            // Get statements tokens
            List<List<Phase2Object>> StatementsTokens = SplitObjects(Phase2Objects, Phase2TokenType.EndOfStatement, out _, true);

            Stack<BuildingBlock> BlockStackInfo = new();
            BlockStackInfo.Push(new BuildingBlock());

            // Evaluate statements
            foreach (List<Phase2Object> StatementTokens in StatementsTokens) {
                bool NoBlockBuilt = false;
                // Special token
                if (StatementTokens[0] is Phase2Token Token) {
                    if (Token.Type == Phase2TokenType.Def) {
                        BuildingMethod BuildingMethod = ParseStartDefineMethod(StatementTokens);
                        // Open define method block
                        BlockStackInfo.Push(BuildingMethod);
                    }
                    else if (Token.Type == Phase2TokenType.Class) {
                        BuildingClass BuildingClass = ParseStartDefineClass(StatementTokens);
                        // Open define class block
                        BlockStackInfo.Push(BuildingClass);
                    }
                    else if (Token.Type == Phase2TokenType.End) {
                        if (BlockStackInfo.Count == 1) {
                            throw new SyntaxErrorException("Unexpected end statement");
                        }
                        Statement? FinishedStatement = ParseEndStatement(BlockStackInfo);
                        if (FinishedStatement != null)
                            BlockStackInfo.Peek().Statements.Add(FinishedStatement);
                    }
                    else if (Token.Type == Phase2TokenType.Return) {
                        BlockStackInfo.Peek().Statements.Add(ParseReturnOrYield(StatementTokens, true));
                    }
                    else if (Token.Type == Phase2TokenType.Yield) {
                        BlockStackInfo.Peek().Statements.Add(ParseReturnOrYield(StatementTokens, false));
                    }
                    else {
                        // BlockStackInfo.Peek().Statements.Add(new ExpressionStatement(ObjectsToExpression(StatementTokens)));
                        NoBlockBuilt = true;
                    }
                }
                else {
                    NoBlockBuilt = true;
                }

                if (NoBlockBuilt) {
                    // Find do statement
                    List<Phase2Object> UnlockedStatementTokens = StatementTokens;
                    int FindDoStatement = StatementTokens.FindIndex(Obj => Obj is Phase2Token Tok && Tok.Type == Phase2TokenType.Do);
                    if (FindDoStatement != -1) {
                        UnlockedStatementTokens = StatementTokens.GetRange(0, FindDoStatement);
                    }
                    // Parse assignment or expression
                    AssignmentStatement? Assignment = ParseAssignmentStatement(UnlockedStatementTokens);
                    if (Assignment != null) {
                        BlockStackInfo.Peek().Statements.Add(Assignment);
                    }
                    else {
                        BlockStackInfo.Peek().Statements.Add(new ExpressionStatement(ObjectsToExpression(UnlockedStatementTokens)));
                    }
                    // Parse do statement
                    if (FindDoStatement != -1) {
                        BuildingDo BuildingDo = new(new()); // TODO: Change nested new() to the actual |arguments|
                        BlockStackInfo.Push(BuildingDo);
                    }
                }
            }
            if (BlockStackInfo.Count != 1) {
                throw new SyntaxErrorException("Block was never closed with an end statement");
            }
            return BlockStackInfo.Pop().Statements;
        }
        public static List<Statement> GetStatements(List<Phase1Token> Phase1Tokens) {
            List<Phase2Token> Phase2Tokens = TokensToPhase2(Phase1Tokens);
            return GetStatements(Phase2Tokens);
        }
        public static List<Statement> GetStatements(List<Phase2Token> Phase2Tokens) {
            List<Phase2Object> Phase2Objects = new(Phase2Tokens);
            return GetStatements(Phase2Objects);
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
    }
}
