using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Embers {
    public sealed class Axis {
        public readonly AxisOptions Options;
        public readonly Globals Globals;

        public readonly Class Object;
        public readonly Class Module;
        public readonly Class Class;
        public readonly Class ControlCode;
        public readonly Class NilClass;
        public readonly Class TrueClass;
        public readonly Class FalseClass;
        public readonly Class String;
        public readonly Class Symbol;
        public readonly Class Integer;
        public readonly Class Float;
        public readonly Class Proc;
        public readonly Class Array;
        public readonly Class Hash;
        public readonly Class Time;
        public readonly Class Range;
        public readonly Class Enumerator;
        public readonly Class Exception;
        public readonly Class WeakRef;
        public readonly Class Thread;
        public readonly Module Math;
        public readonly Module GC;
        public readonly Module File;

        public readonly Instance Main;
        public readonly Instance Nil;
        public readonly Instance True;
        public readonly Instance False;

        internal Axis(AxisOptions? axis_options = null) {
            Options = axis_options ?? new AxisOptions();
            Globals = new Globals(this);

            Scope RootScope = new(this);
            
            Object = new Class(RootScope, null, "Object");
            Module = new Class(RootScope, Object, "Module");
            Class = new Class(RootScope, Module, "Class");

            Object.Class = Class; Object.SuperClass = null;
            Module.Class = Class; Module.SuperClass = Object;
            Class.Class = Class; Class.SuperClass = Module;

            ControlCode = new Class(Object, null, "ControlCode");
            NilClass = new Class(Object, null, "NilClass");
            TrueClass = new Class(Object, null, "TrueClass");
            FalseClass = new Class(Object, null, "FalseClass");
            String = new Class(Object, null, "String");
            Symbol = new Class(Object, null, "Symbol");
            Integer = new Class(Object, null, "Integer");
            Float = new Class(Object, null, "Float");
            Proc = new Class(Object, null, "Proc");
            Array = new Class(Object, null, "Array");
            Hash = new Class(Object, null, "Hash");
            Time = new Class(Object, null, "Time");
            Range = new Class(Object, null, "Range");
            Enumerator = new Class(Object, null, "Enumerator");
            Exception = new Class(Object, null, "Exception");
            WeakRef = new Class(Object, null, "WeakRef");
            Thread = new Class(Object, null, "Thread");
            Math = new Module(Object, "Math");
            GC = new Module(Object, "GC");
            File = new Module(Object, "File");

            Main = new Instance(Object, "main");
            Nil = new Instance(NilClass, null);
            True = new Instance(TrueClass, true);
            False = new Instance(FalseClass, false);

            StandardLibrary.Setup(this);
        }

        public void Warn(CodeLocation Location, string Message) {
            Main.CallMethod("warn", new Instance(String, $"{Location}: warning: {Message}"));
        }

        internal IDictionary<TKey, TValue> CreateDictionary<TKey, TValue>(int Capacity = 0) where TKey : notnull {
            return Options.ThreadSafety
                ? new ConcurrentDictionary<TKey, TValue>(Environment.ProcessorCount, Capacity)
                : new Dictionary<TKey, TValue>(Capacity);
        }
        internal IDictionary<Instance, Instance> CreateInstanceDictionary(int Capacity = 0) {
            return Options.ThreadSafety
                ? new ConcurrentDictionary<Instance, Instance>(Environment.ProcessorCount, Capacity, new Hash.InstanceEqualityComparer(this))
                : new Dictionary<Instance, Instance>(Capacity, new Hash.InstanceEqualityComparer(this));
        }
        internal IList<T> CreateList<T>(int Capacity = 0) {
            return Options.ThreadSafety
                ? new SynchronizedCollection<T>(Capacity)
                : new List<T>(Capacity);
        }
        internal IList<Instance> CreateInstanceList(int Capacity = 0) {
            return CreateList<Instance>(Capacity);
        }
    }
}
