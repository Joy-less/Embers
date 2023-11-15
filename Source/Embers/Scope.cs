using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Numerics;
using static Embers.Phase2;

#nullable enable
#pragma warning disable CS1998

namespace Embers
{
    public abstract class RubyObject {
        public Interpreter Interpreter { get; private set; }
        public RubyObject(Interpreter interpreter) {
            Interpreter = interpreter;
        }
        internal void SetInterpreter(Interpreter interpreter) {
            Interpreter = interpreter;
        }
    }
    public class Scope : RubyObject {
        public Scope? ParentScope { get; protected set; }
        public readonly bool AllowUnsafeApi;
        public Api Api => Interpreter.Api;
        public DebugLocation ApproximateLocation { get; private set; }
        public readonly LockingDictionary<string, Instance> LocalVariables = new();
        public readonly LockingDictionary<string, Instance> Constants = new();
        public readonly HashSet<ScopeThread> Threads = new();

        public AccessModifier CurrentAccessModifier { get; internal set; } = AccessModifier.Public;
        public Method CurrentMethod { get => FindScopeWhere(Scope => Scope.LocalCurrentMethod != null).LocalCurrentMethod!; internal set => LocalCurrentMethod = value; }
        public Module CurrentModule { get => FindScopeWhere(Scope => Scope.LocalCurrentModule != null).LocalCurrentModule!; internal set => LocalCurrentModule = value; }
        public Instance CurrentInstance { get => FindScopeWhere(Scope => Scope.LocalCurrentInstance != null).LocalCurrentInstance!; internal set => LocalCurrentInstance = value; }
        public Method? CurrentOnYield { get; internal set; }

        private Method? LocalCurrentMethod;
        private Module? LocalCurrentModule;
        private Instance? LocalCurrentInstance;

        private bool Stopping;

        public class ScopeThread {
            public Task? Running { get; private set; }
            public Scope? ThreadScope { get; private set; }
            public readonly Scope ParentScope;
            public Method? Method;
            private static readonly TimeSpan ShortTimeSpan = TimeSpan.FromMilliseconds(5);
            public ScopeThread(Scope parentScript) {
                ParentScope = parentScript;
            }
            public async Task Run(Instances? Arguments = null, Method? OnYield = null) {
                // If already running, wait until it's finished
                if (Running != null) {
                    await Running;
                    return;
                }
                // Add thread to running threads
                lock (ParentScope.Threads) ParentScope.Threads.Add(this);
                try {
                    // Create a new script
                    ThreadScope = new Scope(ParentScope);
                    // Call the method in the script
                    Running = Method!.Call(ThreadScope, null, Arguments, OnYield);
                    while (!ThreadScope.Stopping && !ParentScope.Stopping && !Running.IsCompleted) {
                        await Running.WaitAsync(ShortTimeSpan);
                    }
                    // Stop the script
                    ThreadScope?.Stop();
                }
                finally {
                    // Decrease thread counter
                    lock (ParentScope.Threads) ParentScope.Threads.Remove(this);
                }
            }
            public void Stop() {
                ThreadScope?.Stop();
            }
        }

        public IEnumerator<Scope> GetEnumerator() {
            Scope FindScope = this;
            while (true) {
                yield return FindScope;
                if (FindScope.ParentScope == null) break;
                FindScope = FindScope.ParentScope;
            }
        }
        Scope FindScopeWhere(Func<Scope, bool> Condition) {
            foreach (Scope Scope in this) {
                if (Condition(Scope)) {
                    return Scope;
                }
            }
            throw new InternalErrorException("No scope matched with condition");
        }

        public async Task Warn(string Message) {
            await CurrentInstance.CallInstanceMethod(this, "warn", new StringInstance(Api.String, Message));
        }
        public Module CreateModule(string Name, Module? Parent = null, Module? InheritsFrom = null) {
            Parent ??= CurrentModule;
            InheritsFrom ??= Interpreter.Class;
            Module NewModule = new(Name, Parent, InheritsFrom);
            Parent.Constants[Name] = new ModuleReference(NewModule);
            return NewModule;
        }
        public Class CreateClass(string Name, Module? Parent = null, Module? InheritsFrom = null) {
            Parent ??= CurrentModule;
            InheritsFrom ??= Interpreter.Class;
            Class NewClass = new(Name, Parent, InheritsFrom);
            Parent.Constants[Name] = new ModuleReference(NewClass);
            return NewClass;
        }
        public Method CreateMethod(Func<MethodInput, Task<Instance>> Function, Range ArgumentCountRange, bool IsUnsafe = false) {
            Method NewMethod = new(Function, new IntRange(ArgumentCountRange), IsUnsafe: IsUnsafe, accessModifier: CurrentAccessModifier);
            return NewMethod;
        }
        public Method CreateMethod(Func<MethodInput, Task<Instance>> Function, IntRange? ArgumentCountRange, bool IsUnsafe = false) {
            Method NewMethod = new(Function, ArgumentCountRange, IsUnsafe: IsUnsafe, accessModifier: CurrentAccessModifier);
            return NewMethod;
        }
        public Method CreateMethod(Func<MethodInput, Task<Instance>> Function, int ArgumentCount, bool IsUnsafe = false) {
            Method NewMethod = new(Function, ArgumentCount, IsUnsafe: IsUnsafe, accessModifier: CurrentAccessModifier);
            return NewMethod;
        }
        public bool TryGetLocalVariable(string Name, out Instance? LocalVariable) {
            foreach (Scope Scope in this) {
                if (Scope.LocalVariables.TryGetValue(Name, out Instance? FindLocalVariable)) {
                    LocalVariable = FindLocalVariable;
                    return true;
                }
            }
            LocalVariable = null;
            return false;
        }
        public bool TryGetLocalConstant(string Name, out Instance? LocalConstant) {
            foreach (Scope Scope in this) {
                if (Scope.Constants.TryGetValue(Name, out Instance? FindLocalConstant) || (Scope.LocalCurrentModule != null && Scope.LocalCurrentModule.Constants.TryGetValue(Name, out FindLocalConstant))) {
                    LocalConstant = FindLocalConstant;
                    return true;
                }
            }
            LocalConstant = null;
            return false;
        }
        public bool TryGetLocalInstanceMethod(string Name, out Method? LocalInstanceMethod, out Instance? OnInstance) {
            foreach (Scope Scope in this) {
                if (Scope.CurrentInstance != null && Scope.CurrentInstance.TryGetInstanceMethod(Name, out Method? FindLocalInstanceMethod)) {
                    LocalInstanceMethod = FindLocalInstanceMethod;
                    OnInstance = Scope.CurrentInstance;
                    return true;
                }
            }
            LocalInstanceMethod = null;
            OnInstance = null;
            return false;
        }
        public Dictionary<string, Instance> GetAllLocalVariables() {
            Dictionary<string, Instance> LocalVariables = new();
            foreach (Scope Scope in this) {
                foreach (KeyValuePair<string, Instance> LocalVariable in Scope.LocalVariables) {
                    LocalVariables[LocalVariable.Key] = LocalVariable.Value;
                }
            }
            return LocalVariables;
        }
        public Dictionary<string, Instance> GetAllLocalConstants() {
            Dictionary<string, Instance> Constants = new();
            foreach (Scope Scope in this) {
                if (Scope.CurrentModule != null) {
                    foreach (KeyValuePair<string, Instance> Constant in Scope.CurrentModule.Constants) {
                        Constants[Constant.Key] = Constant.Value;
                    }
                }
                foreach (KeyValuePair<string, Instance> Constant in Scope.Constants) {
                    Constants[Constant.Key] = Constant.Value;
                }
                if (Scope.CurrentModule != null) break;
            }
            return Constants;
        }
        internal Method? ToYieldMethod(Method? Current) {
            // This makes yield methods (do ... end) be called back in the scope they're defined in, not in the scope of the method.
            // e.g. 5.times do ... end should be called in the scope of the line, not in the instance of 5.
            if (Current != null) {
                Func<MethodInput, Task<Instance>> CurrentFunction = Current.Function;
                Current.ChangeFunction(async Input => {
                    Input.OverrideScope(this);
                    await Current.SetArgumentVariables(Input.Scope, Input);
                    Instance Result = await CurrentFunction(Input);
                    if (Result is ReturnCodeInstance ReturnCodeInstance) {
                        ReturnCodeInstance.CalledInYieldMethod = true;
                    }
                    return Result;
                });
            }
            return Current;
        }
        async Task<Instances> InterpretArgumentsAsync(List<Expression> Expressions) {
            List<Instance> Arguments = new();
            foreach (Expression Expression in Expressions) {
                Instance Argument = await InterpretExpressionAsync(Expression);
                if (Argument is ReturnCodeInstance) {
                    return Argument;
                }
                Arguments.Add(Argument);
            }
            return Arguments;
        }

