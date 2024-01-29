namespace Embers {
    public sealed class Context {
        public readonly ContextLocals Locals;
        public readonly CodeLocation Location;
        public readonly Scope Scope;
        public readonly Module Module;
        public readonly Instance Instance;
        public readonly Proc? Block;
        public readonly Method? Method;
        public Context(CodeLocation location, Scope scope, Module? module = null, Instance? instance = null, Proc? block = null, Method? method = null) {
            Locals = new ContextLocals();
            Location = location;
            Scope = scope;
            Module = module ?? location.Axis.Object;
            Instance = instance ?? location.Axis.Main;
            Block = block;
            Method = method;
        }
        public Context(ContextLocals locals, CodeLocation location, Scope scope, Module? module = null, Instance? instance = null, Proc? block = null, Method? method = null) {
            Locals = locals;
            Location = location;
            Scope = scope;
            Module = module ?? location.Axis.Object;
            Instance = instance ?? location.Axis.Main;
            Block = block;
            Method = method;
        }
        public Axis Axis => Scope.Axis;
    }
    public sealed class ContextLocals {
        public AccessModifier AccessModifier = AccessModifier.Public;
        public Thread? Thread = null;
    }
}
