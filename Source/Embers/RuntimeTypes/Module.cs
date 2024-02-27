using System;
using System.Collections.Generic;
using System.Linq;

namespace Embers {
    public class Module : Instance {
        public readonly Scope Scope;
        public readonly string Name;
        public Class? SuperClass;

        protected readonly IDictionary<int, Method> ClassMethods;
        protected readonly IDictionary<int, Instance> ClassVariables;
        protected readonly IDictionary<int, Instance> Constants;

        public Module(Module parent, string name) : base(parent.Axis.Module) {
            Scope = parent.Scope;
            Name = name;
            SuperClass = Class;

            // Create dictionaries
            ClassMethods = Axis.CreateDictionary<int, Method>();
            ClassVariables = Axis.CreateDictionary<int, Instance>();
            Constants = Axis.CreateDictionary<int, Instance>();
        }
        protected Module(Scope scope, string name) : base(scope.Axis) {
            Scope = scope;
            Name = name;
            SuperClass = Class;

            // Create dictionaries
            ClassMethods = Axis.CreateDictionary<int, Method>();
            ClassVariables = Axis.CreateDictionary<int, Instance>();
            Constants = Axis.CreateDictionary<int, Instance>();
        }

        public bool DerivesFrom(Module Other) {
            Module Current = this;
            while (true) {
                if (Current == Other) {
                    return true;
                }
                else if (Current.SuperClass is not null) {
                    Current = Current.SuperClass;
                }
                else {
                    return false;
                }
            }
        }

        internal Method SetClassMethod(int MethodNameKey, Method Method) {
            ClassMethods[MethodNameKey] = Method;
            return Method;
        }
        public Method SetClassMethod(string MethodName, Method Method) {
            return SetClassMethod(Axis.Globals.GetNameKey(MethodName), Method);
        }
        public Method SetClassMethod(string MethodName, Delegate Delegate, AccessModifier AccessModifier = AccessModifier.Public) {
            return SetClassMethod(MethodName, new Method(Scope.Location, MethodName, Delegate, AccessModifier));
        }
        internal Method? RemoveClassMethod(int MethodNameKey) {
            ClassMethods.Remove(MethodNameKey, out Method? Method);
            return Method;
        }
        public Method? RemoveClassMethod(string MethodName) {
            return RemoveClassMethod(Axis.Globals.GetNameKey(MethodName));
        }
        internal Method? GetClassMethod(int MethodNameKey) {
            return GetMember(Class => Class.ClassMethods, MethodNameKey);
        }
        public Method? GetClassMethod(string MethodName) {
            return GetClassMethod(Axis.Globals.GetNameKey(MethodName));
        }
        public List<string> GetClassMethodNames() {
            return GetMemberNames(Class => Class.ClassMethods);
        }

        internal Instance SetConstant(int ConstantNameKey, Instance Value) {
            return Constants[ConstantNameKey] = Value;
        }
        public Instance SetConstant(string ConstantName, Instance Value) {
            return SetConstant(Axis.Globals.GetNameKey(ConstantName), Value);
        }
        public Instance SetConstant(string ConstantName, object? Value) {
            return SetConstant(ConstantName, Adapter.GetInstance(new Context(new CodeLocation(Axis), Scope, this, this), Value));
        }
        internal Instance? RemoveConstant(int ConstantNameKey) {
            Constants.Remove(ConstantNameKey, out Instance? Instance);
            return Instance;
        }
        public Instance? RemoveConstant(string ConstantName) {
            return RemoveConstant(Axis.Globals.GetNameKey(ConstantName));
        }
        internal Instance? GetConstant(int ConstantNameKey) {
            return GetMember(Class => Class.Constants, ConstantNameKey);
        }
        public Instance? GetConstant(string ConstantName) {
            return GetConstant(Axis.Globals.GetNameKey(ConstantName));
        }
        public List<string> GetConstantNames() {
            return GetMemberNames(Class => Class.Constants);
        }

        internal Instance SetClassVariable(int VariableNameKey, Instance Value) {
            return ClassVariables[VariableNameKey] = Value;
        }
        public Instance SetClassVariable(string VariableName, Instance Value) {
            return SetConstant(Axis.Globals.GetNameKey(VariableName), Value);
        }
        public Instance SetClassVariable(string VariableName, object? Value) {
            return SetClassVariable(VariableName, Adapter.GetInstance(new Context(new CodeLocation(Axis), Scope, this, this), Value));
        }
        internal Instance? GetClassVariable(int VariableNameKey) {
            return GetMember(Class => Class.ClassVariables, VariableNameKey);
        }
        public Instance? GetClassVariable(string VariableName) {
            return GetClassVariable(Axis.Globals.GetNameKey(VariableName));
        }
        public List<string> GetClassVariableNames() {
            return GetMemberNames(Class => Class.ClassVariables);
        }

