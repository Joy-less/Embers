using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Embers {
    public abstract class Expression : RubyObject {
        public Expression(CodeLocation location) : base(location) { }
        public abstract Instance Interpret(Context Context);
        public async Task<Instance> InterpretAsync(Context Context)
            => await Task.Run(() => Interpret(Context));
    }
    public abstract class ReferenceExpression : Expression {
        public readonly string Name;
        public readonly bool PossibleConstant;
        public ReferenceExpression(CodeLocation location, string name) : base(location) {
            Name = name;
            PossibleConstant = name.IsNamedLikeConstant();
        }
        public abstract Instance Assign(Context Context, Instance Value);
    }
    public class IdentifierExpression : ReferenceExpression {
        public IdentifierExpression(CodeLocation location, string name) : base(location, name) { }
        public override string ToString()
            => Name;
        public override Instance Interpret(Context Context) => Interpreter.InterpretIdentifier(Context, this);
        public override Instance Assign(Context Context, Instance Value) => throw new InteropError();
    }
    public class LocalExpression : ReferenceExpression {
        public LocalExpression(CodeLocation location, string name) : base(location, name) { }
        public override string ToString()
            => Name;
        public override Instance Interpret(Context Context) => Interpreter.InterpretLocal(Context, this);
        public override Instance Assign(Context Context, Instance Value) => Interpreter.InterpretLocalAssignment(Context, this, Value);
    }
    public class ConstantExpression : ReferenceExpression {
        public ConstantExpression(CodeLocation location, string name) : base(location, name) { }
        public override string ToString()
            => Name;
        public override Instance Interpret(Context Context) => Interpreter.InterpretConstant(Context, this);
        public override Instance Assign(Context Context, Instance Value) => Interpreter.InterpretConstantAssignment(Context, this, Value);
    }
    public class GlobalExpression : ReferenceExpression {
        public GlobalExpression(CodeLocation location, string name) : base(location, name) { }
        public override string ToString()
            => Name;
        public override Instance Interpret(Context Context) => Interpreter.InterpretGlobal(Context, this);
        public override Instance Assign(Context Context, Instance Value) => Interpreter.InterpretGlobalAssignment(Context, this, Value);
    }
    public class ClassVariableExpression : ReferenceExpression {
        public ClassVariableExpression(CodeLocation location, string name) : base(location, name) { }
        public override string ToString()
            => Name;
        public override Instance Interpret(Context Context) => Interpreter.InterpretClassVariable(Context, this);
        public override Instance Assign(Context Context, Instance Value) => Interpreter.InterpretClassVariableAssignment(Context, this, Value);
    }
    public class InstanceVariableExpression : ReferenceExpression {
        public InstanceVariableExpression(CodeLocation location, string name) : base(location, name) { }
        public override string ToString()
            => Name;
        public override Instance Interpret(Context Context) => Interpreter.InterpretInstanceVariable(Context, this);
        public override Instance Assign(Context Context, Instance Value) => Interpreter.InterpretInstanceVariableAssignment(Context, this, Value);
    }
    public class ConstantPathExpression : ReferenceExpression {
        public readonly ReferenceExpression Parent;
        public ConstantPathExpression(ReferenceExpression parent, string name) : base(parent.Location, name) {
            Parent = parent;
        }
        public override string ToString()
            => $"{Parent}::{Name}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretConstantPath(Context, this);
        public override Instance Assign(Context Context, Instance Value) => Interpreter.InterpretConstantPathAssignment(Context, this, Value);
    }
    public class MethodCallExpression : ReferenceExpression {
        public readonly Expression? Parent;
        public readonly Expression[] Arguments;
        public readonly Method? Block;
        public readonly bool SafeNavigation;
        public MethodCallExpression(CodeLocation location, Expression? parent, string name, Expression[]? arguments = null, Method? block = null, bool safe_navigation = false) : base(location, name) {
            Parent = parent;
            Arguments = arguments ?? System.Array.Empty<Expression>();
            Block = block;
            SafeNavigation = safe_navigation;
        }
        public override string ToString()
            => $"{(Parent is not null ? $"{Parent}." : "")}{Name}({Arguments.ObjectsToString()}){(Block is not null ? $" {Block}" : "")}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretMethodCall(Context, this);
        public override Instance Assign(Context Context, Instance Value) => Interpreter.InterpretMethodCallAssignment(Context, this, Value);
    }
    public class SelfExpression : ReferenceExpression {
        public SelfExpression(CodeLocation location) : base(location, "self") { }
        public override string ToString()
            => "self";
        public override Instance Interpret(Context Context) => Interpreter.InterpretSelf(Context, this);
        public override Instance Assign(Context Context, Instance Value) => throw new RuntimeError($"{Location}: cannot assign to self");
    }
    public class TokenLiteralExpression : Expression {
        public readonly Token Token;
        public readonly Func<Instance> CreateLiteral;
        public TokenLiteralExpression(Token token) : base(token.Location) {
            Token = token;
            string Value = Token.Value!;

            // Integer
            if (Token.Type is TokenType.Integer) {
                Integer Integer = Integer.Parse(Value);
                CreateLiteral = () => new Instance(Axis.Integer, Integer);
            }
            // Float
            else if (Token.Type is TokenType.Float) {
                Float Float = Float.Parse(Value);
                CreateLiteral = () => new Instance(Axis.Float, Float);
            }
            // Other
            else {
                CreateLiteral = Token.Type switch {
                    TokenType.NilTrueFalse when Value is "nil" => () => Axis.Nil,
                    TokenType.NilTrueFalse when Value is "true" => () => Axis.True,
                    TokenType.NilTrueFalse when Value is "false" => () => Axis.False,
                    TokenType.String => () => new Instance(Axis.String, Value),
                    TokenType.Symbol => () => Axis.Globals.GetImmortalSymbol(Value),
                    _ => throw new InternalError($"{Token.Location}: can't create literal expression from '{Token.Type}'"),
                };
            }
        }
        public override string ToString()
            => Token.Value!;
        public override Instance Interpret(Context Context) => Interpreter.InterpretTokenLiteral(Context, this);
    }
    public class FormattedStringExpression : Expression {
        public readonly string Value;
        public readonly object[] Components;
        public FormattedStringExpression(CodeLocation location, string value, object[] components) : base(location) {
            Value = value;
            Components = components;
        }
        public override string ToString()
            => Value;
        public override Instance Interpret(Context Context) => Interpreter.InterpretFormattedString(Context, this);
    }
    public class LineExpression : Expression {
        public LineExpression(CodeLocation location) : base(location) { }
        public override string ToString()
            => "__LINE__";
        public override Instance Interpret(Context Context) => Interpreter.InterpretLine(Context, this);
    }
    public class FileExpression : Expression {
        public FileExpression(CodeLocation location) : base(location) { }
        public override string ToString()
            => "__FILE__";
        public override Instance Interpret(Context Context) => Interpreter.InterpretFile(Context, this);
    }
    public class BlockGivenExpression : Expression {
        public BlockGivenExpression(CodeLocation location) : base(location) { }
        public override string ToString()
            => "block_given?";
        public override Instance Interpret(Context Context) => Interpreter.InterpretBlockGiven(Context, this);
    }
    public class AssignmentExpression : Expression {
        public readonly ReferenceExpression Target;
        public readonly Expression Value;
        public AssignmentExpression(ReferenceExpression target, Expression value) : base(target.Location) {
            Target = target;
            Value = value;
        }
        public override string ToString()
            => $"{Target} = {Value}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretAssignment(Context, this);
    }
    public class MultiAssignmentExpression : Expression {
        public readonly AssignmentExpression[] Assignments;
        public MultiAssignmentExpression(CodeLocation location, AssignmentExpression[] assignments) : base(location) {
            Assignments = assignments;
        }
        public override string ToString()
            => Assignments.ObjectsToString();
        public override Instance Interpret(Context Context) => Interpreter.InterpretMultiAssignment(Context, this);
    }
    public class ExpandAssignmentExpression : Expression {
        public readonly ReferenceExpression[] Targets;
        public readonly Expression Value;
        public ExpandAssignmentExpression(CodeLocation location, ReferenceExpression[] targets, Expression value) : base(location) {
            Targets = targets;
            Value = value;
        }
        public override string ToString()
            => $"{Targets.ObjectsToString()} = {Value}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretExpandAssignment(Context, this);
    }
    public class NotExpression : Expression {
        public readonly Expression Expression;
        public NotExpression(Expression expression) : base(expression.Location) {
            Expression = expression;
        }
        public override string ToString()
            => $"not {Expression}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretNot(Context, this);
    }
    public class TernaryExpression : Expression {
        public readonly Expression Condition;
        public readonly Expression ExpressionIfTruthy;
        public readonly Expression ExpressionIfFalsey;
        public TernaryExpression(Expression condition, Expression expression_if_truthy, Expression expression_if_falsey) : base(condition.Location) {
            Condition = condition;
            ExpressionIfTruthy = expression_if_truthy;
            ExpressionIfFalsey = expression_if_falsey;
        }
        public override string ToString()
            => $"{Condition} ? {ExpressionIfTruthy} : {ExpressionIfFalsey}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretTernary(Context, this);
    }
    public class LogicExpression : Expression {
        public readonly Expression Left;
        public readonly Expression Right;
        public readonly bool IsAnd; // true: and, false: or
        public LogicExpression(Expression left, Expression right, bool is_and) : base(left.Location) {
            Left = left;
            Right = right;
            IsAnd = is_and;
        }
        public override string ToString()
            => $"{Left} {(IsAnd ? "and" : "or")} {Right}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretLogic(Context, this);
    }
    public class IfModifierExpression : Expression {
        public readonly Expression Condition;
        public readonly Expression Expression;
        public IfModifierExpression(Expression condition, Expression expression) : base(expression.Location) {
            Condition = condition;
            Expression = expression;
        }
        public override string ToString()
            => $"{Expression} if {Condition}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretIfModifier(Context, this);
    }
    public class WhileModifierExpression : Expression {
        public readonly Expression Condition;
        public readonly Expression Expression;
        public WhileModifierExpression(Expression condition, Expression expression) : base(expression.Location) {
            Condition = condition;
            Expression = expression;
        }
        public override string ToString()
            => $"{Expression} while {Condition}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretWhileModifier(Context, this);
    }
    public class RescueModifierExpression : Expression {
        public readonly Expression Expression;
        public readonly Expression Rescue;
        public RescueModifierExpression(Expression expression, Expression rescue) : base(expression.Location) {
            Expression = expression;
            Rescue = rescue;
        }
        public override string ToString()
            => $"{Expression} rescue {Rescue}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretRescueModifier(Context, this);
    }
    public class ArrayExpression : Expression {
        public readonly Expression[] Items;
        public readonly bool WhitespaceBefore;
        public ArrayExpression(CodeLocation location, Expression[] items, bool whitespace_before = true) : base(location) {
            Items = items;
            WhitespaceBefore = whitespace_before;
        }
        public override string ToString()
            => $"[{Items.ObjectsToString()}]";
        public override Instance Interpret(Context Context) => Interpreter.InterpretArray(Context, this);
    }
    public class HashExpression : Expression {
        public readonly IDictionary<Expression, Expression> Items;
        public HashExpression(CodeLocation location, IDictionary<Expression, Expression> items) : base(location) {
            Items = items;
        }
        public override string ToString()
            => $"{{{Items.ObjectsToString()}}}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretHash(Context, this);
    }
    public class KeyValuePairExpression : Expression {
        public readonly Expression Key;
        public readonly Expression Value;
        public KeyValuePairExpression(Expression key, Expression value) : base(key.Location) {
            Key = key;
            Value = value;
        }
        public override string ToString()
            => $"{Key} => {Value}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretKeyValuePair(Context, this);
    }
    public class RangeExpression : Expression {
        public readonly Expression? Min;
        public readonly Expression? Max;
        public readonly bool ExcludeEnd;
        public RangeExpression(CodeLocation location, Expression? min, Expression? max, bool exclude_end) : base(location) {
            Min = min;
            Max = max;
            ExcludeEnd = exclude_end;
        }
        public override string ToString()
            => $"{Min}{(ExcludeEnd ? "..." : "..")}{Max}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretRange(Context, this);
    }
    public class YieldExpression : Expression {
        public readonly Expression[] Arguments;
        public YieldExpression(CodeLocation location, Expression[] arguments) : base(location) {
            Arguments = arguments;
        }
        public override string ToString()
            => Arguments.Length == 0 ? "yield" : $"yield {Arguments.ObjectsToString()}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretYield(Context, this);
    }
    public class SuperExpression : Expression {
        public readonly Expression[] Arguments;
        public SuperExpression(CodeLocation location, Expression[] arguments) : base(location) {
            Arguments = arguments;
        }
        public override string ToString()
            => Arguments.Length == 0 ? "super" : $"super {Arguments.ObjectsToString()}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretSuper(Context, this);
    }
    public class DefinedExpression : Expression {
        public readonly Expression Argument;
        public DefinedExpression(CodeLocation location, Expression argument) : base(location) {
            Argument = argument;
        }
        public override string ToString()
            => $"defined? {Argument}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretDefined(Context, this);
    }
    public class AliasExpression : Expression {
        public readonly string Alias;
        public readonly string Original;
        public AliasExpression(CodeLocation location, string alias, string original) : base(location) {
            Alias = alias;
            Original = original;
        }
        public override string ToString()
            => $"alias {Alias} {Original}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretAlias(Context, this);
    }
    public class ControlExpression : Expression {
        public readonly ControlType Type;
        public readonly Expression? Argument;
        public ControlExpression(CodeLocation location, ControlType type, Expression? argument = null) : base(location) {
            Type = type;
            Argument = argument;
        }
        public override string ToString()
            => $"{Type}".ToLower() + (Argument is not null ? $" {Argument}" : "");
        public override Instance Interpret(Context Context) => Interpreter.InterpretControl(Context, this);
    }
    public enum ControlType {
        /// <summary>Break out of a loop (same as C#'s <see langword="break"/>).</summary>
        Break,
        /// <summary>Skip the current iteration of a loop (same as C#'s <see langword="continue"/>).</summary>
        Next,
        /// <summary>Redo the current iteration of a loop (same as C#'s <see langword="goto"/> with a label at the start of a loop).</summary>
        Redo,
        /// <summary>Retry a loop entirely (same as C#'s <see langword="goto"/> before the loop).</summary>
        Retry,
        /// <summary>Return early from a method call (same as C#'s <see langword="return"/>).</summary>
        Return,
    }
    public abstract class ScopeExpression : Expression {
        public readonly Expression[] Expressions;
        public ScopeExpression(CodeLocation location, Expression[] expressions) : base(location) {
            Expressions = expressions;
        }
    }
    public class BeginExpression : ScopeExpression {
        public RescueExpression[] RescueBranches;
        public Expression[]? ElseBranch;
        public Expression[]? EnsureBranch;
        public BeginExpression(CodeLocation location, Expression[] expressions, RescueExpression[] rescue_branches, Expression[]? else_branch = null, Expression[]? ensure_branch = null) : base(location, expressions) {
            RescueBranches = rescue_branches;
            ElseBranch = else_branch;
            EnsureBranch = ensure_branch;
        }
        public override string ToString()
            => $"begin; {Expressions.ObjectsToString("; ")}; {RescueBranches.ObjectsToString("; ")}; else {ElseBranch.ObjectsToString("; ")}; ensure {EnsureBranch.ObjectsToString("; ")}; end";
        public override Instance Interpret(Context Context) => Interpreter.InterpretBegin(Context, this);
    }
    public class RescueExpression : ScopeExpression {
        public readonly Expression? ExceptionType;
        public readonly string? ExceptionVariable;
        public RescueExpression(CodeLocation location, Expression[] expressions, Expression? exception_type, string? exception_variable) : base(location, expressions) {
            ExceptionType = exception_type;
            ExceptionVariable = exception_variable;
        }
        public override string ToString()
            => $"rescue{(ExceptionType is not null ? ExceptionType : "")}{(ExceptionVariable is not null ? $" => {ExceptionVariable}" : "")}; {Expressions.ObjectsToString("; ")}; end";
        public override Instance Interpret(Context Context) => throw new InternalError($"{Location}: rescue interpreted directly");
    }
    public class IfExpression : ScopeExpression {
        public readonly Expression? Condition;
        public IfExpression(CodeLocation location, Expression[] expressions, Expression? condition) : base(location, expressions) {
            Condition = condition;
        }
        public override string ToString() {
            string ConditionString = Condition is not null ? $"if {Condition}; " : "";
            return ConditionString + Expressions.ObjectsToString("; ") + "; end";
        }
        public override Instance Interpret(Context Context) => Interpreter.InterpretIf(Context, this);
    }
    public class IfElseExpression : IfExpression {
        public readonly IfExpression ElseBranch;
        public IfElseExpression(CodeLocation location, Expression[] expressions, Expression condition, IfExpression else_branch) : base(location, expressions, condition) {
            ElseBranch = else_branch;
        }
        public override string ToString() {
            string ConditionString = Condition is not null ? $"if {Condition}; " : "";
            string ElseString = ElseBranch is not null ? $"else; {ElseBranch}; " : "";
            return ConditionString + Expressions.ObjectsToString("; ") + "; " + ElseString + "end";
        }
        public override Instance Interpret(Context Context) => Interpreter.InterpretIfElse(Context, this);
    }
    public class WhileExpression : ScopeExpression {
        public readonly Expression Condition;
        public WhileExpression(CodeLocation location, Expression[] expressions, Expression condition) : base(location, expressions) {
            Condition = condition;
        }
        public override string ToString()
            => $"while ({Condition}); {Expressions.ObjectsToString("; ")}; end";
        public override Instance Interpret(Context Context) => Interpreter.InterpretWhile(Context, this);
    }
    public class DefMethodExpression : ScopeExpression {
        public readonly ReferenceExpression? Parent;
        public readonly string Name;
        public readonly Argument[] Arguments;
        public DefMethodExpression(CodeLocation location, Expression[] expressions, ReferenceExpression? parent, string name, Argument[] arguments) : base(location, expressions) {
            Parent = parent;
            Name = name;
            Arguments = arguments;
        }
        public override string ToString()
            => $"def {Name}({Arguments.ObjectsToString()}); {Expressions.ObjectsToString("; ")}; end";
        public override Instance Interpret(Context Context) => Interpreter.InterpretDefMethod(Context, this);
    }
    public class ForExpression : ScopeExpression {
        public readonly Expression Target;
        public readonly Argument[] Arguments;
        public readonly Method Block;
        public ForExpression(CodeLocation location, Expression[] expressions, Expression target, Argument[] arguments, Method block) : base(location, expressions) {
            Target = target;
            Arguments = arguments;
            Block = block;
        }
        public override string ToString()
            => $"for {Arguments.ObjectsToString()} in {Target}; {Expressions.ObjectsToString("; ")}; end";
        public override Instance Interpret(Context Context) => Interpreter.InterpretFor(Context, this);
    }
    public class DefModuleExpression : ScopeExpression {
        public readonly string Name;
        public readonly ReferenceExpression? Super;
        public readonly bool IsClass;
        public DefModuleExpression(CodeLocation location, Expression[] expressions, string name, ReferenceExpression? super, bool is_class) : base(location, expressions) {
            Name = name;
            Super = super;
            IsClass = is_class;
        }
        public override string ToString()
            => $"class; {Expressions.ObjectsToString("; ")}; end";
        public override Instance Interpret(Context Context) => Interpreter.InterpretDefModule(Context, this);
    }
    public class CaseExpression : Expression {
        public readonly Expression Subject;
        public readonly List<WhenExpression> WhenBranches;
        public readonly Expression[]? ElseBranch;
        public CaseExpression(CodeLocation location, Expression subject, List<WhenExpression> when_branches, Expression[]? else_branch) : base(location)  {
            Subject = subject;
            WhenBranches = when_branches;
            ElseBranch = else_branch;
        }
        public override string ToString()
            => $"case ({Subject}); {WhenBranches.ObjectsToString("; ")}; else {ElseBranch}; end";
        public override Instance Interpret(Context Context) => Interpreter.InterpretCase(Context, this);
    }
    public class WhenExpression : ScopeExpression {
        public readonly Expression[] Conditions;
        public WhenExpression(CodeLocation location, Expression[] conditions, Expression[] expressions) : base(location, expressions) {
            Conditions = conditions;
        }
        public override string ToString()
            => $"when {Conditions}; {Expressions.ObjectsToString("; ")}";
        public override Instance Interpret(Context Context) => Interpreter.InterpretWhen(Context, this);
    }

    // Temporary Expressions
    /// <summary>Base class for expressions only used during the parsing stage.</summary>
    internal abstract class TemporaryExpression : Expression {
        public readonly List<RubyObject?> Objects;
        public TemporaryExpression(CodeLocation location, List<RubyObject?> objects) : base(location) {
            Objects = objects;
        }
        public override Instance Interpret(Context Context) => throw new InternalError($"{Location}: {GetType().Name} not removed by parser");
    }
    internal class TemporaryScopeExpression : TemporaryExpression {
        private readonly Func<CodeLocation, List<RubyObject?>, Expression> Creator;
        public TemporaryScopeExpression(CodeLocation location, List<RubyObject?> objects, Func<CodeLocation, List<RubyObject?>, Expression> creator) : base(location, objects) {
            Creator = creator;
        }
        public Expression Create(CodeLocation Location) {
            return Creator(Location, Objects);
        }
        public override string ToString()
            => $"[scope] {Objects.ObjectsToString()} [end]";
    }
    internal class TemporaryBlockExpression : TemporaryExpression {
        public readonly bool HighPrecedence;
        public readonly Argument[] Arguments;
        public TemporaryBlockExpression(CodeLocation location, List<RubyObject?> objects, Argument[] arguments, bool high_precedence) : base(location, objects) {
            HighPrecedence = high_precedence;
            Arguments = arguments;
        }
        public override string ToString()
            => "block";
    }
    internal abstract class TemporaryBaseBracketsExpression : TemporaryExpression {
        public Expression[]? Expressions;
        public readonly bool WhitespaceBefore;
        public TemporaryBaseBracketsExpression(CodeLocation location, List<RubyObject?> objects, bool whitespace_before) : base(location, objects) {
            WhitespaceBefore = whitespace_before;
        }
    }
    internal class TemporaryBracketsExpression : TemporaryBaseBracketsExpression {
        public TemporaryBracketsExpression(CodeLocation location, List<RubyObject?> objects, bool whitespace_before) : base(location, objects, whitespace_before) { }
        public override string ToString()
            => $"({(Expressions is not null ? Expressions.ObjectsToString() : Objects.ObjectsToString())})";
    }
    internal class TemporarySquareBracketsExpression : TemporaryBaseBracketsExpression {
        public TemporarySquareBracketsExpression(CodeLocation location, List<RubyObject?> objects, bool whitespace_before) : base(location, objects, whitespace_before) { }
        public override string ToString()
            => $"[{(Expressions is not null ? Expressions.ObjectsToString() : Objects.ObjectsToString())}]";
    }
    internal class TemporaryCurlyBracketsExpression : TemporaryBaseBracketsExpression {
        public TemporaryCurlyBracketsExpression(CodeLocation location, List<RubyObject?> objects, bool whitespace_before) : base(location, objects, whitespace_before) { }
        public override string ToString()
            => $"{{{(Expressions is not null ? Expressions.ObjectsToString() : Objects.ObjectsToString())}}}";
    }
}
