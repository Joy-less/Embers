using System;
using System.Linq;
using System.Reflection;

// Avoid ambiguity with Embers.Expression
using LExpression = System.Linq.Expressions.Expression;

namespace Embers {
    internal static class MethodBinder {
        public static Delegate CreateDelegate(object? Target, MethodInfo Method) {
            // Get delegate type (action or func)
            Type DelegateType = Method.ReturnType == typeof(void)
                ? LExpression.GetActionType(Method.GetParameters().Select(Parameter => Parameter.ParameterType).ToArray())
                : LExpression.GetFuncType(Method.GetParameters().Select(Parameter => Parameter.ParameterType).Append(Method.ReturnType).ToArray());
            // Create delegate
            return Method.CreateDelegate(DelegateType, Target);
        }
        public static object? InvokeBestOverload<TMethod>(TMethod[] Overloads, object? Target, Instance[] Arguments) where TMethod : MethodBase {
            bool GetPassArguments(TMethod Overload, out object?[] PassArguments) {
                // Get overload parameters
                ParameterInfo[] Parameters = Overload.GetParameters();
                // Create pass arguments array
                PassArguments = new object?[Parameters.Length];
                // Set valid flag
                bool Valid = true;
                // Not valid if more arguments passed than accepted
                if (Arguments.Length > Parameters.Length) {
                    Valid = false;
                }
                // Try match each parameter
                for (int i = 0; i < Parameters.Length; i++) {
                    ParameterInfo Parameter = Parameters[i];

                    // Argument given
                    if (i < Arguments.Length) {
                        // Get adapted argument
                        object? Argument = Adapter.GetObject(Arguments[i], Parameter.ParameterType);
                        // Pass argument
                        PassArguments[i] = Argument;
                        // Mark invalid if argument not assignable
                        if (Argument is null ? !Parameter.ParameterType.CanAssignNull() : !Argument.GetType().IsAssignableTo(Parameter.ParameterType)) {
                            Valid = false;
                        }
                    }
                    // Argument not given
                    else {
                        // Pass default value
                        PassArguments[i] = Parameter.DefaultValue;
                        // Mark invalid if argument mandatory
                        if (!Parameter.IsOptional) {
                            Valid = false;
                        }
                    }
                }
                // Return valid
                return Valid;
            }

            // Try match each overload
            foreach (TMethod Overload in Overloads) {
                if (GetPassArguments(Overload, out object?[] PassArguments)) {
                    return Overload.Invoke(Target, PassArguments);
                }
            }

            // If no overload matched, call first overload anyway
            TMethod FirstOverload = Overloads[0];
            GetPassArguments(FirstOverload, out object?[] FirstPassArguments);
            return FirstOverload.Invoke(Target, FirstPassArguments);
        }
        public static bool CanAssignNull(this Type Type) {
            return !Type.IsValueType || Nullable.GetUnderlyingType(Type) is not null;
        }
        public static bool IsParams(this ParameterInfo Parameter, Delegate Delegate) {
            return Delegate.GetType().GetMethod(nameof(Action.Invoke))!.GetParameters()[Parameter.Position].GetCustomAttribute<ParamArrayAttribute>() is not null;
        }
        /// <remarks>NOTE: Due to a C# bug, this always returns <see langword="false"/> for local function parameters.</remarks>
        public static bool IsParams(this ParameterInfo Parameter) {
            return Parameter.GetCustomAttribute<ParamArrayAttribute>() is not null;
        }
    }
}
