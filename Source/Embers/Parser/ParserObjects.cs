using System.Collections.Generic;

namespace Embers {
    internal abstract class Expression : RubyObject {
        public Expression(CodeLocation location) : base(location) { }
        public abstract List<Instruction> ToInstructions();
    }
    internal class CallExpression : Expression {
        public string Name;
        public Expression[] Arguments;
        public CallExpression(CodeLocation location, string name, Expression[] arguments) : base(location) {
            Name = name;
            Arguments = arguments;
        }
        public override List<Instruction> ToInstructions() {
            List<Instruction> Instructions = new();

            foreach (Expression Argument in Arguments) {
                Instructions.AddRange(Argument.ToInstructions());
            }

            return Instructions;

            /*return new Instruction[] {
                new CallInstruction(Location, Name),
            };*/
            /*foreach (Expression Argument in Arguments) {
                foreach (Instruction Instruction in Argument.ToInstructions()) {
                    yield return Instruction;
                }
            yield return new CallInstruction(Location, Name);*/
        }
    }

    /*internal abstract class ParserMultiObject : ParserObject {
        public readonly List<RubyObject?> Objects;
        public ParserMultiObject(CodeLocation location, List<RubyObject?> objects) : base(location) {
            Objects = objects;
        }
    }
    internal class ParserBrackets : ParserMultiObject {
        public readonly bool WhitespaceBefore;
        public Expression[]? Expressions;
        public ParserBrackets(CodeLocation location, List<RubyObject?> objects, bool whitespace_before) : base(location, objects) {
            WhitespaceBefore = whitespace_before;
        }
        public override string ToString()
            => "{Brackets}";
    }
    internal class ParserSquareBrackets : ParserMultiObject {
        public readonly bool WhitespaceBefore;
        public ParserSquareBrackets(CodeLocation location, List<RubyObject?> objects, bool whitespace_before) : base(location, objects) {
            WhitespaceBefore = whitespace_before;
        }
        public override string ToString()
            => "{SquareBrackets}";
    }
    internal class ParserCurlyBrackets : ParserMultiObject {
        public readonly bool WhitespaceBefore;
        public ParserCurlyBrackets(CodeLocation location, List<RubyObject?> objects, bool whitespace_before) : base(location, objects) {
            WhitespaceBefore = whitespace_before;
        }
        public override string ToString()
            => "{CurlyBrackets}";
    }
    internal class ParserDoBlock : ParserMultiObject {
        public ParserDoBlock(CodeLocation location, List<RubyObject?> objects) : base(location, objects) { }
        public override string ToString()
            => "{DoBlock}";
    }
    internal class ParserArray : ParserObject {
        public readonly bool WhitespaceBefore;
        public readonly Expression[] Items;
        public ParserArray(CodeLocation location, Expression[] items, bool whitespace_before) : base(location) {
            WhitespaceBefore = whitespace_before;
            Items = items;
        }
        public override string ToString()
            => "{Array}";
    }
    internal class ParserProc : ParserObject {
        public readonly Argument[] Arguments;
        public ParserProc(CodeLocation location, Argument[] arguments) : base(location) {
            Arguments = arguments;
        }
        public override string ToString()
            => "{Proc}";
    }
    internal class ParserIdentifier : ParserObject {
        public readonly string Name;
        public ParserIdentifier(CodeLocation location, string name) : base(location) {
            Name = name;
        }
        public override string ToString()
            => "{Identifier}";
    }
    internal class ParserIf : Expression {
        public readonly Expression Condition;
        public readonly Expression IfTruthy;
        public readonly Expression? IfFalsey;
        public ParserIf(CodeLocation location, Expression condition, Expression if_truthy, Expression? if_falsey) : base(location) {
            Condition = condition;
            IfTruthy = if_truthy;
            IfFalsey = if_falsey;
        }
        public override string ToString() => $"(If, {Condition}, {IfTruthy}, {IfFalsey})";
        public override Instance Process(ref Context Context) => Processor.ProcessIf(ref Context, this);
    }
    internal class ParserWhile : Expression {
        public readonly Expression Condition;
        public readonly Expression Repeat;
        public ParserWhile(CodeLocation location, Expression condition, Expression repeat) : base(location) {
            Condition = condition;
            Repeat = repeat;
        }
        public override string ToString() => $"(While, {Condition}, {Repeat})";
        public override Instance Process(ref Context Context) => Processor.ProcessWhile(ref Context, this);
    }
    internal class CaseExpression : Expression {
        public readonly Expression Subject;
        public readonly CaseBranch[] Branches;
        public readonly Expression? Else;
        public CaseExpression(CodeLocation location, Expression subject, CaseBranch[] branches, Expression? @else) : base(location) {
            Subject = subject;
            Branches = branches;
            Else = @else;
        }
        public override string ToString() => $"(Case, {Subject}, {Branches}, {Else})";
        public override Instance Process(ref Context Context) => Processor.ProcessCase(ref Context, this);
    }
    internal class CaseBranch : RubyObject {
        public readonly Expression Match;
        public readonly Expression IfTruthy;
        public CaseBranch(CodeLocation location, Expression match, Expression if_truthy) : base(location) {
            Match = match;
            IfTruthy = if_truthy;
        }
        public override string ToString() => $"(CaseBranch, {Match}, {IfTruthy})";
    }
    internal class ProtectExpression : Expression {
        public readonly Expression Try;
        public readonly Expression? RescueClass;
        public readonly Expression Rescue;
        public ProtectExpression(CodeLocation location, Expression @try, Expression? rescue_class, Expression rescue) : base(location) {
            Try = @try;
            RescueClass = rescue_class;
            Rescue = rescue;
        }
        public override string ToString() => $"(Protect, {Try}, {RescueClass}, {Rescue})";
        public override Instance Process(ref Context Context) => Processor.ProcessProtect(ref Context, this);
    }
    internal class EnsureExpression : Expression {
        public readonly Expression Try;
        public readonly Expression Ensure;
        public EnsureExpression(CodeLocation location, Expression @try, Expression ensure) : base(location) {
            Try = @try;
            Ensure = ensure;
        }
        public override string ToString() => $"(Ensure, {Try}, {Ensure})";
        public override Instance Process(ref Context Context) => Processor.ProcessEnsure(ref Context, this);
    }
    internal class DefExpression : Expression {
        public readonly Expression? Target;
        public readonly string Name;
        public readonly Argument[] Arguments;
        public readonly Expression Expression;
        public DefExpression(CodeLocation location, Expression? target, string name, Argument[] arguments, Expression expression) : base(location) {
            Target = target;
            Name = name;
            Arguments = arguments;
            Expression = expression;
        }
        public override string ToString() => $"(Def, {Target}, {Name}, {Arguments}, {Expression})";
        public override Instance Process(ref Context Context) => Processor.ProcessDef(ref Context, this);
    }
    internal class ClassExpression : Expression {
        public readonly string Name;
        public readonly Expression Super;
        public readonly Expression Expression;
        public ClassExpression(CodeLocation location, string name, Expression super, Expression expression) : base(location) {
            Name = name;
            Super = super;
            Expression = expression;
        }
        public override string ToString() => $"(Class, {Name}, {Super}, {Expression})";
        public override Instance Process(ref Context Context) => Processor.ProcessClass(ref Context, this);
    }*/
}
