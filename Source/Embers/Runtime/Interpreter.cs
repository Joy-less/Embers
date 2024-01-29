using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

#pragma warning disable IDE0060

namespace Embers {
    internal static class Interpreter {
        public static Instance InterpretIdentifier(Context Context, IdentifierExpression Expression) {
            // Check for constant
            if (Expression.PossibleConstant) {
                if (Context.Module.GetConstant(Expression.Name) is Instance Value) {
                    return Value;
                }
            }
            // Check for variable
            else {
                if (Context.Scope.GetVariable(Expression.Name) is Instance Value) {
                    return Value;
                }
            }
            // Call method
            return Context.Instance.CallMethod(new Context(Context.Locals, Expression.Location, Context.Scope, Context.Module, Context.Instance), Expression.Name);
        }
        public static Instance InterpretLocal(Context Context, LocalExpression Expression) {
            return Context.Scope.GetVariable(Expression.Name)
                ?? throw new RuntimeError($"{Expression.Location}: undefined local variable '{Expression.Name}' for {Context.Instance.Describe()}");
        }
        public static Instance InterpretConstant(Context Context, ConstantExpression Expression) {
            return Context.Module.GetConstant(Expression.Name)
                ?? throw new RuntimeError($"{Expression.Location}: undefined constant '{Expression.Name}' for {Context.Instance.Describe()}");
        }
        public static Instance InterpretGlobal(Context Context, GlobalExpression Expression) {
            return Context.Axis.Globals.GlobalVariables.GetValueOrDefault(Expression.Name)
                ?? Context.Axis.Nil;
        }
        public static Instance InterpretClassVariable(Context Context, ClassVariableExpression Expression) {
            return Context.Module.GetClassVariable(Expression.Name)
                ?? throw new RuntimeError($"{Expression.Location}: undefined class variable '{Expression.Name}' for {Context.Instance.Describe()}");
        }
        public static Instance InterpretInstanceVariable(Context Context, InstanceVariableExpression Expression) {
            return (Context.Module as Class)?.GetInstanceVariable(Expression.Name)
                ?? Context.Axis.Nil;
        }
        public static Instance InterpretConstantPath(Context Context, ConstantPathExpression Expression) {
            // Find constant
            Instance Parent = Expression.Parent.Interpret(Context);
            // Get constant
            if (Parent is Module Module) {
                return Module.GetConstant(Expression.Name)
                    ?? throw new RuntimeError($"{Expression.Location}: undefined constant '{Expression.Name}' for {Module}");
            }
            else {
                throw new RuntimeError($"{Expression.Location}: {Parent.Inspect()} is not a class/module");
            }
        }
        public static Instance InterpretMethodCall(Context Context, MethodCallExpression Expression) {
            // Get method parent
            Instance MethodParent = Expression.Parent is not null
                ? Expression.Parent.Interpret(Context)
                : Context.Instance;

            // Get given arguments
            Instance[] Arguments = new Instance[Expression.Arguments.Length];
            for (int i = 0; i < Expression.Arguments.Length; i++) {
                Arguments[i] = Expression.Arguments[i].Interpret(Context);
            }

            // Get block
            Proc? Block = Expression.Block is not null
                ? new Proc(Context.Scope, Context.Instance, Expression.Block)
                : null;

            // Call method
            return MethodParent.CallMethod(
                new Context(Context.Locals, Expression.Location, Context.Scope, Context.Module, Context.Instance, Block),
                Expression.Name,
                Arguments
            );
        }
        public static Instance InterpretTokenLiteral(Context Context, TokenLiteralExpression Expression) {
            return Expression.CreateLiteral();
        }
        public static Instance InterpretFormattedString(Context Context, FormattedStringExpression Expression) {
            // Create string builder
            StringBuilder FormattedString = new(Expression.Components.Length);
            // Build each component
            foreach (object Component in Expression.Components) {
                // Build literal or expression.to_s
                FormattedString.Append(Component as string ?? ((Expression)Component).Interpret(Context).ToS());
            }
            // Produce string
            return new Instance(Context.Axis.String, FormattedString.ToString());
        }
        public static Instance InterpretSelf(Context Context, SelfExpression Expression) {
            return Context.Instance;
        }
        public static Instance InterpretLine(Context Context, LineExpression Expression) {
            return new Instance(Context.Axis.Integer, (Integer)Expression.Location.Line);
        }
        public static Instance InterpretFile(Context Context, FileExpression Expression) {
            return new Instance(Context.Axis.String, Process.GetCurrentProcess().MainModule?.ModuleName ?? "unknown");
        }
        public static Instance InterpretBlockGiven(Context Context, BlockGivenExpression Expression) {
            return Context.Block is not null ? Context.Axis.True : Context.Axis.False;
        }
        public static Instance InterpretLocalAssignment(Context Context, LocalExpression Expression, Instance Value) {
            // a = b
            return Context.Scope.SetVariable(Expression.Name, Value);
        }
        public static Instance InterpretConstantAssignment(Context Context, ConstantExpression Expression, Instance Value) {
            // Warn if constant already defined
            if (Context.Module.GetConstant(Expression.Name) is not null) {
                // Warn about constant reassignment
                Context.Axis.Warn(Expression.Location, $"constant '{Expression.Name}' has already been assigned");
            }
            // A = b
            return Context.Module.SetConstant(Expression.Name, Value);
        }
        public static Instance InterpretGlobalAssignment(Context Context, GlobalExpression Expression, Instance Value) {
            // $a = b
            return Context.Axis.Globals.GlobalVariables[Expression.Name] = Value;
        }
        public static Instance InterpretClassVariableAssignment(Context Context, ClassVariableExpression Expression, Instance Value) {
            // @@a = b
            return Context.Module.SetClassVariable(Expression.Name, Value);
        }
        public static Instance InterpretInstanceVariableAssignment(Context Context, InstanceVariableExpression Expression, Instance Value) {
            // @a = b
            return Context.Module.CastClass.SetInstanceVariable(Expression.Name, Value);
        }
        public static Instance InterpretConstantPathAssignment(Context Context, ConstantPathExpression Expression, Instance Value) {
            // Get constant parent
            Module Parent = Expression.Parent is not null
                ? Expression.Parent.Interpret(Context) as Module ?? throw new RuntimeError($"{Expression.Location}: constant parent must be a class/module")
                : Context.Module;
            // Warn if constant already defined
            if (Parent.GetConstant(Expression.Name) is not null) {
                // Warn about constant reassignment
                Context.Axis.Warn(Expression.Location, $"constant '{Expression.Name}' has already been assigned");
            }
            // A = b
            return Parent.SetConstant(Expression.Name, Value);
        }
        public static Instance InterpretMethodCallAssignment(Context Context, MethodCallExpression Expression, Instance Value) {
            // Get method parent
            Instance MethodParent = Expression.Parent is not null
                ? Expression.Parent.Interpret(Context)
                : Context.Instance;
            // a.b = c
            return MethodParent.CallMethod($"{Expression.Name}=", Value);
        }
        public static Instance InterpretAssignment(Context Context, AssignmentExpression Expression) {
            // Interpret value
            Instance Value = Expression.Value.Interpret(Context);
            // Assign value
            return Expression.Target.Assign(Context, Value);
        }
        public static Instance InterpretMultiAssignment(Context Context, MultiAssignmentExpression Expression) {
            // Create array to store values
            Array Values = new(Context.Location, Expression.Assignments.Length);
            // Interpret each assignment
            foreach (Expression Assignment in Expression.Assignments) {
                Values.Add(Assignment.Interpret(Context));
            }
            // Return values array
            return new Instance(Context.Axis.Array, Values);
        }
        public static Instance InterpretExpandAssignment(Context Context, ExpandAssignmentExpression Expression) {
            // Interpret value
            Instance Value = Expression.Value.Interpret(Context);
            // Assign each index of the value
            for (int i = 0; i < Expression.Targets.Length; i++) {
                Expression.Targets[i].Assign(Context, Value.CallMethod(Context, "[]", new Instance(Context.Axis.Integer, (Integer)i)));
            }
            return Context.Axis.Nil;
        }
        public static Instance InterpretNot(Context Context, NotExpression Expression) {
            return Expression.Expression.Interpret(Context).Truthy
                ? Context.Axis.False
                : Context.Axis.True;
        }
        public static Instance InterpretTernary(Context Context, TernaryExpression Expression) {
            // Get result expression
            Expression ResultExpression = Expression.Condition.Interpret(Context).Truthy ? Expression.ExpressionIfTruthy : Expression.ExpressionIfFalsey;
            // Interpret result expression
            return ResultExpression.Interpret(Context);
        }
        public static Instance InterpretLogic(Context Context, LogicExpression Expression) {
            Instance Left = Expression.Left.Interpret(Context);
            if (Left.Truthy != Expression.IsAnd) {
                return Left;
            }
            return Expression.Right.Interpret(Context);
        }
        public static Instance InterpretIfModifier(Context Context, IfModifierExpression Expression) {
            // Condition is true
            if (Expression.Condition.Interpret(Context).Truthy) {
                // Interpret expression
                return Expression.Expression.Interpret(Context);
            }
            // Condition is false
            else {
                return Context.Axis.Nil;
            }
        }
        public static Instance InterpretWhileModifier(Context Context, WhileModifierExpression Expression) {
            Instance LastInstance = Context.Axis.Nil;
            while (Expression.Condition.Interpret(Context).Truthy) {
                // Interpret expression
                LastInstance = Expression.Expression.Interpret(Context);
                // Control code
                if (LastInstance is ControlCode ControlCode) {
                    switch (ControlCode.Type) {
                        // Break
                        case ControlType.Break:
                            return Context.Axis.Nil;
                        // Next, Redo, Retry
                        case ControlType.Next or ControlType.Redo or ControlType.Retry:
                            continue;
                        // Return
                        case ControlType.Return:
                            return ControlCode;
                    }
                }
            }
            return LastInstance;
        }
        public static Instance InterpretRescueModifier(Context Context, RescueModifierExpression Expression) {
            try {
                // Interpret expression
                return Expression.Expression.Interpret(Context);
            }
            catch (Exception) {
                // Interpret rescue
                return Expression.Rescue.Interpret(Context);
            }
        }
        public static Instance InterpretArray(Context Context, ArrayExpression Expression) {
            // Create array with items
            Array Array = new(Expression.Location, Expression.Items.Length);
            foreach (Expression Item in Expression.Items) {
                Array.Add(Item.Interpret(Context));
            }
            return new Instance(Context.Axis.Array, Array);
        }
        public static Instance InterpretHash(Context Context, HashExpression Expression) {
            // Create hash with key-value pairs
            Hash Hash = new(Expression.Location, Expression.Items.Count);
            foreach (KeyValuePair<Expression, Expression> Pair in Expression.Items) {
                Hash[Pair.Key.Interpret(Context)] = Pair.Value.Interpret(Context);
            }
            return new Instance(Context.Axis.Hash, Hash);
        }
        public static Instance InterpretKeyValuePair(Context Context, KeyValuePairExpression Expression) {
            // Create hash with single key-value pair
            Hash Hash = new(Expression.Location, 1);
            Hash[Expression.Key.Interpret(Context)] = Expression.Value.Interpret(Context);
            return new Instance(Context.Axis.Hash, Hash);
        }
        public static Instance InterpretRange(Context Context, RangeExpression Expression) {
            // Create range
            InstanceRange Range = new(Expression.Location,
                Expression.Min?.Interpret(Context) ?? Context.Axis.Nil,
                Expression.Max?.Interpret(Context) ?? Context.Axis.Nil,
                Expression.ExcludeEnd
            );
            return new Instance(Context.Axis.Range, Range);
        }
        public static Instance InterpretYield(Context Context, YieldExpression Expression) {
            // Get given arguments
            Instance[] Arguments = new Instance[Expression.Arguments.Length];
            for (int i = 0; i < Expression.Arguments.Length; i++) {
                Arguments[i] = Expression.Arguments[i].Interpret(Context);
            }

            // Call block with arguments
            if (Context.Block is not null) {
                return Context.Block.Call(Arguments);
            }
            // No block given
            else {
                throw new RuntimeError($"{Expression.Location}: no block given for yield");
            }
        }
        public static Instance InterpretSuper(Context Context, SuperExpression Expression) {
            // Get given arguments
            Instance[] Arguments = new Instance[Expression.Arguments.Length];
            for (int i = 0; i < Expression.Arguments.Length; i++) {
                Arguments[i] = Expression.Arguments[i].Interpret(Context);
            }

            // Get super method name
            string? MethodName = Context.Method?.Name
                ?? throw new RuntimeError($"{Expression.Location}: super called outside of method");
            // Get super method
            Method Method = (Context.Instance is Module ? Context.Module.SuperClass?.GetClassMethod(MethodName) : Context.Module.SuperClass?.GetInstanceMethod(MethodName))
                ?? throw new RuntimeError($"{Expression.Location}: no super method '{MethodName}' for {Context.Instance}");

            // Call super method with arguments
            return Method.Call(Context, Arguments);
        }
        public static Instance InterpretAlias(Context Context, AliasExpression Expression) {
            // Get current class
            Class CurrentClass = Context.Module as Class
                ?? throw new RuntimeError($"{Expression.Location}: alias not valid in Module");

            // Find original method
            Method OriginalMethod = CurrentClass.GetInstanceMethod(Expression.Original)
                ?? throw new RuntimeError($"{Expression.Location}: undefined method '{Expression.Original}' for {CurrentClass.Describe()}");

            // Alias original method
            CurrentClass.SetInstanceMethod(Expression.Alias, OriginalMethod);

            // Return nil
            return Context.Axis.Nil;
        }
        public static Instance InterpretDefined(Context Context, DefinedExpression Expression) {
            // Identifier defined?
            static Instance IdentifierDefined(Context Context, IdentifierExpression Expression) {
                // Check for constant
                if (Expression.PossibleConstant) {
                    if (Context.Module.GetConstant(Expression.Name) is not null) {
                        return Context.Axis.Globals.GetImmortalSymbol("constant");
                    }
                }
                // Check for variable
                else {
                    if (Context.Scope.GetVariable(Expression.Name) is not null) {
                        return Context.Axis.Globals.GetImmortalSymbol("local-variable");
                    }
                }
                // Method defined?
                return Context.Instance.GetMethod(Expression.Name) is not null
                    ? Context.Axis.Globals.GetImmortalSymbol("method")
                    : Context.Axis.Nil;
            }
            // Class variable defined?
            static Instance ClassVariableDefined(Context Context, ClassVariableExpression Expression) {
                return Context.Module.GetClassVariable(Expression.Name) is not null
                    ? Context.Axis.Globals.GetImmortalSymbol("class-variable")
                    : Context.Axis.Nil;
            }
            // Instance variable defined?
            static Instance InstanceVariableDefined(Context Context, InstanceVariableExpression Expression) {
                return (Context.Module as Class)?.GetInstanceVariable(Expression.Name) is not null
                    ? Context.Axis.Globals.GetImmortalSymbol("instance-variable")
                    : Context.Axis.Nil;
            }
            // Method path defined?
            static Instance MethodCallDefined(Context Context, MethodCallExpression Expression) {
                // Interpret parent
                Instance Parent = Context.Instance;
                if (Expression.Parent is not null) {
                    if (Defined(Context, Expression.Parent).Falsey) {
                        return Context.Axis.Nil;
                    }
                    Parent = Expression.Parent.Interpret(Context);
                }
                // Method defined?
                return Parent.GetMethod(Expression.Name) is not null
                    ? Context.Axis.Globals.GetImmortalSymbol("method")
                    : Context.Axis.Nil;
            }
            // Yield defined?
            static Instance YieldDefined(Context Context) {
                return Context.Block is not null
                    ? Context.Axis.Globals.GetImmortalSymbol("yield")
                    : Context.Axis.Nil;
            }
            // Expression defined?
            static Instance Defined(Context Context, Expression Expression) {
                return Expression switch {
                    IdentifierExpression IdentifierExpression => IdentifierDefined(Context, IdentifierExpression),
                    InstanceVariableExpression InstanceVariableExpression => InstanceVariableDefined(Context, InstanceVariableExpression),
                    ClassVariableExpression ClassVariableExpression => ClassVariableDefined(Context, ClassVariableExpression),
                    MethodCallExpression MethodCallExpression => MethodCallDefined(Context, MethodCallExpression),
                    YieldExpression => YieldDefined(Context),
                    _ => Context.Axis.Globals.GetImmortalSymbol("expression"),
                };
            }
            // Defined?
            return Defined(Context, Expression.Argument);
        }
        public static Instance InterpretControl(Context Context, ControlExpression Expression) {
            // Interpret argument if given
            Instance? Argument = Expression.Argument?.Interpret(Context);
            // Throw control code
            return new ControlCode(Context.Axis, Expression.Location, Expression.Type, Argument);
        }
        public static Instance InterpretBegin(Context Context, BeginExpression Expression) {
            try {
                // Interpret begin expressions
                Instance Instance = Expression.Expressions.Interpret(Context);
                // Early exit upon control code
                if (Instance is ControlCode ControlCode) {
                    return ControlCode;
                }
                // Interpret else branch
                if (Expression.ElseBranch is not null) {
                    Instance = Expression.ElseBranch.Interpret(Context);
                }
                // Interpret ensure branch
                if (Expression.EnsureBranch is not null) {
                    Instance = Expression.EnsureBranch.Interpret(Context);
                }
                // Return result
                return Instance;
            }
            catch (Exception Ex) when (Ex is not (ThrowError or OperationCanceledException)) {
                // Get exception
                Instance Exception = Adapter.GetInstance(Context, Ex);
                // Try to rescue exception
                foreach (RescueExpression Rescue in Expression.RescueBranches) {
                    // Get rescue type
                    Class RescueClass = Context.Axis.Exception;
                    if (Rescue.ExceptionType is not null) {
                        RescueClass = Rescue.ExceptionType.Interpret(Context) as Class
                            ?? throw new RuntimeError($"{Rescue.ExceptionType.Location}: expected Class to rescue");
                    }
                    // Try to rescue
                    if (Exception.Class.DerivesFrom(RescueClass)) {
                        // Interpret rescue branch
                        return InterpretRescue(Context, Rescue, Exception);
                    }
                }
                // Interpret ensure branch
                Instance? Instance = Expression.EnsureBranch?.Interpret(Context);
                // Early exit upon control code
                if (Instance is ControlCode ControlCode) {
                    return ControlCode;
                }
                // Exception not caught
                throw;
            }
        }
        public static Instance InterpretRescue(Context Context, RescueExpression Expression, Instance Exception) {
            // Set rescue variable
            if (Expression.ExceptionVariable is not null) {
                Context.Scope.SetVariable(Expression.ExceptionVariable, Exception);
            }
            // Interpret rescue expressions
            return Expression.Expressions.Interpret(Context);
        }
        public static Instance InterpretIf(Context Context, IfExpression Expression) {
            // Condition is true
            if (Expression.Condition is null || Expression.Condition.Interpret(Context).Truthy) {
                // Evaluate true branch
                return Expression.Expressions.Interpret(Context);
            }
            // Condition is false
            else {
                // End of branches
                return Context.Axis.Nil;
            }
        }
        public static Instance InterpretIfElse(Context Context, IfElseExpression Expression) {
            // Condition is true
            if (Expression.Condition!.Interpret(Context).Truthy) {
                // Evaluate true branch
                return Expression.Expressions.Interpret(Context);
            }
            // Condition is false
            else {
                // Evaluate else branch
                return Expression.ElseBranch.Interpret(Context);
            }
        }
        public static Instance InterpretWhile(Context Context, WhileExpression Expression) {
            Instance LastInstance = Context.Axis.Nil;
            while (Expression.Condition.Interpret(Context).Truthy) {
                // Interpret each expression
                LastInstance = Expression.Expressions.Interpret(Context);
                // Control code
                if (LastInstance is ControlCode ControlCode) {
                    switch (ControlCode.Type) {
                        // Break
                        case ControlType.Break:
                            return Context.Axis.Nil;
                        // Next, Redo, Retry
                        case ControlType.Next or ControlType.Redo or ControlType.Retry:
                            continue;
                        // Return
                        case ControlType.Return:
                            return ControlCode;
                    }
                }
            }
            return LastInstance;
        }
        public static Instance InterpretDefMethod(Context Context, DefMethodExpression Expression) {
            // Get method parent
            Instance Parent = Expression.Parent is not null
                ? Expression.Parent.Interpret(Context)
                : Context.Instance;
            // Get method scope
            Scope MethodScope = Context.Instance.SelfOrClass.Scope;
            // Create method
            Method Method = new(MethodScope, Expression.Location, Expression.Name, Expression.Arguments, Expression.Expressions, Context.Locals.AccessModifier);
            // Define method
            if (Parent is Module ParentModule) {
                ParentModule.SetClassMethod(Expression.Name, Method);
            }
            else {
                Parent.Class.SetInstanceMethod(Expression.Name, Method);
            }
            // Return nil
            return Context.Axis.Nil;
        }
        public static Instance InterpretFor(Context Context, ForExpression Expression) {
            // Get target
            Instance Target = Expression.Target.Interpret(Context);

            // Get block
            Proc? Block = Expression.Block is not null
                ? new Proc(Context.Scope, Context.Instance, Expression.Block)
                : null;

            // Call method
            return Target.CallMethod(
                new Context(Context.Locals, Expression.Location, Context.Scope, Context.Module, Context.Instance, Block),
                "each"
            );
        }
        public static Instance InterpretDefModule(Context Context, DefModuleExpression Expression) {
            Module? Module;

            // Get super
            Class? Super = null;
            if (Expression.Super is not null) {
                Super = Expression.Super.Interpret(Context) as Class
                    ?? throw new RuntimeError($"{Expression.Location}: superclass must be a Class");
            }

            // Monkey patch
            if (Context.Module.GetConstant(Expression.Name) is Instance Constant) {
                // Get existing class/module
                Module = Constant as Module;
                if (Module is null || Module is Class != Expression.IsClass) {
                    throw new RuntimeError($"{Expression.Location}: {Expression.Name} is not a {(Expression.IsClass ? "class" : "module")}");
                }
                // Update superclass
                if (Super is not null) {
                    // Ensure existing superclass is not overwritten
                    if (Module.SuperClass is not null && Module.SuperClass != Super) {
                        throw new RuntimeError($"{Expression.Location}: superclass mismatch for {(Expression.IsClass ? "class" : "module")} {Expression.Name}");
                    }
                    // Set new superclass
                    Module.SuperClass = Super;
                }
            }
            // Create new
            else {
                // Class
                if (Expression.IsClass) {
                    // Create class
                    Class Class = new(Context.Module, Super, Expression.Name);
                    Module = Class;

                    // new
                    Instance New(Context Context, params Instance[] Arguments) {
                        Instance Instance = new(Class);
                        Instance.CallMethod(new Context(Context.Locals, Expression.Location, Context.Scope, Class, Class, Context.Block), "initialize", Arguments);
                        return Instance;
                    }
                    Class.SetClassMethod("new", New);

                    // initialize
                    void Initialize() {
                    }
                    Class.SetInstanceMethod("initialize", Initialize, AccessModifier.Private);
                }
                // Module
                else {
                    // Create module
                    Module = new Module(Context.Module, Expression.Name);
                }

                // Add class/module to parent module
                Context.Module.SetConstant(Expression.Name, Module);
            }

            // Create class context
            Context ClassContext = new(
                Expression.Location,
                new Scope(Context.Axis),
                Module,
                Module is Class ModuleAsClass ? new Instance(ModuleAsClass) : Module
            );
            // Interpret class expressions
            Expression.Expressions.Interpret(ClassContext);
            // Return nil
            return Context.Axis.Nil;
        }
        public static Instance InterpretCase(Context Context, CaseExpression Expression) {
            // Interpret subject
            Instance Subject = Expression.Subject.Interpret(Context);
            // Match when branches
            foreach (WhenExpression WhenExpression in Expression.WhenBranches) {
                // Interpret match expression
                Instance Match = WhenExpression.Match.Interpret(Context);
                // Try match
                if (Match.CallMethod(new Context(Context.Locals, Expression.Location, Context.Scope, Context.Module, Context.Instance), "===", Subject).Truthy) {
                    return WhenExpression.Expressions.Interpret(Context);
                }
            }
            // Match else branch
            if (Expression.ElseBranch is not null) {
                return Expression.ElseBranch.Interpret(Context);
            }
            // No matches; return nil
            return Context.Axis.Nil;
        }
        public static Instance InterpretWhen(Context Context, WhenExpression Expression) {
            return Expression.Expressions.Interpret(Context);
        }
    }
}
