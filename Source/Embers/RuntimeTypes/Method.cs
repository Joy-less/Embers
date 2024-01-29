using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Embers {
    public sealed class Method : RubyObject {
        public readonly string Name;
        public readonly AccessModifier AccessModifier;
        public readonly Argument[] Arguments;
        public readonly IntRange ArgumentsRange;
        public readonly MethodType MethodType;

        private readonly FastDelegate? CallDelegate;
        private readonly Expression[]? CallExpressions;
        private readonly Scope? CallExpressionsScope;
        private readonly bool TakesContext;
        private readonly bool TakesBlock;
        private readonly bool TakesDoubleSplat;

        /// <summary>Create method from .NET delegate.</summary>
        internal Method(CodeLocation location, string name, Delegate dot_net_delegate, AccessModifier access_modifier) : base(location) {
            Name = name;
            AccessModifier = access_modifier;
            MethodType = MethodType.Adapted;

            // Create call delegate
            CallDelegate = new FastDelegate(dot_net_delegate, Axis.Options.EnableCompilation);

            // Disallow generic arguments
            if (dot_net_delegate.Method.ContainsGenericParameters) {
                throw new InteropError("methods with generic arguments are disallowed");
            }

            // Get arguments
            ParameterInfo[] Parameters = dot_net_delegate.Method.GetParameters();
            // Takes context
            if (Parameters.FirstOrDefault()?.ParameterType == typeof(Context)) {
                TakesContext = true;
            }
            // Takes block
            if (Parameters.LastOrDefault()?.GetCustomAttribute<BlockAttribute>() is not null) {
                TakesBlock = true;
            }
            // Create arguments array
            int Offset = TakesContext ? 1 : 0;
            Arguments = new Argument[Parameters.Length - Offset];
            for (int i = Offset; i < Parameters.Length; i++) {
                ParameterInfo Parameter = Parameters[i];
                Arguments[i - Offset] = new Argument(location, Parameter, Parameter.IsParams(dot_net_delegate));
            }

            // Get arguments info
            GetArgumentsInfo(out ArgumentsRange, out TakesDoubleSplat, out TakesBlock);
        }
        /// <summary>Create method from Ruby expressions.</summary>
        internal Method(Scope scope, CodeLocation location, string name, Argument[] arguments, Expression[] expressions, AccessModifier access_modifier) : base(location) {
            Name = name;
            AccessModifier = access_modifier;
            Arguments = arguments;
            CallExpressions = expressions;
            CallExpressionsScope = scope;
            MethodType = MethodType.Def;

            // Get arguments info
            GetArgumentsInfo(out ArgumentsRange, out TakesDoubleSplat, out TakesBlock);
        }
        /// <summary>Create method from Ruby block.</summary>
        internal Method(CodeLocation location, Argument[] arguments, Expression[] expressions) : base(location) {
            Name = "block";
            Arguments = arguments;
            CallExpressions = expressions;
            MethodType = MethodType.Block;

            // Get arguments info
            GetArgumentsInfo(out ArgumentsRange, out TakesDoubleSplat, out TakesBlock);
        }
        public Instance Call(Context Context, params Instance[] GivenArguments) {
            // Warn if block implicitly passed
            if (!TakesBlock && Context.Block is not null) {
                Axis.Warn(Location, "block passed to method with no &block argument");
            }

            // Get pass arguments
            Instance[] PassArguments = GetPassArguments(Context, GivenArguments);

            // Call method
            Instance ReturnValue;
            // Call .NET delegate
            if (MethodType is MethodType.Adapted) {
                // Convert given arguments from Ruby to .NET
                int Offset = TakesContext ? 1 : 0;
                object?[] InvokeArguments = new object?[Arguments.Length + Offset];
                if (TakesContext) {
                    InvokeArguments[0] = Context;
                }
                for (int i = 0; i < Arguments.Length; i++) {
                    InvokeArguments[i + Offset] = Adapter.GetObject(PassArguments[i], Arguments[i].AdaptedParameter!.ParameterType);
                }
                // Invoke .NET delegate
                object? ObjectReturnValue = CallDelegate!.Invoke(InvokeArguments);
                // Convert the return value
                ReturnValue = Adapter.GetInstance(Context, ObjectReturnValue);
            }
            // Call Ruby method
            else {
                // Create call scope
                Scope CallScope = new(CallExpressionsScope ?? Context.Scope, Location);
                // Create call context
                Context CallContext = new(Context.Locals, Context.Location, CallScope, Context.Module, Context.Instance, Context.Block, this);
                // Assign local variables from arguments
                for (int i = 0; i < Arguments.Length; i++) {
                    CallScope.SetVariable(Arguments[i].Name, PassArguments[i]);
                }
                // Interpret method expressions
                ReturnValue = CallExpressions!.Interpret(CallContext);
            }

            // Control code
            if (ReturnValue is ControlCode ControlCode) {
                switch (ControlCode.Type) {
                    case ControlType.Return:
                        return ControlCode.Argument ?? Axis.Nil;
                }
            }
            return ReturnValue;
        }
        public Method Alias(string AliasName, AccessModifier? AliasAccessModifier) {
            return MethodType switch {
                MethodType.Adapted => new Method(Location, AliasName, CallDelegate!.Original, AliasAccessModifier ?? AccessModifier),
                MethodType.Def => new Method(CallExpressionsScope!, Location, AliasName, Arguments, CallExpressions!, AliasAccessModifier ?? AccessModifier),
                _ => throw new InternalError($"{Location}: cannot alias method of type '{MethodType}'")
            };
        }
        public void VerifyAccess(CodeLocation Location, Instance FromInstance, Instance MethodInstance) {
            // Get from and to
            Module From = FromInstance.SelfOrClass;
            Module To = MethodInstance.SelfOrClass;

            // Private
            if (AccessModifier is AccessModifier.Private) {
                if (From != To) {
                    throw new RuntimeError($"{Location}: tried to call private method '{Name}' of '{MethodInstance.Inspect()}'");
                }
            }
            // Protected
            else if (AccessModifier is AccessModifier.Protected) {
                if (!From.DerivesFrom(To)) {
                    throw new RuntimeError($"{Location}: tried to call protected method '{Name}' of '{MethodInstance.Inspect()}'");
                }
            }
        }
        public override string ToString()
            => Name;

        private void GetArgumentsInfo(out IntRange ArgumentsRange, out bool TakesDoubleSplat, out bool TakesBlock) {
            // Get splat arguments
            bool TakesSplat = false;
            TakesDoubleSplat = false;
            TakesBlock = false;
            for (int i = 0; i < Arguments.Length; i++) {
                Argument Argument = Arguments[i];

                // Splat
                if (Argument.ArgumentType is ArgumentType.Splat) {
                    if (TakesSplat) {
                        throw new SyntaxError($"{Argument.Location}: methods can have at most one splat (*) argument");
                    }
                    TakesSplat = true;
                }
                // Double splat
                else if (Argument.ArgumentType is ArgumentType.DoubleSplat) {
                    if (i + 1 < Arguments.Length && Arguments[i + 1].ArgumentType is not ArgumentType.Block) {
                        throw new SyntaxError($"{Argument.Location}: expected end of arguments or block (&) argument after double splat (**) argument");
                    }
                    TakesDoubleSplat = true;
                }
                // Block
                else if (Argument.ArgumentType is ArgumentType.Block) {
                    if (i + 1 < Arguments.Length) {
                        throw new SyntaxError($"{Argument.Location}: block (&) argument must be the last argument");
                    }
                    TakesBlock = true;
                }
            }

            // Get arguments range
            int MinArguments = Arguments.Count(Argument =>
                Argument.DefaultValue is null && (Argument.AdaptedParameter is null || !Argument.AdaptedParameter.HasDefaultValue) && Argument.ArgumentType is ArgumentType.Normal
            );
            int? MaxArguments = (TakesSplat || TakesDoubleSplat) ? null : Arguments.Length;
            if (TakesBlock) {
                MinArguments--;
                MaxArguments--;
            }
            ArgumentsRange = new IntRange(MinArguments, MaxArguments);
        }
        private Instance[] GetPassArguments(Context Context, Instance[] GivenArguments) {
            // Create array of arguments to pass
            Instance[] PassArguments = new Instance[Arguments.Length];

            // Validate arguments count
            if (MethodType is not MethodType.Block) {
                // Not enough arguments
                if (GivenArguments.Length < ArgumentsRange.Min) {
                    string AllowedRange = $"{(ArgumentsRange.Min == ArgumentsRange.Max ? ArgumentsRange.Min : ArgumentsRange)}";
                    throw new RuntimeError($"{Context.Location}: not enough arguments given for '{Name}' (expected {AllowedRange}, got {GivenArguments.Length})");
                }
                // Too many arguments
                if (GivenArguments.Length > ArgumentsRange.Max) {
                    string AllowedRange = $"{(ArgumentsRange.Min == ArgumentsRange.Max ? ArgumentsRange.Min : ArgumentsRange)}";
                    throw new RuntimeError($"{Context.Location}: too many arguments given for '{Name}' (expected {AllowedRange}, got {GivenArguments.Length})");
                }
            }

            // Input:
            //   method(arg1, arg2, *arg3, arg4, **arg5)
            //   call(1, 2, 3, 4, 5, 6, 7, :a => :b, :c => :d)
            // Output:
            //   arg1 = 1; arg2 = 2; arg3 = [3, 4, 5, 6]; arg4 = 7; arg5 = {:a => :b, :c => :d}

            int GiveIndex = 0;
            for (int TakeIndex = 0; TakeIndex < Arguments.Length; TakeIndex++) {
                Argument Argument = Arguments[TakeIndex];

                // Normal
                if (Argument.ArgumentType is ArgumentType.Normal) {
                    // Pass one argument
                    if (GiveIndex < GivenArguments.Length) {
                        PassArguments[TakeIndex] = GivenArguments[GiveIndex];
                        // Next
                        GiveIndex++;
                    }
                    // Pass default value
                    else if (Argument.DefaultValue is not null) {
                        PassArguments[TakeIndex] = Argument.DefaultValue.Interpret(Context);
                    }
                    else if (Argument.AdaptedParameter is not null && Argument.AdaptedParameter.HasDefaultValue) {
                        PassArguments[TakeIndex] = Adapter.GetInstance(Context, Argument.AdaptedParameter.DefaultValue);
                    }
                    // Pass nil (blocks only)
                    else {
                        PassArguments[TakeIndex] = Axis.Nil;
                    }
                }
                // Splat
                else if (Argument.ArgumentType is ArgumentType.Splat) {
                    // Get number of splat arguments
                    int LeftoverArguments = Arguments.Length - TakeIndex;
                    int LeftoverGivenArguments = GivenArguments.Length - GiveIndex;
                    int SplatCount = LeftoverGivenArguments - LeftoverArguments + 1;
                    if (TakesDoubleSplat) {
                        SplatCount--;
                    }
                    if (TakesBlock) {
                        SplatCount++;
                    }
                    // Create splat array
                    Array SplatArray = new(Context.Location, SplatCount);
                    // Fill splat array
                    for (int i = 0; i < SplatCount; i++) {
                        SplatArray.Add(GivenArguments[GiveIndex]);
                        // Next
                        GiveIndex++;
                    }
                    // Pass splat array
                    PassArguments[TakeIndex] = new Instance(Axis.Array, SplatArray);
                }
                // Double splat
                else if (Argument.ArgumentType is ArgumentType.DoubleSplat) {
                    // Create double splat hash
                    Hash DoubleSplatHash = new(Location);
                    // Pass hash arguments
                    while (GiveIndex < GivenArguments.Length) {
                        // Get current key-value pair or hash
                        Hash HashArgument = GivenArguments[GiveIndex].CastHash;
                        // Add each key-value pair to hash
                        foreach (KeyValuePair<Instance, Instance> KeyValue in HashArgument.Inner) {
                            DoubleSplatHash[KeyValue.Key] = KeyValue.Value;
                        }
                        // Next
                        GiveIndex++;
                    }
                    // Pass double splat hash
                    PassArguments[TakeIndex] = new Instance(Axis.Hash, DoubleSplatHash);
                }
                // Block
                else if (Argument.ArgumentType is ArgumentType.Block) {
                    // Pass block or nil if not given
                    PassArguments[TakeIndex] = Context.Block is not null ? new Instance(Axis.Proc, Context.Block) : Axis.Nil;
                }
                // Invalid
                else {
                    throw new InternalError($"{Argument.Location}: splat type not handled: '{Argument.ArgumentType}'");
                }
            }

            // Return pass arguments
            return PassArguments;
        }
    }
    public sealed class Argument : RubyObject {
        public readonly string Name;
        public readonly ArgumentType ArgumentType;
        public readonly Expression? DefaultValue;
        public readonly ParameterInfo? AdaptedParameter;

        internal Argument(CodeLocation location, string name, Expression? default_value = null, ArgumentType argument_type = ArgumentType.Normal) : base(location) {
            Name = name;
            DefaultValue = default_value;
            ArgumentType = argument_type;
        }
        internal Argument(CodeLocation location, ParameterInfo parameter, bool is_params) : base(location) {
            Name = parameter.Name?.ToSnakeCase() ?? string.Empty;
            AdaptedParameter = parameter;
            ArgumentType = GetArgumentType(parameter, is_params);
        }
        public override string ToString()
            => DefaultValue is not null ? $"{Name} = {DefaultValue}" : Name;

        private ArgumentType GetArgumentType(ParameterInfo Parameter, bool IsParams) {
            ArgumentType FindArgumentType = ArgumentType.Normal;
            // Splat
            if (Parameter.GetCustomAttribute<SplatAttribute>() is not null || IsParams) {
                if (!Parameter.ParameterType.IsAssignableTo(typeof(IList))) {
                    throw new InteropError($"{Location}: splat argument must be a list or array (got {Parameter.ParameterType})");
                }
                FindArgumentType = ArgumentType.Splat;
            }
            // Double splat
            if (Parameter.GetCustomAttribute<DoubleSplatAttribute>() is not null) {
                if (Parameter.ParameterType != typeof(Hash)) {
                    throw new InteropError($"{Location}: double splat argument must be a hash (got {Parameter.ParameterType})");
                }
                FindArgumentType = ArgumentType.DoubleSplat;
            }
            // Block
            if (Parameter.GetCustomAttribute<BlockAttribute>() is not null) {
                if (Parameter.ParameterType != typeof(Proc)) {
                    throw new InteropError($"{Location}: block argument must be a proc (got {Parameter.ParameterType})");
                }
                FindArgumentType = ArgumentType.Block;
            }
            return FindArgumentType;
        }
    }
    public sealed class Proc {
        public readonly Scope Scope;
        public readonly Instance Target;
        public readonly Method Method;
        public Proc(Scope scope, Instance target, Method method) {
            Scope = scope;
            Target = target;
            Method = method;
        }
        public Instance Call(Instance[] GivenArguments, Proc? Block) {
            return Method.Call(new Context(Method.Location, Scope, Target.SelfOrClass, Target, Block), GivenArguments);
        }
        public Instance Call(params Instance[] GivenArguments) {
            return Call(GivenArguments, null);
        }
        public int? ArgumentCount
            => Method.ArgumentsRange.Max;
        public override string ToString()
            => $"#<Method:{Target}.{Method}>";
    }
    public enum AccessModifier {
        Public,
        Private,
        Protected,
    }
    public enum ArgumentType {
        Normal,
        Splat,
        DoubleSplat,
        Block,
    }
    public enum MethodType {
        Adapted,
        Def,
        Block,
    }
}
