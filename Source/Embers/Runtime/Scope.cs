using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Embers {
    public sealed class Scope : RubyObject {
        public readonly Scope? Parent;

        private readonly IDictionary<string, Instance> Variables;

        /// <summary>Creates a root scope and axis with the given axis options.</summary>
        public Scope(AxisOptions? axis_options = null) : base(new CodeLocation(new Axis(axis_options))) {
            Variables = Axis.CreateDictionary<string, Instance>();
        }
        /// <summary>Creates a root scope within an axis.</summary>
        public Scope(Axis axis) : base(new CodeLocation(axis)) {
            Variables = Axis.CreateDictionary<string, Instance>();
        }
        /// <summary>Creates a child scope within another scope.</summary>
        public Scope(Scope parent, CodeLocation? location = null) : base(location ?? parent.Location) {
            Parent = parent;
            Variables = Axis.CreateDictionary<string, Instance>();
        }

        public override string ToString()
            => "scope";

        public Expression[] Parse(string Code) {
            // Lexer
            List<Token?> Tokens = Lexer.Analyse(Location, Code);
            // Parser
            Expression[] Expressions = Parser.ParseNullSeparatedExpressions(new CodeLocation(Axis), Tokens.CastTo<RubyObject?>());
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
            foreach (Scope Scope in Hierarchy) {
                if (Scope.Variables.ContainsKey(VariableName)) {
                    return Scope.Variables[VariableName] = Value;
                }
            }
            return Variables[VariableName] = Value;
        }
        public Instance SetVariable(string VariableName, object? Value) {
            return SetVariable(VariableName, Adapter.GetInstance(new Context(new CodeLocation(Axis), this), Value));
        }
        public Instance? GetVariable(string VariableName) {
            // Search for variable in hierarchy
            foreach (Scope Scope in Hierarchy) {
                // Search scope for variable
                if (Scope.Variables.TryGetValue(VariableName, out Instance? Value)) {
                    return Value;
                }
            }
            // Variable not found
            return null;
        }
        public List<string> GetVariableNames() {
            List<string> VariableNames = new();
            // Search for variable names in hierarchy
            foreach (Scope Scope in Hierarchy) {
                // Get variable names in scope
                VariableNames.AddRange(Scope.Variables.Keys);
            }
            // Return all variable names
            return VariableNames;
        }
        public IEnumerable<Scope> Hierarchy {
            get {
                Scope Current = this;
                while (true) {
                    yield return Current;
                    if (Current.Parent is null) {
                        break;
                    }
                    Current = Current.Parent;
                }
            }
        }
    }
}
