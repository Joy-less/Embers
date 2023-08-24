using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Embers.Interpreter;
using static Embers.Phase1;

namespace Embers
{
    public static class Phase2
    {
        public abstract class Phase2Object {
            public abstract string Inspect();
        }

        public enum Phase2TokenType {
            LocalVariableOrMethod,
            GlobalVariable,
            Constant,
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
        }

        public abstract class Expression : Phase2Object { }
        public class ValueExpression : Expression {
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
            public ValueExpression MethodName;
            public List<Expression> Arguments;
            public MethodCallExpression(ValueExpression methodName, List<Expression> arguments) {
                MethodName = methodName;
                Arguments = arguments;
            }
            public override string Inspect() {
                return MethodName.Inspect() + "(" + InspectList(Arguments) + ")";
            }
        }
        public class DefinedExpression : Expression {
            public Expression Expression;
            public DefinedExpression(Expression expression) {
                Expression = expression;
            }
            public override string Inspect() {
                return "defined? (" + Expression.Inspect() + ")";
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
        }
        public class AssignmentStatement : Statement {
            public Expression Left;
            public string Operator;
            public Expression Right;
            public AssignmentStatement(Expression left, string op, Expression right) {
                Left = left;
                Operator = op;
                Right = right;
            }
            public override string Inspect() {
                return Left.Inspect() + " " + Operator + " " + Right.Inspect();
            }
        }
        /*public class SetScopeStatement : Statement {
            public Scope Scope;
            public SetScopeStatement(Scope scope) {
                Scope = scope;
            }
            public override string Inspect() {
                return $"Set scope to {Scope}";
            }
        }*/
        public class DefineMethodStatement : Statement {
            public ValueExpression MethodName;
            public Method Method;
            public DefineMethodStatement(ValueExpression methodName, Method method) {
                MethodName = methodName;
                Method = method;
            }
            public override string Inspect() {
                return "def " + MethodName.Inspect();
            }
        }
        public class UndefineMethodStatement : Statement {
            public ValueExpression MethodName;
            public UndefineMethodStatement(ValueExpression methodName) {
                MethodName = methodName;
            }
            public override string Inspect() {
                return "undef " + MethodName.Inspect();
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
                IdentifierType = Phase2TokenType.Constant;
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
            return IsVariableToken(Type)
                || Type == Phase2TokenType.Nil
                || Type == Phase2TokenType.True
                || Type == Phase2TokenType.False
                || Type == Phase2TokenType.String
                || Type == Phase2TokenType.Integer
                || Type == Phase2TokenType.Float;
        }
        static bool IsObjectToken(Phase2Token? Token) {
            return Token != null && IsObjectToken(Token.Type);
        }
        public static bool IsVariableToken(Phase2TokenType? Type) {
            return Type == Phase2TokenType.LocalVariableOrMethod
                || Type == Phase2TokenType.GlobalVariable
                || Type == Phase2TokenType.Constant
                || Type == Phase2TokenType.InstanceVariable
                || Type == Phase2TokenType.ClassVariable;
        }
        static string InspectList<T>(List<T> List, string Separator = ", ") where T : Phase2Object {
            string ListInspection = "";
            foreach (Phase2Object Object in List) {
                if (ListInspection.Length != 0)
                    ListInspection += Separator;
                ListInspection += Object.Inspect();
            }
            return ListInspection;
        }

        static List<Phase2Object> TokensToPhase2(List<Phase1Token> Tokens) {
            // Phase 1 tokens to phase 2 tokens
            List<Phase2Token> NewTokens = new();
            for (int i = 0; i < Tokens.Count; i++) {
                Phase1Token Token = Tokens[i];

                /*Phase1Token? NextToken = i + 1 < Tokens.Count ? Tokens[i + 1] : null;
                Phase1TokenType? NextTokenType = NextToken?.Type;

                if (NextTokenType == Phase1TokenType.Comma) {

                }*/

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
                        Phase1TokenType.Comma => new Phase2Token(Phase2TokenType.Comma, Token.Value, Token.FollowsWhitespace),
                        Phase1TokenType.OpenBracket => new Phase2Token(Phase2TokenType.OpenBracket, Token.Value, Token.FollowsWhitespace),
                        Phase1TokenType.CloseBracket => new Phase2Token(Phase2TokenType.CloseBracket, Token.Value, Token.FollowsWhitespace),
                        Phase1TokenType.EndOfStatement => new Phase2Token(Phase2TokenType.EndOfStatement, Token.Value, Token.FollowsWhitespace),
                        _ => throw new InternalErrorException($"Conversion of {Token.Type} from phase 1 to phase 2 not supported")
                    });
                }
            }