        async Task<Instance> InterpretMethodCallExpression(MethodCallExpression MethodCallExpression) {
            Instance MethodPath = await InterpretExpressionAsync(MethodCallExpression.MethodPath, ReturnType.HypotheticalVariable);
            if (MethodPath is VariableReference MethodReference) {
                // Get arguments
                Instances Arguments = await InterpretArgumentsAsync(MethodCallExpression.Arguments);
                if (Arguments.Count == 1 && Arguments[0] is ReturnCodeInstance ReturnCodeInstance) {
                    return ReturnCodeInstance;
                }
                // Static method
                if (MethodReference.Module != null) {
                    // Get class/module which owns method
                    Module MethodModule = MethodReference.Module;
                    // Call class method
                    return await MethodModule.CallMethod(this, MethodReference.Token.Value!, Arguments, MethodCallExpression.OnYield?.ToYieldMethod(this, CurrentOnYield));
                }
                // Instance method
                else {
                    // Get instance which owns method
                    Instance MethodInstance = MethodReference.IsLocalReference ? CurrentInstance : MethodReference.Instance!;
                    // Call instance method
                    return await MethodInstance.CallInstanceMethod(this, MethodReference.Token.Value!, Arguments, MethodCallExpression.OnYield?.ToYieldMethod(this, CurrentOnYield));
                }
            }
            else {
                throw new InternalErrorException($"{MethodCallExpression.Location}: MethodPath should be VariableReference, not {MethodPath.GetType().Name}");
            }
        }
        async Task<Instance> InterpretObjectTokenExpression(ObjectTokenExpression ObjectTokenExpression, ReturnType ReturnType) {
            // Path
            if (ObjectTokenExpression is PathExpression PathExpression) {
                Instance ParentInstance = await InterpretExpressionAsync(PathExpression.ParentObject);
                // Class method
                if (ParentInstance is ModuleReference ParentModule) {
                    // Method
                    if (ReturnType != ReturnType.HypotheticalVariable) {
                        // Call class method
                        if (ReturnType == ReturnType.InterpretResult) {
                            return await ParentModule.Module!.CallMethod(this, PathExpression.Token.Value!);
                        }
                        else {
                            // Return class method
                            if (ParentModule.Module!.Methods.ContainsKey(PathExpression.Token.Value!)) {
                                return new VariableReference(ParentModule.Module, PathExpression.Token);
                            }
                            // Error
                            else {
                                throw new RuntimeException($"{PathExpression.Token.Location}: Undefined method '{PathExpression.Token.Value!}' for {ParentModule.Module.Name}");
                            }
                        }
                    }
                    // New method
                    else {
                        return new VariableReference(ParentModule.Module!, PathExpression.Token);
                    }
                }
                // Instance method
                else {
                    // Method
                    if (ReturnType != ReturnType.HypotheticalVariable) {
                        // Call instance method
                        if (ReturnType == ReturnType.InterpretResult) {
                            return await ParentInstance.CallInstanceMethod(this, PathExpression.Token.Value!);
                        }
                        else {
                            // Return instance method
                            if (ParentInstance.TryGetInstanceMethod(PathExpression.Token.Value!, out _)) {
                                return new VariableReference(ParentInstance, PathExpression.Token);
                            }
                            // Error
                            else {
                                throw new RuntimeException($"{PathExpression.Token.Location}: Undefined method '{PathExpression.Token.Value!}' for {ParentInstance.Inspect()}");
                            }
                        }
                    }
                    // New method
                    else {
                        return new VariableReference(ParentInstance, PathExpression.Token);
                    }
                }
            }
            // Constant Path
            else if (ObjectTokenExpression is ConstantPathExpression ConstantPathExpression) {
                Instance ParentInstance = await InterpretExpressionAsync(ConstantPathExpression.ParentObject);
                // Constant
                if (ReturnType != ReturnType.HypotheticalVariable) {
                    // Constant
                    if (ParentInstance.Module!.Constants.TryGetValue(ConstantPathExpression.Token.Value!, out Instance? ConstantValue)) {
                        // Return constant
                        if (ReturnType == ReturnType.InterpretResult) {
                            return ConstantValue;
                        }
                        // Return constant reference
                        else {
                            return new VariableReference(ParentInstance.Module, ConstantPathExpression.Token);
                        }
                    }
                    // Error
                    else {
                        throw new RuntimeException($"{ConstantPathExpression.Token.Location}: Uninitialized constant {ConstantPathExpression.Inspect()}");
                    }
                }
                // New constant
                else {
                    return new VariableReference(ParentInstance.Module!, ConstantPathExpression.Token);
                }
            }
            // Local
            else {
                // Literal
                if (ObjectTokenExpression.Token.IsObjectToken) {
                    return await Instance.CreateFromToken(this, ObjectTokenExpression.Token);
                }
                else {
                    if (ReturnType != ReturnType.HypotheticalVariable) {
                        switch (ObjectTokenExpression.Token.Type) {
                            // Local variable or method
                            case Phase2TokenType.LocalVariableOrMethod: {
                                // Local variable (priority)
                                if (TryGetLocalVariable(ObjectTokenExpression.Token.Value!, out Instance? Value)) {
                                    // Return local variable value
                                    if (ReturnType == ReturnType.InterpretResult) {
                                        return Value!;
                                    }
                                    // Return local variable reference
                                    else {
                                        return new VariableReference(ObjectTokenExpression.Token, Interpreter);
                                    }
                                }
                                // Method
                                else if (TryGetLocalInstanceMethod(ObjectTokenExpression.Token.Value!, out Method? Method, out Instance? OnInstance)) {
                                    // Call local method
                                    if (ReturnType == ReturnType.InterpretResult) {
                                        return await Method!.Call(this, OnInstance!);
                                    }
                                    // Return method reference
                                    else {
                                        return new VariableReference(ObjectTokenExpression.Token, Interpreter);
                                    }
                                }
                                // Undefined
                                else {
                                    throw new RuntimeException($"{ObjectTokenExpression.Token.Location}: Undefined local variable or method '{ObjectTokenExpression.Token.Value!}' for {this}");
                                }
                            }
                            // Global variable
                            case Phase2TokenType.GlobalVariable: {
                                if (Interpreter.GlobalVariables.TryGetValue(ObjectTokenExpression.Token.Value!, out Instance? Value)) {
                                    // Return global variable value
                                    if (ReturnType == ReturnType.InterpretResult) {
                                        return Value;
                                    }
                                    // Return global variable reference
                                    else {
                                        return new VariableReference(ObjectTokenExpression.Token, Interpreter);
                                    }
                                }
                                else {
                                    return Api.Nil;
                                }
                            }
                            // Constant
                            case Phase2TokenType.ConstantOrMethod: {
                                // Constant (priority)
                                if (TryGetLocalConstant(ObjectTokenExpression.Token.Value!, out Instance? ConstantValue)) {
                                    // Return constant value
                                    if (ReturnType == ReturnType.InterpretResult) {
                                        return ConstantValue!;
                                    }
                                    // Return constant reference
                                    else {
                                        return new VariableReference(ObjectTokenExpression.Token, Interpreter);
                                    }
                                }
                                // Method
                                else if (TryGetLocalInstanceMethod(ObjectTokenExpression.Token.Value!, out Method? Method, out Instance? OnInstance)) {
                                    // Call local method
                                    if (ReturnType == ReturnType.InterpretResult) {
                                        return await Method!.Call(this, OnInstance!);
                                    }
                                    // Return method reference
                                    else {
                                        return new VariableReference(ObjectTokenExpression.Token, Interpreter);
                                    }
                                }
                                // Uninitialized
                                else {
                                    throw new RuntimeException($"{ObjectTokenExpression.Token.Location}: Uninitialized constant '{ObjectTokenExpression.Token.Value!}' for {CurrentModule.Name}");
                                }
                            }
                            // Instance variable
                            case Phase2TokenType.InstanceVariable: {
                                if (CurrentInstance.InstanceVariables.TryGetValue(ObjectTokenExpression.Token.Value!, out Instance? Value)) {
                                    // Return instance variable value
                                    if (ReturnType == ReturnType.InterpretResult) {
                                        return Value;
                                    }
                                    // Return instance variable reference
                                    else {
                                        return new VariableReference(ObjectTokenExpression.Token, Interpreter);
                                    }
                                }
                                else {
                                    return Api.Nil;
                                }
                            }
                            // Class variable
                            case Phase2TokenType.ClassVariable: {
                                if (CurrentModule.ClassVariables.TryGetValue(ObjectTokenExpression.Token.Value!, out Instance? Value)) {
                                    // Return class variable value
                                    if (ReturnType == ReturnType.InterpretResult) {
                                        return Value;
                                    }
                                    // Return class variable reference
                                    else {
                                        return new VariableReference(ObjectTokenExpression.Token, Interpreter);
                                    }
                                }
                                else {
                                    throw new RuntimeException($"{ObjectTokenExpression.Token.Location}: Uninitialized class variable '{ObjectTokenExpression.Token.Value!}' for {CurrentModule.Name}");
                                }
                            }
                            // Error
                            default:
                                throw new InternalErrorException($"{ObjectTokenExpression.Token.Location}: Unknown variable type {ObjectTokenExpression.Token.Type}");
                        }
                    }
                    // Variable
                    else {
                        return new VariableReference(ObjectTokenExpression.Token, Interpreter);
                    }
                }
            }
        }
        async Task<Instance> InterpretIfExpression(IfExpression IfExpression) {
            if (IfExpression.Condition == null || (await InterpretExpressionAsync(IfExpression.Condition)).IsTruthy != IfExpression.Inverse) {
                return await new Scope(this, CurrentOnYield).InternalInterpretAsync(IfExpression.Statements);
            }
            return Api.Nil;
        }
        async Task<Instance> InterpretRescueExpression(RescueExpression RescueExpression) {
            try {
                await InterpretExpressionAsync(RescueExpression.Statement);
            }
            catch {
                await InterpretExpressionAsync(RescueExpression.RescueStatement);
            }
            return Api.Nil;
        }
        async Task<Instance> InterpretTernaryExpression(TernaryExpression TernaryExpression) {
            bool ConditionIsTruthy = (await InterpretExpressionAsync(TernaryExpression.Condition)).IsTruthy;
            if (ConditionIsTruthy) {
                return await InterpretExpressionAsync(TernaryExpression.ExpressionIfTrue);
            }
            else {
                return await InterpretExpressionAsync(TernaryExpression.ExpressionIfFalse);
            }
        }
        async Task<Instance> InterpretCaseExpression(CaseExpression CaseExpression) {
            Instance Subject = await InterpretExpressionAsync(CaseExpression.Subject);
            foreach (WhenExpression Branch in CaseExpression.Branches) {
                // Check if when statements apply
                bool WhenApplies = false;
                // When
                if (Branch.Conditions.Count != 0) {
                    foreach (Expression Condition in Branch.Conditions) {
                        Instance ConditionObject = await InterpretExpressionAsync(Condition);
                        if ((await ConditionObject.CallInstanceMethod(this, "===", Subject)).IsTruthy) {
                            WhenApplies = true;
                        }
                    }
                }
                // Else
                else {
                    WhenApplies = true;
                }
                // Run when statements
                if (WhenApplies) {
                    return await new Scope(this, CurrentOnYield).InternalInterpretAsync(Branch.Statements);
                }
            }
            return Api.Nil;
        }
        async Task<ArrayInstance> InterpretArrayExpression(ArrayExpression ArrayExpression) {
            List<Instance> Items = new();
            foreach (Expression Item in ArrayExpression.Expressions) {
                Items.Add(await InterpretExpressionAsync(Item));
            }
            return new ArrayInstance(Api.Array, Items);
        }
        async Task<HashInstance> InterpretHashExpression(HashExpression HashExpression) {
            HashDictionary Items = new();
            foreach (KeyValuePair<Expression, Expression> Item in HashExpression.Expressions) {
                await Items.Store(this, await InterpretExpressionAsync(Item.Key), await InterpretExpressionAsync(Item.Value));
            }
            return new HashInstance(Api.Hash, Items, Api.Nil);
        }
        async Task<Instance> InterpretWhileExpression(WhileExpression WhileExpression) {
            while ((await InterpretExpressionAsync(WhileExpression.Condition!)).IsTruthy != WhileExpression.Inverse) {
                Instance Result = await new Scope(this, CurrentOnYield).InternalInterpretAsync(WhileExpression.Statements);
                if (Result is LoopControlReturnCode LoopControlReturnCode) {
                    if (LoopControlReturnCode.Type is LoopControlType.Break) {
                        break;
                    }
                    else if (LoopControlReturnCode.Type is LoopControlType.Retry) {
                        throw new SyntaxErrorException($"{ApproximateLocation}: Retry not valid in while loop");
                    }
                    else if (LoopControlReturnCode.Type is LoopControlType.Redo or LoopControlType.Next) {
                        continue;
                    }
                }
                else if (Result is ReturnCodeInstance) {
                    return Result;
                }
            }
            return Api.Nil;
        }
        async Task<Instance> InterpretWhileStatement(WhileStatement WhileStatement) {
            // Run statements
            await new Scope(this).InterpretExpressionAsync(WhileStatement.WhileExpression);
            return Api.Nil;
        }
        async Task<Instance> InterpretForStatement(ForStatement ForStatement) {
            Instance InResult = await InterpretExpressionAsync(ForStatement.InExpression);
            await InResult.CallInstanceMethod(this, "each", OnYield: ForStatement.BlockStatementsMethod);
            return Api.Nil;
        }
        async Task<Instance> InterpretLogicalExpression(LogicalExpression LogicalExpression) {
            Instance Left = await InterpretExpressionAsync(LogicalExpression.Left);
            switch (LogicalExpression.LogicType) {
                case LogicalExpression.LogicalExpressionType.And:
                    if (!Left.IsTruthy)
                        return Left;
                    break;
            }
            Instance Right = await InterpretExpressionAsync(LogicalExpression.Right);
            switch (LogicalExpression.LogicType) {
                case LogicalExpression.LogicalExpressionType.And:
                    return Right;
                case LogicalExpression.LogicalExpressionType.Or:
                    if (Left.IsTruthy)
                        return Left;
                    else
                        return Right;
                case LogicalExpression.LogicalExpressionType.Xor:
                    if (Left.IsTruthy && !Right.IsTruthy)
                        return Left;
                    else if (!Left.IsTruthy && Right.IsTruthy)
                        return Right;
                    else
                        return Api.False;
                default:
                    throw new InternalErrorException($"{LogicalExpression.Location}: Unhandled logical expression type: '{LogicalExpression.LogicType}'");
            }
        }
        async Task<Instance> InterpretNotExpression(NotExpression NotExpression) {
            Instance Right = await InterpretExpressionAsync(NotExpression.Right);
            return Right.IsTruthy ? Api.False : Api.True;
        }
        async Task<Instance> InterpretDefineMethodStatement(DefineMethodStatement DefineMethodStatement) {
            Instance MethodNameObject = await InterpretExpressionAsync(DefineMethodStatement.MethodName, ReturnType.HypotheticalVariable);
            if (MethodNameObject is VariableReference MethodNameRef) {
                string MethodName = MethodNameRef.Token.Value!;
                // Define static method
                if (MethodNameRef.Module != null) {
                    Module MethodModule = MethodNameRef.Module;
                    // Prevent redefining unsafe API methods
                    if (!AllowUnsafeApi && MethodModule.TryGetMethod(MethodName, out Method? ExistingMethod) && ExistingMethod!.Unsafe) {
                        throw new RuntimeException($"{DefineMethodStatement.Location}: The static method '{MethodName}' cannot be redefined since AllowUnsafeApi is disabled for this script.");
                    }
                    // Create or overwrite static method
                    MethodModule.Methods[MethodName] = DefineMethodStatement.MethodExpression.ToMethod(CurrentAccessModifier);
                }
                // Define instance method
                else {
                    Instance MethodInstance = MethodNameRef.Instance ?? CurrentInstance;
                    // Prevent redefining unsafe API methods
                    if (!AllowUnsafeApi && MethodInstance.TryGetInstanceMethod(MethodName, out Method? ExistingMethod) && ExistingMethod!.Unsafe) {
                        throw new RuntimeException($"{DefineMethodStatement.Location}: The instance method '{MethodName}' cannot be redefined since AllowUnsafeApi is disabled for this script.");
                    }
                    // Create or overwrite instance method
                    Method NewInstanceMethod = DefineMethodStatement.MethodExpression.ToMethod(CurrentAccessModifier);
                    if (MethodNameRef.Instance != null) {
                        // Define method for a specific instance
                        MethodInstance.InstanceMethods[MethodName] = NewInstanceMethod;
                    }
                    else {
                        // Define method for all instances of a class
                        MethodInstance.Module!.InstanceMethods[MethodName] = NewInstanceMethod;
                    }
                }
            }
            else {
                throw new InternalErrorException($"{DefineMethodStatement.Location}: Invalid method name: {MethodNameObject}");
            }
            return Api.Nil;
        }
        async Task<Instance> InterpretDefineClassStatement(DefineClassStatement DefineClassStatement) {
            Instance ClassNameObject = await InterpretExpressionAsync(DefineClassStatement.ClassName, ReturnType.HypotheticalVariable);
            if (ClassNameObject is VariableReference ClassNameRef) {
                string ClassName = ClassNameRef.Token.Value!;
                Module? InheritsFrom = DefineClassStatement.InheritsFrom != null ? (await InterpretExpressionAsync(DefineClassStatement.InheritsFrom)).Module : null;

                // Create or patch class
                Module NewModule;
                // Patch class
                if ((ClassNameRef.Module ?? CurrentModule).Constants.TryGetValue(ClassName, out Instance? ConstantValue) && ConstantValue is ModuleReference ModuleReference) {
                    if (InheritsFrom != null) {
                        throw new SyntaxErrorException($"{DefineClassStatement.Location}: Cannot change the inheritance of a class/module");
                    }
                    NewModule = ModuleReference.Module!;
                }
                // Create class
                else {
                    if (DefineClassStatement.IsModule) {
                        if (ClassNameRef.Module != null) {
                            NewModule = CreateModule(ClassName, ClassNameRef.Module, InheritsFrom);
                        }
                        else {
                            NewModule = CreateModule(ClassName, (ClassNameRef.Instance ?? CurrentInstance).Module!, InheritsFrom);
                        }
                    }
                    else {
                        if (ClassNameRef.Module != null) {
                            NewModule = CreateClass(ClassName, ClassNameRef.Module, InheritsFrom);
                        }
                        else {
                            NewModule = CreateClass(ClassName, (ClassNameRef.Instance ?? CurrentInstance).Module!, InheritsFrom);
                        }
                    }
                }
                // Interpret class statements
                Scope ModuleScope = new(this, CurrentOnYield) { CurrentModule = NewModule, CurrentInstance = new ModuleReference(NewModule), CurrentAccessModifier = AccessModifier.Public };
                await ModuleScope.InternalInterpretAsync(DefineClassStatement.BlockStatements);
            }
            else {
                throw new InternalErrorException($"{DefineClassStatement.Location}: Invalid class/module name: {ClassNameObject}");
            }
            return Api.Nil;
        }
        async Task<Instance> InterpretYieldExpression(YieldExpression YieldExpression) {
            if (CurrentOnYield != null) {
                Instances? YieldArgs = null;
                if (YieldExpression.YieldValues != null) {
                    YieldArgs = await InterpretArgumentsAsync(YieldExpression.YieldValues);
                    if (YieldArgs.Count == 1 && YieldArgs[0] is ReturnCodeInstance ReturnCodeInstance) {
                        throw new SyntaxErrorException($"{YieldExpression.Location}: Invalid {ReturnCodeInstance.GetType().Name}");
                    }
                }
                return await CurrentOnYield.Call(this, null, YieldArgs, BreakHandleType: BreakHandleType.Destroy, CatchReturn: false);
            }
            else {
                throw new RuntimeException($"{YieldExpression.Location}: No block given to yield to");
            }
        }
        async Task<Instance> InterpretSuperExpression(SuperExpression SuperExpression) {
            Module CurrentModule = this.CurrentModule;
            string? SuperMethodName = CurrentMethod?.Name;
            if (CurrentModule.SuperModule is Module SuperModule) {
                if (SuperMethodName != null && SuperModule.TryGetInstanceMethod(SuperMethodName, out Method? SuperMethod)) {
                    Instances? Arguments = null;
                    if (SuperExpression.Arguments != null) {
                        Arguments = await InterpretArgumentsAsync(SuperExpression.Arguments);
                        if (Arguments.Count == 1 && Arguments[0] is ReturnCodeInstance ReturnCodeInstance) {
                            return ReturnCodeInstance;
                        }
                    }
                    return await SuperMethod!.Call(this, null, Arguments, BypassAccessModifiers: true);
                }
            }
            throw new RuntimeException($"{SuperExpression.Location}: No super method '{SuperMethodName}' to call");
        }
        async Task<Instance> InterpretAliasStatement(AliasStatement AliasStatement) {
            Instance MethodAlias = await InterpretExpressionAsync(AliasStatement.AliasAs, ReturnType.HypotheticalVariable);
            if (MethodAlias is VariableReference MethodAliasRef) {
                Instance MethodOrigin = await InterpretExpressionAsync(AliasStatement.MethodToAlias, ReturnType.FoundVariable);
                if (MethodOrigin is VariableReference MethodOriginRef) {
                    // Get target methods dictionary
                    LockingDictionary<string, Method> TargetMethods = MethodAliasRef.Instance != null ? MethodAliasRef.Instance.InstanceMethods
                        : (MethodAliasRef.Module != null ? MethodAliasRef.Module.Methods : CurrentInstance.InstanceMethods);
                    // Get origin method
                    Method OriginMethod;
                    if (MethodOriginRef.Instance != null) {
                        MethodOriginRef.Instance.TryGetInstanceMethod(MethodOriginRef.Token.Value!, out OriginMethod!);
                    }
                    else if (MethodOriginRef.Module != null) {
                        MethodOriginRef.Module.TryGetMethod(MethodOriginRef.Token.Value!, out OriginMethod!);
                    }
                    else {
                        CurrentInstance.TryGetInstanceMethod(MethodOriginRef.Token.Value!, out OriginMethod!);
                    }
                    // Create alias for method
                    TargetMethods[AliasStatement.AliasAs.Token.Value!] = OriginMethod;
                }
                else {
                    throw new SyntaxErrorException($"{AliasStatement.Location}: Expected method to alias, got '{MethodOrigin.Inspect()}'");
                }
            }
            else {
                throw new SyntaxErrorException($"{AliasStatement.Location}: Expected method alias, got '{MethodAlias.Inspect()}'");
            }
            return Api.Nil;
        }
        async Task<Instance> InterpretRangeExpression(RangeExpression RangeExpression) {
            Instance? RawMin = null;
            if (RangeExpression.Min != null) RawMin = await InterpretExpressionAsync(RangeExpression.Min);
            Instance? RawMax = null;
            if (RangeExpression.Max != null) RawMax = await InterpretExpressionAsync(RangeExpression.Max);

            if (RawMin is IntegerInstance Min && RawMax is IntegerInstance Max) {
                return new RangeInstance(Api.Range, Min, Max, RangeExpression.IncludesMax);
            }
            else if (RawMin == null && RawMax is IntegerInstance MaxOnly) {
                return new RangeInstance(Api.Range, null, MaxOnly, RangeExpression.IncludesMax);
            }
            else if (RawMax == null && RawMin is IntegerInstance MinOnly) {
                return new RangeInstance(Api.Range, MinOnly, null, RangeExpression.IncludesMax);
            }
            else {
                throw new RuntimeException($"{RangeExpression.Location}: Range bounds must be integers (got '{RawMin?.LightInspect()}' and '{RawMax?.LightInspect()}')");
            }
        }
        async Task<Instance> InterpretIfBranchesStatement(IfBranchesStatement IfStatement) {
            for (int i = 0; i < IfStatement.Branches.Count; i++) {
                IfExpression Branch = IfStatement.Branches[i];
                // If / elsif
                if (Branch.Condition != null) {
                    Instance ConditionResult = await InterpretExpressionAsync(Branch.Condition);
                    if (ConditionResult.IsTruthy != Branch.Inverse) {
                        // Run statements
                        return await new Scope(this, CurrentOnYield).InternalInterpretAsync(Branch.Statements);
                    }
                }
                // Else
                else {
                    // Run statements
                    return await new Scope(this, CurrentOnYield).InternalInterpretAsync(Branch.Statements);
                }
            }
            return Api.Nil;
        }
        async Task<Instance> InterpretBeginBranchesStatement(BeginBranchesStatement BeginBranchesStatement) {
            // Begin
            BeginStatement BeginBranch = (BeginStatement)BeginBranchesStatement.Branches[0];
            Exception ExceptionToRescue;
            try {
                return await new Scope(this, CurrentOnYield).InternalInterpretAsync(BeginBranch.Statements);
            }
            catch (Exception Ex) {
                ExceptionToRescue = Ex;
            }

            // Rescue
            bool Rescued = false;
            if (ExceptionToRescue != null) {
                // Find a rescue statement that can rescue the given error
                for (int i = 1; i < BeginBranchesStatement.Branches.Count; i++) {
                    BeginComponentStatement Branch = BeginBranchesStatement.Branches[i];
                    if (Branch is RescueStatement RescueStatement) {
                        // Get or create the exception to rescue
                        Interpreter.ExceptionsTable.TryGetValue(ExceptionToRescue, out ExceptionInstance? ExceptionInstance);
                        ExceptionInstance ??= new ExceptionInstance(Api.RuntimeError, ExceptionToRescue);
                        // Get the rescuing exception type
                        Module RescuingExceptionModule = RescueStatement.Exception != null
                            ? (await InterpretExpressionAsync(RescueStatement.Exception)).Module!
                            : Api.StandardError;

                        // Check whether rescue applies to this exception
                        bool CanRescue = false;
                        if (ExceptionInstance.Module!.InheritsFrom(RescuingExceptionModule)) {
                            CanRescue = true;
                        }

                        // Run the statements in the rescue block
                        if (CanRescue) {
                            Rescued = true;
                            Scope RescueScope = new(this, CurrentOnYield);
                            // Set exception variable to exception instance
                            if (RescueStatement.ExceptionVariable != null) {
                                RescueScope.LocalVariables[RescueStatement.ExceptionVariable.Value!] = ExceptionInstance;
                            }
                            // Run statements
                            await RescueScope.InternalInterpretAsync(RescueStatement.Statements);
                            break;
                        }
                    }
                }
                // Rethrow exception if not rescued
                if (!Rescued) throw ExceptionToRescue;
            }

            // Ensure & Else
            for (int i = 1; i < BeginBranchesStatement.Branches.Count; i++) {
                BeginComponentStatement Branch = BeginBranchesStatement.Branches[i];
                if (Branch is EnsureStatement || (Branch is RescueElseStatement && !Rescued)) {
                    // Run statements
                    await new Scope(this, CurrentOnYield).InternalInterpretAsync(Branch.Statements);
                }
            }

            return Api.Nil;
        }
        async Task AssignToVariable(VariableReference Variable, Instance Value) {
            Scope FindMostAppropriateScope(Func<Scope, bool> IsAppropriate) {
                Scope SetScope = this;
                foreach (Scope Scope in this) {
                    if (IsAppropriate(Scope)) {
                        SetScope = Scope;
                        break;
                    }
                    else if (Scope.CurrentModule != null || Scope.CurrentInstance != null) {
                        break;
                    }
                }
                return SetScope;
            }

            switch (Variable.Token.Type) {
                case Phase2TokenType.LocalVariableOrMethod:
                    // call instance.variable=
                    if (Variable.Instance != null) {
                        await Variable.Instance.CallInstanceMethod(this, Variable.Token.Value! + "=", Value);
                    }
                    // set variable =
                    else {
                        // Find most appropriate local variable scope
                        Scope SetLocalVariableScope = FindMostAppropriateScope(Scope => Scope.LocalVariables.ContainsKey(Variable.Token.Value!));
                        // Set local variable
                        SetLocalVariableScope.LocalVariables[Variable.Token.Value!] = Value;
                    }
                    break;
                case Phase2TokenType.GlobalVariable:
                    Interpreter.GlobalVariables[Variable.Token.Value!] = Value;
                    break;
                case Phase2TokenType.ConstantOrMethod:
                    // Find appropriate constant scope
                    Scope SetLocalConstantScope = FindMostAppropriateScope(Scope => Scope.Constants.ContainsKey(Variable.Token.Value!));
                    // Warn if constant already initialized
                    if (SetLocalConstantScope.Constants.ContainsKey(Variable.Token.Value!)) {
                        await Warn($"{Variable.Token.Location}: Already initialized constant '{Variable.Token.Value!}'");
                    }
                    // Set local constant
                    SetLocalConstantScope.Constants[Variable.Token.Value!] = Value;
                    break;
                case Phase2TokenType.InstanceVariable:
                    CurrentInstance.InstanceVariables[Variable.Token.Value!] = Value;
                    break;
                case Phase2TokenType.ClassVariable:
                    CurrentModule.ClassVariables[Variable.Token.Value!] = Value;
                    break;
                default:
                    throw new InternalErrorException($"{Variable.Token.Location}: Assignment variable token is not a variable type (got {Variable.Token.Type})");
            }
        }
        async Task<Instance> InterpretAssignmentExpression(AssignmentExpression AssignmentExpression, ReturnType ReturnType) {
            Instance Right = await InterpretExpressionAsync(AssignmentExpression.Right);
            Instance Left = await InterpretExpressionAsync(AssignmentExpression.Left, ReturnType.HypotheticalVariable);

            if (Left is VariableReference LeftVariable) {
                if (Right is Instance RightInstance) {
                    // LeftVariable = RightInstance
                    await AssignToVariable(LeftVariable, RightInstance);
                    // Return left variable reference or value
                    if (ReturnType == ReturnType.InterpretResult) {
                        return RightInstance;
                    }
                    else {
                        return Left;
                    }
                }
                else {
                    throw new InternalErrorException($"{LeftVariable.Token.Location}: Assignment value should be an instance, but got {Right.GetType().Name}");
                }
            }
            else {
                throw new RuntimeException($"{AssignmentExpression.Left.Location}: {Left.GetType()} cannot be the target of an assignment");
            }
        }
        async Task<Instance> InterpretMultipleAssignmentExpression(MultipleAssignmentExpression MultipleAssignmentExpression, ReturnType ReturnType) {
            // Check if assigning variables from array (e.g. a, b = [c, d])
            Instance FirstRight = await InterpretExpressionAsync(MultipleAssignmentExpression.Right[0]);
            ArrayInstance? AssigningFromArray = null;
            if (MultipleAssignmentExpression.Right.Count == 1 && FirstRight is ArrayInstance AssignmentValueArray) {
                AssigningFromArray = AssignmentValueArray;
            }
            // Assign each variable to each value
            List<Instance> AssignedValues = new();
            for (int i = 0; i < MultipleAssignmentExpression.Left.Count; i++) {
                Instance Right = AssigningFromArray == null
                    ? i != 0 ? await InterpretExpressionAsync(MultipleAssignmentExpression.Right[i]) : FirstRight
                    : await AssigningFromArray.CallInstanceMethod(this, "[]", new IntegerInstance(Api.Integer, i));
                Instance Left = await InterpretExpressionAsync(MultipleAssignmentExpression.Left[i], ReturnType.HypotheticalVariable);

                if (Left is VariableReference LeftVariable) {
                    if (Right is Instance RightInstance) {
                        // LeftVariable = RightInstance
                        await AssignToVariable(LeftVariable, RightInstance);
                        // Return left variable reference or value
                        if (ReturnType == ReturnType.InterpretResult) {
                            AssignedValues.Add(RightInstance);
                        }
                        else {
                            throw new InternalErrorException($"{MultipleAssignmentExpression.Location}: Cannot get variable reference from multiple assignment");
                        }
                    }
                    else {
                        throw new InternalErrorException($"{LeftVariable.Token.Location}: Assignment value should be an instance, but got {Right.GetType().Name}");
                    }
                }
                else {
                    throw new RuntimeException($"{MultipleAssignmentExpression.Left[i].Location}: {Left.GetType()} cannot be the target of an assignment");
                }
            }
            return new ArrayInstance(Api.Array, AssignedValues);
        }
        async Task<Instance> InterpretUndefineMethodStatement(UndefineMethodStatement UndefineMethodStatement) {
            string MethodName = UndefineMethodStatement.MethodName.Token.Value!;
            if (MethodName == "initialize") {
                await Warn($"{UndefineMethodStatement.MethodName.Token.Location}: Undefining 'initialize' may cause problems");
            }
            if (!CurrentModule.InstanceMethods.Remove(MethodName)) {
                throw new RuntimeException($"{UndefineMethodStatement.MethodName.Token.Location}: Undefined method '{MethodName}' for {CurrentModule.Name}");
            }
            return Api.Nil;
        }
        async Task<Instance> InterpretDefinedExpression(DefinedExpression DefinedExpression) {
            if (DefinedExpression.Expression is MethodCallExpression DefinedMethod) {
                try {
                    await InterpretExpressionAsync(DefinedMethod.MethodPath, ReturnType.FoundVariable);
                }
                catch (RuntimeException) {
                    return Api.Nil;
                }
                return new StringInstance(Api.String, "method");
            }
            if (DefinedExpression.Expression is PathExpression DefinedPath) {
                try {
                    await InterpretExpressionAsync(DefinedPath, ReturnType.FoundVariable);
                }
                catch (RuntimeException) {
                    return Api.Nil;
                }
                return new StringInstance(Api.String, "method");
            }
            else if (DefinedExpression.Expression is ObjectTokenExpression ObjectToken) {
                if (ObjectToken.Token.Type == Phase2TokenType.LocalVariableOrMethod) {
                    if (TryGetLocalVariable(ObjectToken.Token.Value!, out _)) {
                        return new StringInstance(Api.String, "local-variable");
                    }
                    else if (TryGetLocalInstanceMethod(ObjectToken.Token.Value!, out _, out _)) {
                        return new StringInstance(Api.String, "method");
                    }
                    else {
                        return Api.Nil;
                    }
                }
                else if (ObjectToken.Token.Type == Phase2TokenType.GlobalVariable) {
                    if (Interpreter.GlobalVariables.ContainsKey(ObjectToken.Token.Value!)) {
                        return new StringInstance(Api.String, "global-variable");
                    }
                    else {
                        return Api.Nil;
                    }
                }
                else if (ObjectToken.Token.Type == Phase2TokenType.ConstantOrMethod) {
                    if (TryGetLocalConstant(ObjectToken.Token.Value!, out _)) {
                        return new StringInstance(Api.String, "constant");
                    }
                    else if (TryGetLocalInstanceMethod(ObjectToken.Token.Value!, out _, out _)) {
                        return new StringInstance(Api.String, "method");
                    }
                    else {
                        return Api.Nil;
                    }
                }
                else if (ObjectToken.Token.Type == Phase2TokenType.InstanceVariable) {
                    if (CurrentInstance.InstanceVariables.ContainsKey(ObjectToken.Token.Value!)) {
                        return new StringInstance(Api.String, "instance-variable");
                    }
                    else {
                        return Api.Nil;
                    }
                }
                else if (ObjectToken.Token.Type == Phase2TokenType.ClassVariable) {
                    if (CurrentModule.ClassVariables.ContainsKey(ObjectToken.Token.Value!)) {
                        return new StringInstance(Api.String, "class-variable");
                    }
                    else {
                        return Api.Nil;
                    }
                }
                else {
                    return new StringInstance(Api.String, "expression");
                }
            }
            else if (DefinedExpression.Expression is SelfExpression) {
                return new StringInstance(Api.String, "self");
            }
            else if (DefinedExpression.Expression is SuperExpression) {
                return new StringInstance(Api.String, "super");
            }
            else {
                return new StringInstance(Api.String, "expression");
            }
        }
        async Task<Instance> InterpretHashArgumentsExpression(HashArgumentsExpression HashArgumentsExpression) {
            return new HashArgumentsInstance(
                await InterpretHashExpression(HashArgumentsExpression.HashExpression),
                Interpreter
            );
        }
        async Task<Instance> InterpretEnvironmentInfoExpression(EnvironmentInfoExpression EnvironmentInfoExpression) {
            return EnvironmentInfoExpression.Type switch {
                EnvironmentInfoType.__LINE__ => new IntegerInstance(Api.Integer, EnvironmentInfoExpression.Location.Line),
                EnvironmentInfoType.__FILE__ => new StringInstance(Api.String, System.IO.Path.GetFileName(new System.Diagnostics.StackTrace(true).GetFrame(0)?.GetFileName() ?? "")),
                _ => throw new InternalErrorException($"{ApproximateLocation}: Environment info type not handled: '{EnvironmentInfoExpression.Type}'"),
            };
        }

