using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Embers {
    public sealed class Scope : RubyObject {
        public readonly Scope? Parent;

        private readonly IDictionary<int, Instance> Variables;

        /// <summary>Creates a root scope and axis with the given axis options.</summary>
        public Scope(AxisOptions? axis_options = null) : base(new CodeLocation(new Axis(axis_options))) {
            Variables = Axis.CreateDictionary<int, Instance>();
        }
        /// <summary>Creates a root scope within an axis.</summary>
        public Scope(Axis axis) : base(new CodeLocation(axis)) {
            Variables = Axis.CreateDictionary<int, Instance>();
        }
        /// <summary>Creates a child scope within another scope.</summary>
        public Scope(Scope parent, CodeLocation? location = null) : base(location ?? parent.Location) {
            Parent = parent;
            Variables = Axis.CreateDictionary<int, Instance>();
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
        internal Instance SetVariable(int VariableNameKey, Instance Value) {
            if (Parent is not null) {
                // Try reassign variable in parent scope
                Scope? Current = this;
                while (Current is not null) {
                    if (Current.Variables.ContainsKey(VariableNameKey)) {
                        return Current.Variables[VariableNameKey] = Value;
                    }
                    Current = Current.Parent;
                }
            }
            return Variables[VariableNameKey] = Value;
        }
        public Instance SetVariable(string VariableName, Instance Value) {
            return SetVariable(Axis.Globals.GetNameKey(VariableName), Value);
        }
        public Instance SetVariable(string VariableName, object? Value) {
            return SetVariable(VariableName, Adapter.GetInstance(new Context(new CodeLocation(Axis), this), Value));
        }
        internal Instance? GetVariable(int VariableNameKey) {
            return GetMember(Scope => Scope.Variables, VariableNameKey);
        }
        public Instance? GetVariable(string VariableName) {
            return GetVariable(Axis.Globals.GetNameKey(VariableName));
        }
        public List<string> GetVariableNames() {
            return GetMemberNames(Scope => Scope.Variables);
        }

        private T? GetMember<T>(Func<Scope, IDictionary<int, T>> Members, int MemberNameKey) where T : class {
            // Search for member in hierarchy
            Scope? Current = this;
            while (Current is not null) {
                // Search for member in module
                if (Members(Current).TryGetValue(MemberNameKey, out T? Member)) {
                    return Member;
                }
                Current = Current.Parent;
            }
            // Member not found
            return null;
        }
        private List<string> GetMemberNames<T>(Func<Scope, IDictionary<int, T>> Members) {
            List<string> MemberNames = new();
            // Get member names in hierarchy
            Scope? Current = this;
            while (Current is not null) {
                // Get member names in module
                MemberNames.AddRange(Members(Current).Keys.Select(Axis.Globals.GetName));
                Current = Current.Parent;
            }
            // Return member names
            return MemberNames;
        }
    }
}
