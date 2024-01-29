using System;
using System.Linq;

namespace Embers {
    public class Instance {
        public readonly Axis Axis;
        public readonly long ObjectId;
        public Class Class;
        public object? Value;
        public bool Truthy => Value is not (null or false);
        public bool Falsey => Value is null or false;

        public Instance(Class from_class, object? value) {
            Axis = from_class.Axis;
            ObjectId = Axis.Globals.NewObjectId();
            Class = from_class ?? Axis.Class;
            Value = value;
        }
        public Instance(Class from_class) {
            Axis = from_class.Axis;
            ObjectId = Axis.Globals.NewObjectId();
            Class = from_class ?? Axis.Class;
            Value = this;
        }
        internal Instance(Axis axis) {
            Axis = axis;
            ObjectId = Axis.Globals.NewObjectId();
            Class = Axis.Class;
            Value = this;
        }
        
        public Module CastModule => Cast<Module>();
        public Class CastClass => Cast<Class>();
        public bool CastBoolean => Cast<bool>();
        public string CastString => Cast<string>();
        public Integer CastInteger => Value is Float Float && Float.IsInteger() ? (Integer)Float : Cast<Integer>();
        public Float CastFloat => Value as Integer ?? Cast<Float>();
        public Proc CastProc => Cast<Proc>();
        public Array CastArray => Cast<Array>();
        public Hash CastHash => Cast<Hash>();
        public DateTimeOffset CastTime => Cast<DateTimeOffset>();
        public InstanceRange CastRange => Cast<InstanceRange>();
        public Enumerator CastEnumerator => Cast<Enumerator>();
        public Exception CastException => Cast<Exception>();
        public WeakReference<Instance> CastWeakRef => Cast<WeakReference<Instance>>();
        public Thread CastThread => Cast<Thread>();
        public T Cast<T>() => Value is T ValueAsT ? ValueAsT : throw new InteropError($"can't cast {Value ?? "nil"}:{Class} to {typeof(T)}");

        public Module SelfOrClass
            => (this as Module) ?? Class;

        public Method? GetMethod(string MethodName) {
            // Class method
            if (this is Module ThisModule) {
                return ThisModule.GetClassMethod(MethodName);
            }
            // Instance method
            else {
                return Class.GetInstanceMethod(MethodName);
            }
        }
        public Instance CallMethod(Context Context, string MethodName, params Instance[] Arguments) {
            // Get method
            Method? Method = GetMethod(MethodName);
            if (Method is null) {
                // Otherwise get method_missing
                Method = GetMethod("method_missing")
                    ?? throw new RuntimeError($"{Context.Location}: undefined method '{MethodName}' for {Describe()}");
                // Prepend method name to arguments for method_missing
                Arguments = Arguments.Prepend(new Instance(Context.Axis.String, MethodName)).ToArray();
            }
            // Verify access modifiers
            Method.VerifyAccess(Context.Location, Context.Instance, this);
            // Call method
            return Method.Call(new Context(Context.Locals, Context.Location, SelfOrClass.Scope, SelfOrClass, this, Context.Block), Arguments);
        }
        public Instance CallMethod(string MethodName, params Instance[] Arguments)
            => CallMethod(new Context(new CodeLocation(Axis), Class.Scope), MethodName, Arguments);
        public string Inspect()
            => CallMethod("inspect").CastString;
        public string ToS()
            => CallMethod("to_s").CastString;
        public int Hash()
            => (int)CallMethod("hash").CastInteger;
        public string Describe()
            => $"{Inspect()}:{Class}";
        public override string ToString()
            => (Value == this)
                ? $"#<{Class.Name}:0x{GetHashCode():x16}>"
                : $"{Value}";
    }
}