        public enum AccessModifier {
            Public,
            Private,
            Protected,
        }
        public enum BreakHandleType {
            Invalid,
            Rethrow,
            Destroy
        }
        public enum ReturnType {
            InterpretResult,
            FoundVariable,
            HypotheticalVariable
        }
        internal async Task<Instance> InterpretExpressionAsync(Expression Expression, ReturnType ReturnType = ReturnType.InterpretResult) {
            // Set approximate location
            ApproximateLocation = Expression.Location;

            // Stop script
            if (Stopping)
                return new StopReturnCode(false, Interpreter);

            // Interpret expression
            return Expression switch {
                MethodCallExpression MethodCallExpression => await InterpretMethodCallExpression(MethodCallExpression),
                ObjectTokenExpression ObjectTokenExpression => await InterpretObjectTokenExpression(ObjectTokenExpression, ReturnType),
                IfExpression IfExpression => await InterpretIfExpression(IfExpression),
                WhileExpression WhileExpression => await InterpretWhileExpression(WhileExpression),
                RescueExpression RescueExpression => await InterpretRescueExpression(RescueExpression),
                TernaryExpression TernaryExpression => await InterpretTernaryExpression(TernaryExpression),
                CaseExpression CaseExpression => await InterpretCaseExpression(CaseExpression),
                ArrayExpression ArrayExpression => await InterpretArrayExpression(ArrayExpression),
                HashExpression HashExpression => await InterpretHashExpression(HashExpression),
                WhileStatement WhileStatement => await InterpretWhileStatement(WhileStatement),
                ForStatement ForStatement => await InterpretForStatement(ForStatement),
                SelfExpression => CurrentInstance,
                LogicalExpression LogicalExpression => await InterpretLogicalExpression(LogicalExpression),
                NotExpression NotExpression => await InterpretNotExpression(NotExpression),
                DefineMethodStatement DefineMethodStatement => await InterpretDefineMethodStatement(DefineMethodStatement),
                DefineClassStatement DefineClassStatement => await InterpretDefineClassStatement(DefineClassStatement),
                ReturnStatement ReturnStatement => new ReturnReturnCode(ReturnStatement.ReturnValue != null
                                                        ? await InterpretExpressionAsync(ReturnStatement.ReturnValue) : Api.Nil,
                                                        Interpreter),
                LoopControlStatement LoopControlStatement => LoopControlStatement.Type switch {
                    LoopControlType.Break => new LoopControlReturnCode(LoopControlType.Break, Interpreter),
                    LoopControlType.Retry => new LoopControlReturnCode(LoopControlType.Retry, Interpreter),
                    LoopControlType.Redo => new LoopControlReturnCode(LoopControlType.Redo, Interpreter),
                    LoopControlType.Next => new LoopControlReturnCode(LoopControlType.Next, Interpreter),
                    _ => throw new InternalErrorException($"{Expression.Location}: Loop control type not handled: '{LoopControlStatement.Type}'")},
                YieldExpression YieldExpression => await InterpretYieldExpression(YieldExpression),
                SuperExpression SuperExpression => await InterpretSuperExpression(SuperExpression),
                AliasStatement AliasStatement => await InterpretAliasStatement(AliasStatement),
                RangeExpression RangeExpression => await InterpretRangeExpression(RangeExpression),
                IfBranchesStatement IfBranchesStatement => await InterpretIfBranchesStatement(IfBranchesStatement),
                BeginBranchesStatement BeginBranchesStatement => await InterpretBeginBranchesStatement(BeginBranchesStatement),
                AssignmentExpression AssignmentExpression => await InterpretAssignmentExpression(AssignmentExpression, ReturnType),
                MultipleAssignmentExpression MultipleAssignmentExpression => await InterpretMultipleAssignmentExpression(MultipleAssignmentExpression, ReturnType),
                UndefineMethodStatement UndefineMethodStatement => await InterpretUndefineMethodStatement(UndefineMethodStatement),
                DefinedExpression DefinedExpression => await InterpretDefinedExpression(DefinedExpression),
                HashArgumentsExpression HashArgumentsExpression => await InterpretHashArgumentsExpression(HashArgumentsExpression),
                EnvironmentInfoExpression EnvironmentInfoExpression => await InterpretEnvironmentInfoExpression(EnvironmentInfoExpression),
                _ => throw new InternalErrorException($"{Expression.Location}: Not sure how to interpret expression {Expression.GetType().Name} ({Expression.Inspect()})"),
            };
        }
        internal async Task<Instance> InternalInterpretAsync(List<Expression> Statements) {
            // Interpret statements
            Instance LastInstance = Api.Nil;
            foreach (Expression Statement in Statements) {
                LastInstance = await InterpretExpressionAsync(Statement);
                if (LastInstance is ReturnCodeInstance) {
                    break;
                }
            }
            // Return last instance
            return LastInstance;
        }
        internal async Task<Instance> InternalEvaluateAsync(string Code) {
            // Get statements from code
            List<Phase1.Phase1Token> Tokens = Phase1.GetPhase1Tokens(Code);
            List<Expression> Statements = ObjectsToExpressions(Tokens, ExpressionsType.Statements);

            // Interpret statements
            return await InternalInterpretAsync(Statements);
        }

