using System;
using System.Reflection;
using System.Threading.Tasks;

// Avoid ambiguity with Embers.Expression
using LExpression = System.Linq.Expressions.Expression;
using LParameterExpression = System.Linq.Expressions.ParameterExpression;
using LUnaryExpression = System.Linq.Expressions.UnaryExpression;
using LInvocationExpression = System.Linq.Expressions.InvocationExpression;
using LConstantExpression = System.Linq.Expressions.ConstantExpression;
using LBinaryExpression = System.Linq.Expressions.BinaryExpression;
using LMemberExpression = System.Linq.Expressions.MemberExpression;

namespace Embers {
    public sealed class FastDelegate {
        public readonly Delegate Original;

        private readonly Func<object?[], object?> CallFunc;
        private readonly Func<Task, object?>? TaskResultFunc;

        public FastDelegate(Delegate original, bool compile_call) {
            Original = original;

            // Get Delegate type
            Type DelegateType = original.GetType();

            // CallFunc
            if (compile_call) {
                // Compile type casting
                Func<Delegate, object?[], object?> CompiledCall = CompileMethodCall(DelegateType, original.Method);
                CallFunc = Arguments => CompiledCall(original, Arguments);
            }
            else {
                // Interpret type casting
                CallFunc = Arguments => original.Method.Invoke(original.Target, Arguments);
            }

            // Return type
            Type ReturnType = original.Method.ReturnType;
            if (ReturnType == typeof(Task) || (ReturnType.IsGenericType && ReturnType.GetGenericTypeDefinition() == typeof(Task<>))) {
                TaskResultFunc = CompileTaskResultCall(ReturnType);
            }
        }
        public object? Invoke(params object?[] Arguments) {
            object? Result = CallFunc(Arguments);
            return TaskResultFunc is not null ? AwaitTask((Task)Result!).Result : Result;
        }
        public async Task<object?> InvokeAsync(params object?[] Arguments) {
            object? Result = CallFunc(Arguments);
            return TaskResultFunc is not null ? await AwaitTask((Task)Result!) : Result;
        }

        private static Func<Delegate, object?[], object?> CompileMethodCall(Type MethodType, MethodInfo Method) {
            // Take Delegate
            LParameterExpression DelegateExp = LExpression.Parameter(typeof(Delegate), "Delegate");
            // Cast Delegate to Action/Func
            LUnaryExpression FunctionExp = LExpression.Convert(DelegateExp, MethodType);

            // Get method parameters
            ParameterInfo[] Parameters = Method.GetParameters();

            // Create converted parameters array
            LParameterExpression UnconvertedParameters = LExpression.Parameter(typeof(object[]), "Arguments");
            LExpression[] ConvertedParameters = new LExpression[Parameters.Length];

            // Convert given parameters
            for (int i = 0; i < Parameters.Length; i++) {
                LConstantExpression Index = LExpression.Constant(i, typeof(int));
                LBinaryExpression UnconvertedParameter = LExpression.ArrayIndex(UnconvertedParameters, Index);
                ConvertedParameters[i] = LExpression.Convert(UnconvertedParameter, Parameters[i].ParameterType);
            }

            // Invoke function
            LInvocationExpression InvokeExp = LExpression.Invoke(FunctionExp, ConvertedParameters);

            // Create return expression
            LExpression ReturnExp = Method.ReturnType != typeof(void)
                ? LExpression.Convert(InvokeExp, typeof(object)) // Return object
                : LExpression.Block(InvokeExp, LExpression.Constant(null, typeof(object))); // Return null

            // Compile call function
            return LExpression.Lambda<Func<Delegate, object?[], object?>>(ReturnExp, DelegateExp, UnconvertedParameters).Compile();
        }
        private async Task<object?> AwaitTask(Task Task) {
            await Task;
            if (Task.IsFaulted) {
                throw Task.Exception!;
            }
            return TaskResultFunc!(Task);
        }
        static Func<Task, object?> CompileTaskResultCall(Type TaskType) {
            // Task<T>
            if (TaskType.IsGenericType) {
                // Take Task
                LParameterExpression TaskExp = LExpression.Parameter(typeof(Task), "Task");
                // Cast Task to Task<T>
                LUnaryExpression GenericTaskExp = LExpression.Convert(TaskExp, TaskType);

                // Get Result property
                LMemberExpression ResultExp = LExpression.PropertyOrField(GenericTaskExp, nameof(Task<object>.Result));

                // Compile Result call
                return LExpression.Lambda<Func<Task, object?>>(ResultExp, TaskExp).Compile();
            }
            // Task
            else {
                return Task => null;
            }
        }
    }
}