using System.Collections.Generic;
using System.Threading.Tasks;

namespace Embers {
    public sealed class Scope : RubyObject {
        public readonly Scope? Parent;

        private readonly IDictionary<string, Instance> Variables;

        /// <summary>Creates a root scope and axis with the given axis options.</summary>
        public Scope(AxisOptions? axis_options = null) : base(new CodeLocation(new Axis(axis_options))) {
            // Create dictionary
            Variables = Axis.CreateDictionary<string, Instance>();
        }
        /// <summary>Creates a root scope within an axis.</summary>
        public Scope(Axis axis) : base(new CodeLocation(axis)) {
            // Create dictionary
            Variables = Axis.CreateDictionary<string, Instance>();
        }
        /// <summary>Creates a child scope within another scope.</summary>
        public Scope(Scope parent, CodeLocation? location = null) : base(location ?? parent.Location) {
            Parent = parent;

            // Create dictionary
            Variables = Axis.CreateDictionary<string, Instance>();
        }

        public override string ToString()
            => "scope";

        public Expression[] Parse(string Code) {
            // Lexer
            List<Token?> Tokens = Lexer.Analyse(Location, Code);
            // Parser
            Expression[] Expressions = Parser.ParseNullSeparatedExpressions(new CodeLocation(Axis), Tokens.CastTo<RubyObject?>());
            // System.Console.WriteLine(Expressions.ObjectsToString(";\n"));
            // Return
            return Expressions;
        }
        public Instance Interpret(Expression[] Expressions) {
            return Expressions.Interpret(new Context(new CodeLocation(Axis), this));
        }
        public async Task<Instance> InterpretAsync(Expression[] Expressions) {
            return await Task.Run(() => Interpret(Expressions));
        }
        public Instance Evaluate(string Code) {
            Expression[] Expressions = Parse(Code);
            return Interpret(Expressions);
        }
        public async Task<Instance> EvaluateAsync(string Code) {
            return await Task.Run(() => Evaluate(Code));
        }
        public Instance SetVariable(string VariableName, Instance Value) {
            return Variables[VariableName] = Value;
        }
        public Instance SetVariable(string VariableName, object? Value) {
            return SetVariable(VariableName, Adapter.GetInstance(new Context(new CodeLocation(Axis), this), Value));
        }
        public Instance? GetVariable(string VariableName) {
            // Search for member in hierarchy
            Scope? Current = this;
            do {
                // Search current
                if (Current.Variables.TryGetValue(VariableName, out Instance? Value)) {
                    return Value;
                }
                // Search parent
                Current = Current.Parent;
            } while (Current is not null);
            // Member not found
            return null;
        }
        public List<string> GetVariableNames() {
            // Search for member names in hierarchy
            List<string> MemberNames = new();
            Scope? Current = this;
            do {
                // Search current
                MemberNames.AddRange(Current.Variables.Keys);
                // Search parent
                Current = Current.Parent;
            } while (Current is not null);
            // Return all member names
            return MemberNames;
        }
    }
}