        public async Task<Instance> InterpretAsync(List<Expression> Statements) {
            // Interpret statements and store the result
            Instance LastInstance = await InternalInterpretAsync(Statements);
            if (LastInstance is LoopControlReturnCode LoopControlReturnCode) {
                throw new SyntaxErrorException($"{ApproximateLocation}: Invalid {LoopControlReturnCode.Type} (must be in a loop)");
            }
            else if (LastInstance is ReturnReturnCode ReturnReturnCode) {
                return ReturnReturnCode.ReturnValue;
            }
            else if (LastInstance is StopReturnCode) {
                return Api.Nil;
            }
            else if (LastInstance is ThrowReturnCode ThrowReturnCode) {
                throw new RuntimeException($"{ApproximateLocation}: uncaught throw {ThrowReturnCode.Identifier.Inspect()}");
            }
            else if (LastInstance is ReturnCodeInstance) {
                throw new RuntimeException($"{ApproximateLocation}: Invalid {LastInstance.GetType().Name}");
            }
            return LastInstance;
        }
        public Instance Interpret(List<Expression> Statements) {
            return InterpretAsync(Statements).Result;
        }
        public async Task<Instance> EvaluateAsync(string Code) {
            List<Phase1.Phase1Token> Tokens = Phase1.GetPhase1Tokens(Code);
            List<Expression> Statements = ObjectsToExpressions(Tokens, ExpressionsType.Statements);

            /*Console.WriteLine(Statements.Inspect("\n"));
            Console.Write("Press enter to continue.");
            Console.ReadLine();*/

            return await InterpretAsync(Statements);
        }
        public Instance Evaluate(string Code) {
            return EvaluateAsync(Code).Result;
        }
        public async Task WaitForThreadsAsync() {
            HashSet<ScopeThread> CurrentThreads = new(Threads);
            foreach (ScopeThread Thread in CurrentThreads) {
                if (Thread.Running != null && !Thread.Running.IsCompleted) {
                    await Thread.Running;
                }
            }
        }
        public void WaitForThreads() {
            WaitForThreadsAsync().Wait();
        }
        /// <summary>Stops the script, including all running threads.</summary>
        public void Stop() {
            Stopping = true;
        }