        protected T? GetMember<T>(Func<Module, IDictionary<int, T>> Members, int MemberNameKey) where T : class {
            // Search for member in hierarchy
            Module? Current = this;
            while (Current is not null) {
                // Search for member in module
                if (Members(Current).TryGetValue(MemberNameKey, out T? Member)) {
                    return Member;
                }
                Current = Current.SuperClass;
            }
            // Member not found
            return null;
        }
        protected List<string> GetMemberNames<T>(Func<Module, IDictionary<int, T>> Members) {
            List<string> MemberNames = new();
            // Get member names in hierarchy
            Module? Current = this;
            while (Current is not null) {
                // Get member names in module
                MemberNames.AddRange(Members(Current).Keys.Select(Axis.Globals.GetName));
                Current = Current.SuperClass;
            }
            // Return member names
            return MemberNames;
        }
        public override string ToString()
            => Name;
    }
    public class Class : Module {
        protected readonly IDictionary<int, Method> InstanceMethods;
        protected readonly IDictionary<int, Instance> InstanceVariables;

        public Class(Module parent, Class? super, string name) : base(parent, name) {
            Class = Axis.Class;
            SuperClass = super ?? Class;

            // Create dictionaries
            InstanceMethods = Axis.CreateDictionary<int, Method>();
            InstanceVariables = Axis.CreateDictionary<int, Instance>();
        }
        internal Class(Scope scope, Class? super, string name) : base(scope, name) {
            Class = Axis.Class;
            SuperClass = super ?? Class;

            // Create dictionaries
            InstanceMethods = Axis.CreateDictionary<int, Method>();
            InstanceVariables = Axis.CreateDictionary<int, Instance>();
        }

        internal Method SetInstanceMethod(int MethodNameKey, Method Method) {
            return InstanceMethods[MethodNameKey] = Method;
        }
        public Method SetInstanceMethod(string MethodName, Method Method) {
            return SetInstanceMethod(Axis.Globals.GetNameKey(MethodName), Method);
        }
        public Method SetInstanceMethod(string MethodName, Delegate Delegate, AccessModifier AccessModifier = AccessModifier.Public) {
            return SetInstanceMethod(MethodName, new Method(Scope.Location, MethodName, Delegate, AccessModifier));
        }
        internal Method? RemoveInstanceMethod(int MethodNameKey) {
            InstanceMethods.Remove(MethodNameKey, out Method? Method);
            return Method;
        }
        public Method? RemoveInstanceMethod(string MethodName) {
            return RemoveInstanceMethod(Axis.Globals.GetNameKey(MethodName));
        }
        internal Method? GetInstanceMethod(int MethodNameKey) {
            return GetMember(Class => ((Class)Class).InstanceMethods, MethodNameKey);
        }
        public Method? GetInstanceMethod(string MethodName) {
            return GetInstanceMethod(Axis.Globals.GetNameKey(MethodName));
        }
        public List<string> GetInstanceMethodNames() {
            return GetMemberNames(Class => ((Class)Class).InstanceMethods);
        }

        internal Instance SetInstanceVariable(int VariableNameKey, Instance Value) {
            return InstanceVariables[VariableNameKey] = Value;
        }
        public Instance SetInstanceVariable(string VariableName, Instance Value) {
            return SetInstanceVariable(Axis.Globals.GetNameKey(VariableName), Value);
        }
        public Instance SetInstanceVariable(string VariableName, object? Value) {
            return SetInstanceVariable(VariableName, Adapter.GetInstance(new Context(new CodeLocation(Axis), Scope, this, this), Value));
        }
        internal Instance? GetInstanceVariable(int VariableNameKey) {
            return GetMember(Class => ((Class)Class).InstanceVariables, VariableNameKey);
        }
        public Instance? GetInstanceVariable(string VariableName) {
            return GetInstanceVariable(Axis.Globals.GetNameKey(VariableName));
        }
        public List<string> GetInstanceVariableNames() {
            return GetMemberNames(Class => ((Class)Class).InstanceVariables);
        }
    }
}
