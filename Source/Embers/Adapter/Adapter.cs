using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using PeterO.Numbers;

namespace Embers {
    public static class Adapter {
        internal const BindingFlags SearchFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;

        private static readonly ConditionalWeakTable<Type, Module> AdaptedModules = new();

        /// <summary>Get an instance from a .NET object.</summary>
        public static Instance GetInstance(Context Context, object? DotNetObject) {
            Axis Axis = Context.Axis;

            Instance DelegateToInstance(Delegate Delegate) {
                Method Method = new(Context.Location, Delegate.Method.Name.ToSnakeCase(), Delegate, Context.Locals.AccessModifier);
                return new Instance(Axis.Proc, new Proc(Context.Scope, Context.Instance, Method));
            }
            Instance TypeToInstance(Type Type) {
                return GetModule(Context.Module, Type);
            }
            Instance ObjectToInstance(object Object) {
                Type ObjectType = Object.GetType();
                Class Class = (Class)GetModule(Context.Module, ObjectType);
                return new Instance(Class, Object);
            }
            Instance RangeToInstance(Integer? Min, Integer? Max) {
                return new Instance(Axis.Range, new InstanceRange(Context.Location, new Instance(Axis.Range, Min), new Instance(Axis.Range, Max), exclude_end: true));
            }
            Instance ExceptionToInstance(Exception Exception) {
                Type ExceptionType = Exception.GetType();
                if (ExceptionType == typeof(Exception)) {
                    return new Instance(Axis.Exception, Exception);
                }
                else {
                    Class ExceptionClass = (Class)GetModule(Context.Module, ExceptionType);
                    ExceptionClass.SuperClass = Axis.Exception;
                    return new Instance(ExceptionClass, Exception);
                }
            }

            return DotNetObject switch {
                // Instance (no conversion)
                Instance Instance => Instance,
                // Class
                Type Type => TypeToInstance(Type),
                // Nil, True, False
                null => Axis.Nil,
                true => Axis.True,
                false => Axis.False,
                // String
                string => new Instance(Axis.String, DotNetObject),
                char => new Instance(Axis.String, DotNetObject.ToString()),
                // Integer
                Integer Integer => new Instance(Axis.Integer, Integer),
                int Int => new Instance(Axis.Integer, (Integer)Int),
                uint UInt => new Instance(Axis.Integer, (Integer)UInt),
                long Long => new Instance(Axis.Integer, (Integer)Long),
                ulong ULong => new Instance(Axis.Integer, (Integer)ULong),
                short Short => new Instance(Axis.Integer, (Integer)Short),
                ushort UShort => new Instance(Axis.Integer, (Integer)UShort),
                byte Byte => new Instance(Axis.Integer, (Integer)Byte),
                sbyte SByte => new Instance(Axis.Integer, (Integer)SByte),
                BigInteger BigInteger => new Instance(Axis.Integer, (Integer)BigInteger),
                EInteger EInteger => new Instance(Axis.Integer, (Integer)EInteger),
#if NET7_0_OR_GREATER
                Int128 Int128 => new Instance(Axis.Integer, (Integer)Int128),
                UInt128 UInt128 => new Instance(Axis.Integer, (Integer)UInt128),
#endif
                // Float
                Float Float => new Instance(Axis.Float, Float),
                float Float => new Instance(Axis.Float, (Float)Float),
                double Double => new Instance(Axis.Float, (Float)Double),
                decimal Decimal => new Instance(Axis.Float, (Float)Decimal),
                EDecimal EDecimal => new Instance(Axis.Float, (Float)EDecimal),
                EFloat EFloat => new Instance(Axis.Float, (Float)EFloat),
                ERational ERational => new Instance(Axis.Float, (Float)ERational),
#if NET7_0_OR_GREATER
                Half Half => new Instance(Axis.Float, (Float)Half),
#endif
                // Proc
                Proc Proc => new Instance(Axis.Proc, Proc),
                Method Method => new Instance(Axis.Proc, new Proc(Context.Scope, Context.Instance, Method)),
                Delegate Delegate => DelegateToInstance(Delegate),
                // Array
                Array Array => new Instance(Context.Axis.Array, Array),
                IList<Instance> List => new Instance(Context.Axis.Array, new Array(Context.Location, List)),
                // Hash
                Hash Hash => new Instance(Context.Axis.Hash, Hash),
                IDictionary<Instance, Instance> Dictionary => new Instance(Context.Axis.Array, new Hash(Context.Location, Dictionary)),
                // Time
                DateTimeOffset Time => new Instance(Axis.Time, Time),
                DateTime Time => new Instance(Axis.Time, new DateTimeOffset(Time)),
                // Range
                InstanceRange Range => new Instance(Axis.Range, Range),
                IntRange Range => RangeToInstance(Range.Min, Range.Max),
                Range Range => RangeToInstance(Range.Start.Value * (Range.Start.IsFromEnd ? -1 : 1), Range.End.Value * (Range.End.IsFromEnd ? -1 : 1)),
                // Enumerator
                Enumerator Enumerator => new Instance(Axis.Enumerator, Enumerator),
                IEnumerable<Instance> IEnumerable => new Instance(Axis.Enumerator, new Enumerator(IEnumerable)),
                IEnumerator<Instance> IEnumerator => new Instance(Axis.Enumerator, new Enumerator(IEnumerator)),
                IEnumerable IEnumerable => new Instance(Axis.Enumerator, new Enumerator(Context, IEnumerable)),
                IEnumerator IEnumerator => new Instance(Axis.Enumerator, new Enumerator(Context, IEnumerator)),
                // Exception
                Exception Exception => ExceptionToInstance(Exception),
                // WeakRef
                WeakReference<Instance> WeakRef => new Instance(Axis.WeakRef, WeakRef),
                // Thread
                Thread Thread => new Instance(Axis.Thread, Thread),
                // Object
                _ => ObjectToInstance(DotNetObject)
            };
        }
        /// <summary>Get the value of an instance, converting to the target type where possible.</summary>
        public static object? GetObject(Instance? Instance, Type TargetType) {
            object? GetPrimitive() {
                return Type.GetTypeCode(TargetType) switch {
                    // Integer
                    TypeCode.Int32 => (int)Instance.CastInteger,
                    TypeCode.UInt32 => (uint)Instance.CastInteger,
                    TypeCode.Int64 => (long)Instance.CastInteger,
                    TypeCode.UInt64 => (ulong)Instance.CastInteger,
                    TypeCode.Int16 => (short)Instance.CastInteger,
                    TypeCode.UInt16 => (ushort)Instance.CastInteger,
                    TypeCode.Byte => (byte)Instance.CastInteger,
                    TypeCode.SByte => (sbyte)Instance.CastInteger,
                    // Float
                    TypeCode.Single => (float)Instance.CastFloat,
                    TypeCode.Double => (double)Instance.CastFloat,
                    TypeCode.Decimal => (decimal)Instance.CastFloat,
                    // Other
                    _ => null
                };
            }
            object? GetOther() {
                // Instance
                if (TargetType == typeof(Instance) || TargetType.IsSubclassOf(typeof(Instance))) {
                    return Instance;
                }
                // Null
                if (Instance.Value is null) {
                    return null;
                }
                // Nullable
                if (Nullable.GetUnderlyingType(TargetType) is Type UnderlyingType) {
                    return GetObject(Instance, UnderlyingType);
                }
                // Array
                if (TargetType.IsArray && TargetType.GetElementType() is Type ArrayElementType) {
                    // Convert array to .NET array
                    if (Instance.Value is Array) {
                        return GetObjectArray(Instance.CastArray.Inner, ArrayElementType);
                    }
                    // Leniently convert object to .NET array of length 1
                    else {
                        return GetObjectArray(new Instance[] { Instance }, ArrayElementType);
                    }
                }
                // Integer
                if (TargetType == typeof(Integer)) {
                    return Instance.CastInteger;
                }
                // Float
                if (TargetType == typeof(Float)) {
                    return Instance.CastFloat;
                }
                // Other
                return Instance.Value;
            }
            if (Instance is null) return null;
            return GetPrimitive() ?? GetOther();
        }
        /// <summary>Get a module from a .NET type. The module will be a class if the type has a constructor.</summary>
        public static Module GetModule(Module Parent, Type Type) {
            // Type already adapted
            if (AdaptedModules.TryGetValue(Type, out Module? AdaptedModule)) {
                return AdaptedModule;
            }

            // Unbound generic type
            if (Type.IsGenericTypeDefinition) {
                // Warn that they're not supported (e.g. List<>)
                Parent.Axis.Warn(new CodeLocation(Parent.Axis), $"unbound generic types should not be used ({Type.Name[0..Type.Name.IndexOf('`')]}<>)");
            }

            // Create adapted module
            Module Module = Type.HasConstructor()
                ? new Class(Parent, null, Type.Name.ToPascalCase())
                : new Module(Parent, Type.Name.ToPascalCase());
            
            // Adapt unbound methods
            void AdaptUnboundMethod(MethodInfo MethodInfo, string MethodName, bool Static) {
                // Get parameter types
                Type[] ParameterTypes = MethodInfo.GetParameters().Select(Param => Param.ParameterType).ToArray();

                // Try to adapt the method
                try {
                    // Class method
                    if (Static) {
                        // Create delegate without target
                        Delegate Delegate = MethodBinder.CreateDelegate(null, MethodInfo);
                        // Add class method
                        Module.SetClassMethod(MethodName, Delegate);
                    }
                    // Instance method
                    else if (Module is Class ModuleAsClass) {
                        // Bind method to instance at runtime
                        // Note: This creates a new method to bind the instance every time the method is called. I can't think of a better way without caching.
                        object? CallInstanceMethod(Context Context, params Instance[] Arguments) {
                            // Create delegate bound to instance
                            Delegate BoundDelegate = MethodBinder.CreateDelegate(Context.Instance.Value, MethodInfo);
                            // Create method from bound delegate
                            Method BoundMethod = new(Context.Location, MethodInfo.Name, BoundDelegate, Context.Locals.AccessModifier);
                            // Call bound method
                            return BoundMethod.Call(Context, Arguments);
                        }
                        // Add instance method
                        ModuleAsClass.SetInstanceMethod(MethodName, CallInstanceMethod);
                    }
                }
                // Cannot adapt (probably has ref or pointer arguments)
                catch (Exception) {
                    // Pass
                }
            }
            // Adapt bound methods
            void AdaptBoundMethod(Delegate Delegate, string MethodName, bool Static) {
                // Class method
                if (Static) {
                    Module.SetClassMethod(MethodName, Delegate);
                }
                // Instance method
                else if (Module is Class Class) {
                    Class.SetInstanceMethod(MethodName, Delegate);
                }
            }

            // Copy methods
            foreach (IGrouping<string, MethodInfo> OverloadInfos in Type.GetMethods(SearchFlags).GroupBy(Overload => Overload.Name)) {
                // Get overloads
                MethodInfo[] AllOverloads = OverloadInfos.ToArray();
                MethodInfo[] StaticOverloads = AllOverloads.Where(Overload => Overload.IsStatic).ToArray();
                MethodInfo[] InstanceOverloads = AllOverloads.Where(Overload => !Overload.IsStatic).ToArray();

                // Adapt overloads
                void AdaptOverloads(MethodInfo[] Overloads, bool Static) {
                    // Get first overload
                    if (Overloads.Length == 0) return;
                    MethodInfo Overload = Overloads.First();

                    // Single overload
                    if (Overloads.Length == 1) {
                        // Adapt method directly
                        AdaptUnboundMethod(Overload, Overload.Name.ToSnakeCase(), Overload.IsStatic);
                    }
                    // Multiple overloads
                    else {
                        // Late overload selector
                        object? Invoke(Context Context, [Splat] Instance[] Arguments) {
                            // Invoke best overload
                            return MethodBinder.InvokeBestOverload(Overloads, Context.Instance.Value, Arguments);
                        }
                        // Adapt overload selector
                        AdaptBoundMethod(Invoke, Overload.Name.ToSnakeCase(), Overload.IsStatic);
                    }
                }
                AdaptOverloads(StaticOverloads, true);
                AdaptOverloads(InstanceOverloads, false);
            }

            // Copy properties
            foreach (PropertyInfo PropertyInfo in Type.GetProperties(SearchFlags)) {
                // Try get getter and setter
                MethodInfo? Get = PropertyInfo.GetMethod;
                MethodInfo? Set = PropertyInfo.SetMethod;

                // Try get index parameters
                ParameterInfo[] IndexParameters = PropertyInfo.GetIndexParameters();

                // Indexer
                if (IndexParameters.Length != 0) {
                    // Get
                    if (Get is not null) {
                        AdaptUnboundMethod(Get, "[]", Get.IsStatic);
                    }
                    // Set
                    if (Set is not null) {
                        AdaptUnboundMethod(Set, "[]=", Set.IsStatic);
                    }
                }
                // Property
                else {
                    // Get
                    if (Get is not null) {
                        AdaptUnboundMethod(Get, PropertyInfo.Name.ToSnakeCase(), Get.IsStatic);
                    }
                    // Set
                    if (Set is not null) {
                        AdaptUnboundMethod(Set, PropertyInfo.Name.ToSnakeCase() + "=", Set.IsStatic);
                    }
                }
            }

            // Copy fields
            foreach (FieldInfo FieldInfo in Type.GetFields(SearchFlags)) {
                // Get field name
                string FieldName = FieldInfo.Name.ToSnakeCase();
                // Mimic property get
                Delegate Getter = (Context Context) => {
                    return FieldInfo.GetValue(Context.Instance.Value);
                };
                AdaptBoundMethod(Getter, FieldName, FieldInfo.IsStatic);
                // Mimic property set
                Delegate Setter = (Context Context, Instance Value) => {
                    FieldInfo.SetValue(Context.Instance.Value, GetObject(Value, FieldInfo.FieldType));
                };
                AdaptBoundMethod(Setter, FieldName + "=", FieldInfo.IsStatic);
            }

            // Class
            if (Module is Class Class) {
                // Copy constructors
                ConstructorInfo[] Constructors = Type.GetConstructors(SearchFlags);
                if (Constructors.Length != 0) {
                    // new
                    Instance New([Splat] Instance[] Arguments) {
                        // Create object
                        object NewObject = FormatterServices.GetUninitializedObject(Type);
                        // Invoke best constructor overload
                        MethodBinder.InvokeBestOverload(Constructors, NewObject, Arguments);
                        // Return instance of class
                        return new Instance(Class, NewObject);
                    }
                    Class.SetClassMethod("new", New);
                }

                // Copy System.Array indexers
                if (Type.IsArray) {
                    if (Class.GetInstanceMethod("get_value") is Method Getter) {
                        Class.SetInstanceMethod("[]", Getter);
                    }
                    if (Class.GetInstanceMethod("set_value") is Method Setter) {
                        Class.SetInstanceMethod("[]=", Setter);
                    }
                }
            }

            // All done!
            return Module;
        }
        /// <summary>Get an array from a collection of objects.</summary>
        public static Array GetArray(Context Context, ICollection Collection) {
            Array Array = new(Context.Location, Collection.Count);
            foreach (object? Item in Collection) {
                Array.Add(GetInstance(Context, Item));
            }
            return Array;
        }
        /// <summary>Get a System.Array of the target type from a list of instances, converting each item to the target type where possible.</summary>
        public static System.Array GetObjectArray(IList<Instance> InstanceList, Type TargetItemType) {
            // Create array
            System.Array ConvertedArray = System.Array.CreateInstance(TargetItemType, InstanceList.Count);
            // Add converted arguments
            for (int i = 0; i < InstanceList.Count; i++) {
                object? Item = GetObject(InstanceList[i], TargetItemType);
                ConvertedArray.SetValue(Item, i);
            }
            return ConvertedArray;
        }
        /// <summary>Get an object?[] from a list of instances, converting each item to the corresponding target type where possible.</summary>
        public static object?[] GetObjectArray(IList<Instance> InstanceList, Type[] TargetItemTypes) {
            // Create array
            object?[] ConvertedArray = new object?[InstanceList.Count];
            // Add converted arguments
            for (int i = 0; i < InstanceList.Count; i++) {
                object? Item = GetObject(InstanceList[i], TargetItemTypes[i]);
                ConvertedArray.SetValue(Item, i);
            }
            return ConvertedArray;
        }
    }
}