        /// <summary>Interop between C# and Ruby.</summary>
        public object? this[string Key] {
            get {
                if (string.IsNullOrEmpty(Key)) throw new ApiException("Scope key must not be empty");
                Dictionary<string, Instance> Dict = Key.IsConstantIdentifier() ? Constants : LocalVariables;
                return Dict[Key].Object;
            }
            set {
                if (string.IsNullOrEmpty(Key)) throw new ApiException("Scope key must not be empty");

                static Instance ObjectToInstance(Api Api, object? Object) {
                    return Object switch {
                        null => Api.Nil,
                        true => Api.True,
                        false => Api.False,
                        int ValueInt => new IntegerInstance(Api.Integer, ValueInt),
                        long ValueLong => new IntegerInstance(Api.Integer, ValueLong),
                        BigInteger ValueBigInteger => new IntegerInstance(Api.Integer, ValueBigInteger),
                        DynInteger ValueDynInteger => new IntegerInstance(Api.Integer, ValueDynInteger),
                        float ValueFloat => new FloatInstance(Api.Float, ValueFloat),
                        double ValueDouble => new FloatInstance(Api.Float, ValueDouble),
                        decimal ValueDecimal => new FloatInstance(Api.Float, (double)ValueDecimal),
                        string ValueString => new StringInstance(Api.String, ValueString),
                        Range ValueRange => new RangeInstance(Api.Range, new IntegerInstance(Api.Integer, ValueRange.Start.IsFromEnd ? -ValueRange.Start.Value : ValueRange.Start.Value), new IntegerInstance(Api.Integer, ValueRange.End.IsFromEnd ? -ValueRange.End.Value : ValueRange.End.Value), true),
                        Delegate ValueDelegate => new ProcInstance(Api.Proc, DelegateToMethod(Api, ValueDelegate)),
                        _ => throw new ApiException($"Scope value type not valid: '{Object.GetType()}'"),
                    };
                }
                static object? LenientlyConvertType(object? Object, Type TargetType) {
                    if (Object is DynInteger DynInteger) {
                        if (TargetType == typeof(int)) return (int)(long)DynInteger;
                        else if (TargetType == typeof(uint)) return (uint)(long)DynInteger;
                        else if (TargetType == typeof(short)) return (short)(long)DynInteger;
                        else if (TargetType == typeof(ushort)) return (ushort)(long)DynInteger;
                        else if (TargetType == typeof(byte)) return (byte)(long)DynInteger;
                        else if (TargetType == typeof(sbyte)) return (sbyte)(long)DynInteger;
                    }
                    else if (Object is DynFloat DynFloat) {
                        if (TargetType == typeof(float)) return (float)(double)DynFloat;
                        else if (TargetType == typeof(decimal)) return (decimal)(double)DynFloat;
                    }
                    return Object;
                }
                static Method DelegateToMethod(Api Api, Delegate Delegate) {
                    // Get argument count
                    System.Reflection.ParameterInfo[] Parameters = Delegate.Method.GetParameters();
                    int RequiredArgumentCount = Parameters.Count(Param => !Param.HasDefaultValue);
                    int TotalArgumentCount = Parameters.Length;
                    // Create method
                    return new Method(
                        async Input => {
                            object?[] Arguments = new object?[TotalArgumentCount];
                            for (int i = 0; i < Arguments.Length; i++) {
                                // Set argument
                                Arguments[i] = i < Input.Arguments.Count
                                    ? Input.Arguments[i].Object
                                    : Parameters[i].DefaultValue;
                                // Convert argument if necessary
                                Arguments[i] = LenientlyConvertType(Arguments[i], Parameters[i].ParameterType);
                            }
                            return ObjectToInstance(Api, Delegate.DynamicInvoke(Arguments));
                        },
                        new IntRange(RequiredArgumentCount, TotalArgumentCount)
                    );
                }

                Dictionary<string, Instance> Dict = Key.IsConstantIdentifier() ? Constants : LocalVariables;
                // Convert C# instance to Ruby instance
                Dict[Key] = ObjectToInstance(Api, value);
            }
        }
        