            // Paths
            List<Phase2Object> NewTokens2 = new();
            for (int i = 0; i < NewTokens.Count; i++) {
                Phase2Token Token = NewTokens[i];

                if (IsObjectToken(Token)) {
                    List<Phase2Token> Path = new() {Token};
                    for (i++; i < NewTokens.Count; i++) {
                        if (NewTokens[i].Type != Phase2TokenType.Dot) {
                            i--;
                            break;
                        }
                        i++;
                        if (i >= NewTokens.Count || !IsObjectToken(NewTokens[i])) {
                            throw new SyntaxErrorException("Expected value after .");
                        }
                        Path.Add(NewTokens[i]);
                    }
                    NewTokens2.Add(new ValueExpression(Path));
                }
                else {
                    NewTokens2.Add(Token);
                }
            }

            return NewTokens2;
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
        static List<Expression> BuildArguments(List<Phase2Object> Objects, ref int IndexBeforeArguments, bool WrappedInBrackets) {
            List<Expression> Arguments = new();

            bool AcceptArgument = true;
            while (IndexBeforeArguments < Objects.Count) {
                int AddArgument(int Index) {
                    List<Phase2Object> Argument = GetTokensUntil(Objects, ref Index, obj =>
                        obj is Phase2Token tok && (tok.Type == Phase2TokenType.Comma || tok.Type == Phase2TokenType.CloseBracket));
                    Arguments.Add(ObjectsToExpression(Argument));
                    Index--;
                    AcceptArgument = false;
                    return Index;
                }
                if (Objects[IndexBeforeArguments] is Phase2Token Token) {
                    if (Token.Type == Phase2TokenType.Comma) {
                        if (AcceptArgument)
                            throw new SyntaxErrorException("Expected argument before ','");
                        AcceptArgument = true;
                    }
                    else if (Token.Type == Phase2TokenType.CloseBracket) {
                        if (WrappedInBrackets)
                            IndexBeforeArguments++;
                        break;
                    }
                    else {
                        IndexBeforeArguments = AddArgument(IndexBeforeArguments);
                    }
                }
                else {
                    IndexBeforeArguments = AddArgument(IndexBeforeArguments);
                }
                IndexBeforeArguments++;
            }
            return Arguments;
        }
        static List<Expression> ParseArgumentsWithBrackets(List<Phase2Object> Objects, ref int IndexBeforeArguments) {
            IndexBeforeArguments += 2;
            List<Expression> Arguments = BuildArguments(Objects, ref IndexBeforeArguments, true);
            return Arguments;
        }
        static List<Expression> ParseArgumentsWithoutBrackets(List<Phase2Object> Objects, ref int IndexBeforeArguments) {
            IndexBeforeArguments++;
            List<Expression> Arguments = BuildArguments(Objects, ref IndexBeforeArguments, false);
            return Arguments;
        }
        static List<Expression>? ParseArguments(List<Phase2Object> Objects, ref int IndexBeforeArguments) {
            if (IndexBeforeArguments + 1 < Objects.Count) {
                Phase2Object NextObject = Objects[IndexBeforeArguments + 1];
                if (NextObject is Phase2Token NextToken && NextToken.Type == Phase2TokenType.OpenBracket) {
                    if (!NextToken.FollowsWhitespace) {
                        return ParseArgumentsWithBrackets(Objects, ref IndexBeforeArguments);
                    }
                    else {
                        return ParseArgumentsWithoutBrackets(Objects, ref IndexBeforeArguments);
                    }
                }
                else if (NextObject is not Phase2Token && NextObject != null) {
                    return ParseArgumentsWithoutBrackets(Objects, ref IndexBeforeArguments);
                }
            }
            return null;
        }
        static List<Expression> ObjectsToExpressions(List<Phase2Object> Phase2Objects) {
            List<Phase2Object> ParsedObjects = new(Phase2Objects);

            // Brackets
            

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
                                ParsedObjects.Insert(i, new MethodCallExpression(LastExpression."+", NextExpression));
                                // ParsedObjects.Insert(i, new MethodCallExpression(LastExpression, Token.Value!, NextExpression));
                            }
                            else {
                                throw new SyntaxErrorException("Arithmetic operator must be between two expressions");
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

                if (UnknownToken is ValueExpression Value && (Value.MainType == Phase2TokenType.LocalVariableOrMethod || Value.MainType == Phase2TokenType.Constant)) {
                    int EndOfArgumentsIndex = i;
                    List<Expression>? Arguments = ParseArguments(ParsedObjects, ref EndOfArgumentsIndex);
                    if (Arguments != null) {
                        ParsedObjects.RemoveRange(i, EndOfArgumentsIndex - i);
                        ParsedObjects.Insert(i, new MethodCallExpression(Value, Arguments));
                    }
                }
            }

