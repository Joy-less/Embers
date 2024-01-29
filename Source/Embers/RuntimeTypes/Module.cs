using System;
using System.Collections.Generic;

namespace Embers {
    public class Module : Instance {
        public readonly Scope Scope;
        public readonly string Name;
        public Class? SuperClass;

        protected readonly IDictionary<string, Method> ClassMethods;
        protected readonly IDictionary<string, Instance> ClassVariables;
        protected readonly IDictionary<string, Instance> Constants;

        public Module(Module parent, string name) : base(parent.Axis.Module) {
            Scope = parent.Scope;
            Name = name;
            SuperClass = Class;

            // Create dictionaries
            ClassMethods = Axis.CreateDictionary<string, Method>();
            ClassVariables = Axis.CreateDictionary<string, Instance>();
            Constants = Axis.CreateDictionary<string, Instance>();
        }
        protected Module(Scope scope, string name) : base(scope.Axis) {
            Scope = scope;
            Name = name;
            SuperClass = Class;

            // Create dictionaries
            ClassMethods = Axis.CreateDictionary<string, Method>();
            ClassVariables = Axis.CreateDictionary<string, Instance>();
            Constants = Axis.CreateDictionary<string, Instance>();
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

        public Method SetClassMethod(string MethodName, Method Method) {
            ClassMethods[MethodName] = Method;
            return Method;
        }
        public Method SetClassMethod(string MethodName, Delegate Delegate, AccessModifier AccessModifier = AccessModifier.Public) {
            return SetClassMethod(MethodName, new Method(Scope.Location, MethodName, Delegate, AccessModifier));
        }
        public Method? RemoveClassMethod(string MethodName) {
            ClassMethods.Remove(MethodName, out Method? Method);
            return Method;
        }
        public Method? GetClassMethod(string MethodName) {
            return GetMember(Class => Class.ClassMethods, MethodName);
        }
        public List<string> GetClassMethodNames() {
            return GetMemberNames(Class => Class.ClassMethods);
        }

        public Instance SetConstant(string ConstantName, Instance Value) {
            return Constants[ConstantName] = Value;
        }
        public Instance SetConstant(string ConstantName, object? Value) {
            return SetConstant(ConstantName, Adapter.GetInstance(new Context(new CodeLocation(Axis), Scope, this, this), Value));
        }
        public Instance? RemoveConstant(string MethodName) {
            Constants.Remove(MethodName, out Instance? Instance);
            return Instance;
        }
        public Instance? GetConstant(string ConstantName) {
            return GetMember(Class => Class.Constants, ConstantName);
        }
        public List<string> GetConstantNames() {
            return GetMemberNames(Class => Class.Constants);
        }

        public Instance SetClassVariable(string ClassVariableName, Instance Value) {
            return ClassVariables[ClassVariableName] = Value;
        }
        public Instance SetClassVariable(string ClassVariableName, object? Value) {
            return SetClassVariable(ClassVariableName, Adapter.GetInstance(new Context(new CodeLocation(Axis), Scope, this, this), Value));
        }
        public Instance? GetClassVariable(string ClassVariableName) {
            return GetMember(Class => Class.ClassVariables, ClassVariableName);
        }
        public List<string> GetClassVariableNames() {
            return GetMemberNames(Class => Class.ClassVariables);
        }

        protected T? GetMember<T>(Func<Module, IDictionary<string, T>> Members, string MemberName) where T : class {
            // Search for member in hierarchy
            Module Current = this;
            while (true) {
                // Search current
                if (Members(Current).TryGetValue(MemberName, out T? Member)) {
                    return Member;
                }
                // Search superclass
                else if (Current.SuperClass is not null) {
                    Current = Current.SuperClass;
                }
                // Member not found
                else {
                    return null;
                }
            }
        }
        protected List<string> GetMemberNames<T>(Func<Module, IDictionary<string, T>> Members) {
            // Search for member names in hierarchy
            List<string> MemberNames = new();
            Module Current = this;
            while (true) {
                // Search current
                MemberNames.AddRange(Members(Current).Keys);
                // Search superclass
                if (Current.SuperClass is not null) {
                    Current = Current.SuperClass;
                }
                // Return all member names
                else {
                    return MemberNames;
                }
            }
        }
        public override string ToString()
            => Name;
    }
    public class Class : Module {
        protected readonly IDictionary<string, Method> InstanceMethods;
        protected readonly IDictionary<string, Instance> InstanceVariables;

        public Class(Module parent, Class? super, string name) : base(parent, name) {
            Class = Axis.Class;
            SuperClass = super ?? Class;

            // Create dictionaries
            InstanceMethods = Axis.CreateDictionary<string, Method>();
            InstanceVariables = Axis.CreateDictionary<string, Instance>();
        }
        internal Class(Scope scope, Class? super, string name) : base(scope, name) {
            Class = Axis.Class;
            SuperClass = super ?? Class;

            // Create dictionaries
            InstanceMethods = Axis.CreateDictionary<string, Method>();
            InstanceVariables = Axis.CreateDictionary<string, Instance>();
        }
        
        public Method SetInstanceMethod(string MethodName, Method Method) {
            return InstanceMethods[MethodName] = Method;
        }
        public Method SetInstanceMethod(string MethodName, Delegate Delegate, AccessModifier AccessModifier = AccessModifier.Public) {
            return SetInstanceMethod(MethodName, new Method(Scope.Location, MethodName, Delegate, AccessModifier));
        }
        public Method? RemoveInstanceMethod(string MethodName) {
            InstanceMethods.Remove(MethodName, out Method? Method);
            return Method;
        }
        public Method? GetInstanceMethod(string MethodName) {
            return GetMember(Class => ((Class)Class).InstanceMethods, MethodName);
        }
        public List<string> GetInstanceMethodNames() {
            return GetMemberNames(Class => ((Class)Class).InstanceMethods);
        }

        public Instance SetInstanceVariable(string InstanceVariableName, Instance Value) {
            // Set instance variable
            if (Value != Axis.Nil) {
                return InstanceVariables[InstanceVariableName] = Value;
            }
            // Remove instance variable
            else {
                InstanceVariables.Remove(InstanceVariableName);
                return Axis.Nil;
            }
        }
        public Instance SetInstanceVariable(string InstanceVariableName, object? Value) {
            return SetInstanceVariable(InstanceVariableName, Adapter.GetInstance(new Context(new CodeLocation(Axis), Scope, this, this), Value));
        }
        public Instance? GetInstanceVariable(string InstanceVariableName) {
            return GetMember(Class => ((Class)Class).InstanceVariables, InstanceVariableName);
        }
        public List<string> GetInstanceVariableNames() {
            return GetMemberNames(Class => ((Class)Class).InstanceVariables);
        }
    }
}