        public Scope(Scope? parentScope, bool AllowUnsafeApi) : base(parentScope?.Interpreter!) {
            this.AllowUnsafeApi = AllowUnsafeApi;
            // Root scope
            if (parentScope == null) {
                this.AllowUnsafeApi = AllowUnsafeApi;
                SetInterpreter(new Interpreter(this));
            }
            // Child scope
            else {
                ParentScope = parentScope;
                CurrentAccessModifier = parentScope.CurrentAccessModifier;
            }
        }
        public Scope(Scope? parentScope = null, Method? OnYield = null) : base(parentScope?.Interpreter!) {
            CurrentOnYield = OnYield;
            // Root scope
            if (parentScope == null) {
                AllowUnsafeApi = true;
                SetInterpreter(new Interpreter(this));
            }
            // Child scope
            else {
                AllowUnsafeApi = parentScope.AllowUnsafeApi;
                ParentScope = parentScope;
                CurrentAccessModifier = parentScope.CurrentAccessModifier;
            }
        }
    }
    public class Module {
        public readonly string Name;
        public readonly LockingDictionary<string, Method> Methods = new();
        public readonly LockingDictionary<string, Method> InstanceMethods = new();
        public readonly LockingDictionary<string, Instance> InstanceVariables = new();
        public readonly LockingDictionary<string, Instance> ClassVariables = new();
        public readonly LockingDictionary<string, Instance> Constants = new();
        public readonly Module? Parent;
        public readonly Module? SuperModule;
        public readonly Interpreter Interpreter;
        public Module(string name, Module parent, Module? superModule) {
            Name = name;
            Parent = parent;
            SuperModule = superModule;
            Interpreter = parent.Interpreter;
        }
        public Module(string name, Interpreter interpreter, Module? superModule) {
            Name = name;
            SuperModule = superModule;
            Interpreter = interpreter;
        }
        public bool InheritsFrom(Module? Ancestor) {
            if (Ancestor == null) return false;
            Module? CurrentAncestor = this;
            while (CurrentAncestor != null) {
                if (CurrentAncestor == Ancestor)
                    return true;
                CurrentAncestor = CurrentAncestor.SuperModule;
            }
            return false;
        }
        public bool TryGetMethod(string MethodName, out Method? Method) {
            Method = TryGetMethod(Module => Module.Methods, MethodName);
            return Method != null;
        }
        public async Task<Instance> CallMethod(Scope Scope, string MethodName, Instances? Arguments = null, Method? OnYield = null) {
            return await CallMethod(Module => Module.Methods, Scope, new ModuleReference(this), MethodName, Arguments, OnYield);
        }
        public bool TryGetInstanceMethod(string MethodName, out Method? Method) {
            Method = TryGetMethod(Module => Module.InstanceMethods, MethodName);
            return Method != null;
        }
        public async Task<Instance> CallInstanceMethod(Scope Scope, Instance OnInstance, string MethodName, Instances? Arguments = null, Method? OnYield = null) {
            return await CallMethod(Module => Module.InstanceMethods, Scope, OnInstance, MethodName, Arguments, OnYield);
        }
        private Method? TryGetMethod(Func<Module, LockingDictionary<string, Method>> MethodsDict, string MethodName) {
            Module? CurrentSuperModule = this;
            while (true) {
                if (MethodsDict(CurrentSuperModule).TryFindMethod(MethodName, out Method? FindMethod)) return FindMethod;
                CurrentSuperModule = CurrentSuperModule.SuperModule;
                if (CurrentSuperModule == null) return null;
            }
        }
        private async Task<Instance> CallMethod(Func<Module, LockingDictionary<string, Method>> MethodsDict, Scope Scope, Instance OnInstance, string MethodName, Instances? Arguments = null, Method? OnYield = null) {
            Method? Method = TryGetMethod(MethodsDict, MethodName);
            if (Method == null) {
                throw new RuntimeException($"{Scope.ApproximateLocation}: Undefined method '{MethodName}' for {Name}");
            }
            else if (Method.Name == "method_missing") {
                // Get arguments (method_name, *args)
                Instances MethodMissingArguments;
                if (Arguments != null) {
                    List<Instance> GivenArguments = new(Arguments.MultiInstance);
                    GivenArguments.Insert(0, Scope.Api.GetSymbol(MethodName));
                    MethodMissingArguments = new Instances(GivenArguments);
                }
                else {
                    MethodMissingArguments = Scope.Api.GetSymbol(MethodName);
                }
                // Call method_missing
                return await Method.Call(Scope, OnInstance, MethodMissingArguments, OnYield);
            }
            else {
                return await Method.Call(Scope, OnInstance, Arguments, OnYield);
            }
        }
    }
    public class Class : Module {
        public Class(string name, Module parent, Module? superModule) : base(name, parent, superModule) {
            Setup();
        }
        public Class(string name, Interpreter interpreter, Module? superModule) : base(name, interpreter, superModule) {
            Setup();
        }
        void Setup() {
            // Default method: new
            if (!TryGetMethod("new", out _)) {
                Methods["new"] = new Method(async Input => {
                    Class Class = (Class)Input.Instance.Module!;
                    Instance NewInstance = Input.Api.CreateInstanceFromClass(Input.Scope, Class);
                    Scope NewInstanceScope = new(Input.Scope) { CurrentModule = Class, CurrentInstance = NewInstance };
                    await NewInstance.CallInstanceMethod(NewInstanceScope, "initialize", Input.Arguments, Input.OnYield);
                    return NewInstance;
                }, null);
            }
            // Default method: initialize
            if (!TryGetInstanceMethod("initialize", out _)) {
                InstanceMethods["initialize"] = new Method(async Input => {
                    return Input.Api.Nil;
                }, 0);
            }
        }
    }
}