            // Convert objects to expressions
            List<Expression> Expressions = new();
            foreach (Phase2Object ParsedObject in ParsedObjects) {
                if (ParsedObject is Expression ParsedExpression)
                    Expressions.Add(ParsedExpression);
                else
                    throw new InternalErrorException($"Parsed object should be an expression (got {ParsedObject.GetType().Name} ({ParsedObject.Inspect()}))");
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

        class BuildingBlock {
            public List<Statement> Statements = new();
        }
        class BuildingMethod : BuildingBlock {
            public readonly ValueExpression MethodName;
            public BuildingMethod(ValueExpression methodName) {
                MethodName = methodName;
            }
        }
        public static List<Statement> GetStatements(List<Phase2Object> Phase2Objects) {
            // List<Statement> Statements = new();

            // Get statements tokens
            List<List<Phase2Object>> StatementsTokens = SplitObjects(Phase2Objects, Phase2TokenType.EndOfStatement, out _, true);

            // Stack<Block> BlockStack = new();
            Stack<BuildingBlock> BlockStackInfo = new();
            Stack<List<Statement>> BlockStackStatements = new();

            // BlockStack.Push(new Scope(null));
            BlockStackInfo.Push(new BuildingBlock());
            BlockStackStatements.Push(new List<Statement>());

            // Evaluate statements
            foreach (List<Phase2Object> StatementTokens in StatementsTokens) {
                List<List<Phase2Object>> Expressions = SplitObjects(StatementTokens, Phase2TokenType.AssignmentOperator, out List<string?> AssignmentOperators, false);

                // Assignment
                if (AssignmentOperators.Count != 0) {
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

                    BlockStackStatements.Peek().Add(CurrentAssignment);
                }
                else {
                    // Expression
                    if (Expressions.Count == 1) {
                        List<Phase2Object> Objects = Expressions[0];

                        void ExpectEndOfStatement(int CurrentIndex) {
                            if (CurrentIndex < Objects.Count - 1) {
                                throw new SyntaxErrorException($"Expected end of statement, got {InspectList(Objects.GetRange(CurrentIndex, Objects.Count - CurrentIndex))}");
                            }
                        }

                        // Keywords
                        int LastObjectIndexInStatement = -1;
                        if (Objects[0] is Phase2Token Token) {
                            // def
                            if (Token.Type == Phase2TokenType.Def) {
                                if (Objects.Count == 1 || Objects[1] is not ValueExpression MethodName) {
                                    throw new SyntaxErrorException("Def keyword must be followed by an identifier");
                                }
                                BlockStackInfo.Push(new BuildingMethod(MethodName));
                                BlockStackStatements.Push(new List<Statement>());
                                LastObjectIndexInStatement = 1;
                            }
                            // end
                            else if (Token.Type == Phase2TokenType.End) {
                                if (BlockStackInfo.Count == 1) {
                                    throw new SyntaxErrorException("Unexpected end statement");
                                }
                                BuildingBlock Block = BlockStackInfo.Pop();
                                List<Statement> BlockStatements = BlockStackStatements.Pop();
                                if (Block is BuildingMethod MethodBlock) {
                                    BlockStackInfo.Peek().Statements.Add(new DefineMethodStatement(MethodBlock.MethodName,
                                        new Method(async (Interpreter Interpreter, InstanceOrBlock InstanceOrBlock, List<Instance> Arguments) => {
                                            return await Interpreter.InterpretAsync(BlockStatements);
                                        }, 0
                                    )));
                                }
                                else {
                                    throw new InternalErrorException($"Unrecognised block type: {Block.GetType().Name}");
                                }
                                LastObjectIndexInStatement = 0;
                            }
                        }
                        if (LastObjectIndexInStatement != -1) {
                            ExpectEndOfStatement(LastObjectIndexInStatement);
                        }
                        // Expression
                        else {
                            BlockStackStatements.Peek().Add(new ExpressionStatement(ObjectsToExpression(Expressions[0])));
                        }
                    }
                    // Empty
                    else if (Expressions.Count == 0) {
                        // Pass
                    }
                    // Error
                    else {
                        throw new InternalErrorException($"Invalid statement ({Expressions.Count} expressions, {AssignmentOperators.Count} assignment operators)");
                    }
                }
            }
            if (BlockStackStatements.Count != 0) {
                if (BlockStackStatements.Count == 1) {
                    BlockStackInfo.Peek().Statements.AddRange(BlockStackStatements.Pop());
                }
                else {
                    throw new SyntaxErrorException("Block was never closed with an end statement");
                }
            }
            return BlockStackInfo.Pop().Statements;
        }
        public static List<Statement> GetStatements(List<Phase1Token> Phase1Tokens) {
            List<Phase2Object> Phase2Objects = TokensToPhase2(Phase1Tokens);
            return GetStatements(Phase2Objects);
        }
    }
}
