using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Embers.Scope;
using static Embers.Phase2;

#nullable enable

namespace Embers
{
    public class Instance : RubyObject {
        /// <summary>Module will be null if instance is a PseudoInstance.</summary>
        public readonly Module? Module;
        public long ObjectId { get; private set; }
        public LockingDictionary<string, Instance> InstanceVariables { get; protected set; } = new();
        public LockingDictionary<string, Method> InstanceMethods { get; protected set; } = new();
        public bool IsTruthy => Object is not (null or false);
        public virtual object? Object { get { return null; } }
        public virtual bool Boolean { get { throw new RuntimeException("Instance is not a Boolean"); } }
        public virtual string String { get { throw new RuntimeException("Instance is not a String"); } }
        public virtual DynInteger Integer { get { throw new RuntimeException("Instance is not an Integer"); } }
        public virtual DynFloat Float { get { throw new RuntimeException("Instance is not a Float"); } }
        public virtual Method Proc { get { throw new RuntimeException("Instance is not a Proc"); } }
        public virtual ScopeThread? Thread { get { throw new RuntimeException("Instance is not a Thread"); } }
        public virtual IntegerRange Range { get { throw new RuntimeException("Instance is not a Range"); } }
        public virtual List<Instance> Array { get { throw new RuntimeException("Instance is not an Array"); } }
        public virtual HashDictionary Hash { get { throw new RuntimeException("Instance is not a Hash"); } }
        public virtual Exception Exception { get { throw new RuntimeException("Instance is not an Exception"); } }
        public virtual DateTimeOffset Time { get { throw new RuntimeException("Instance is not a Time"); } }
        public virtual WeakReference<Instance> WeakRef { get { throw new RuntimeException("Instance is not a WeakRef"); } }
        public virtual HttpResponseMessage HttpResponse { get { throw new RuntimeException("Instance is not a HttpResponse"); } }
        public virtual string Inspect() {
            return $"#<{Module?.Name}:0x{base.GetHashCode():x16}>";
        }
        public virtual string LightInspect() {
            return Inspect();
        }
        public Instance Clone(Interpreter Interpreter) {
            Instance Clone = (Instance)MemberwiseClone();
            Clone.ObjectId = Interpreter.NewObjectId();
            Clone.InstanceVariables = new();
            InstanceVariables.CopyTo(Clone.InstanceVariables);
            Clone.InstanceMethods = new();
            InstanceMethods.CopyTo(Clone.InstanceMethods);
            return Clone;
        }
        public static async Task<Instance> CreateFromToken(Scope Scope, Phase2Token Token) {
            if (Token.ProcessFormatting) {
                string String = Token.Value!;
                Stack<int> FormatPositions = new();
                char? LastChara = null;
                for (int i = 0; i < String.Length; i++) {
                    char Chara = String[i];

                    if (LastChara == '#' && Chara == '{') {
                        FormatPositions.Push(i - 1);
                    }
                    else if (Chara == '}') {
                        if (FormatPositions.TryPop(out int StartPosition)) {
                            string FirstHalf = String[..StartPosition];
                            string ToFormat = String[(StartPosition + 2)..i];
                            string SecondHalf = String[(i + 1)..];

                            string Formatted = (await Scope.InternalEvaluateAsync(ToFormat)).LightInspect();
                            String = FirstHalf + Formatted + SecondHalf;
                            i = FirstHalf.Length - 1;
                        }
                    }
                    LastChara = Chara;
                }
                return new StringInstance(Scope.Api.String, String);
            }

            return Token.Type switch {
                Phase2TokenType.Nil => Scope.Api.Nil,
                Phase2TokenType.True => Scope.Api.True,
                Phase2TokenType.False => Scope.Api.False,
                Phase2TokenType.String => new StringInstance(Scope.Api.String, Token.Value!),
                Phase2TokenType.Symbol => Scope.Api.GetSymbol(Token.Value!),
                Phase2TokenType.Integer => new IntegerInstance(Scope.Api.Integer, Token.ValueAsInteger),
                Phase2TokenType.Float => new FloatInstance(Scope.Api.Float, Token.ValueAsFloat),
                _ => throw new InternalErrorException($"{Token.Location}: Cannot create new object from token type {Token.Type}")
            };
        }
        public Instance(Module fromModule) : base(fromModule.Interpreter) {
            Module = fromModule;
            if (this is not PseudoInstance) {
                ObjectId = Module.Interpreter.NewObjectId();
            }
        }
        public Instance(Interpreter interpreter) : base(interpreter) {
            Module = null;
            if (this is not PseudoInstance) {
                ObjectId = interpreter.NewObjectId();
            }
        }
        public bool TryGetInstanceMethod(string MethodName, out Method? Method) {
            if (this is not PseudoInstance) {
                if (InstanceMethods.TryFindMethod(MethodName, out Method? FindMethod)) {
                    Method = FindMethod;
                    return true;
                }
            }
            return Module!.TryGetInstanceMethod(MethodName, out Method);
        }
        public async Task<Instance> CallInstanceMethod(Scope Scope, string MethodName, Instances? Arguments = null, Method? OnYield = null) {
            if (this is not PseudoInstance || !TryGetInstanceMethod(MethodName, out Method? Method)) {
                return await Module!.CallInstanceMethod(Scope, this, MethodName, Arguments, OnYield);
            }
            else if (Method!.Name == "method_missing") {
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
                return await Method.Call(Scope, this, MethodMissingArguments, OnYield);
            }
            else {
                return await Method.Call(Scope, this, Arguments, OnYield);
            }
        }
        public override string ToString() {
            return Inspect();
        }
        public override int GetHashCode() {
            return Inspect().GetHashCode();
        }
    }
    public abstract class PseudoInstance : Instance {
        public PseudoInstance(Module module) : base(module) { }
        public PseudoInstance(Interpreter interpreter) : base(interpreter) { }
    }
    public class VariableReference : PseudoInstance {
        public Instance? Instance;
        public Phase2Token Token;
        public bool IsLocalReference => Module == null && Instance == null;
        public override string Inspect() {
            return $"{(Module != null ? Module.GetType().Name : (Instance != null ? Instance.Inspect() : Token.Inspect()))} var ref in {Token.Inspect()}";
        }
        public VariableReference(Module module, Phase2Token token) : base(module) {
            Token = token;
        }
        public VariableReference(Instance instance, Phase2Token token) : base(instance.Module!.Interpreter) {
            Instance = instance;
            Token = token;
        }
        public VariableReference(Phase2Token token, Interpreter interpreter) : base(interpreter) {
            Token = token;
        }
    }
    public class ScopeReference : PseudoInstance {
        public Scope Scope;
        public override string Inspect() {
            return Scope.GetType().Name;
        }
        public ScopeReference(Scope scope, Interpreter interpreter) : base(interpreter) {
            Scope = scope;
        }
    }
    public class MethodReference : PseudoInstance {
        readonly Method Method;
        public override object? Object { get { return Method; } }
        public override string Inspect() {
            return Method.ToString()!;
        }
        public MethodReference(Method method, Interpreter interpreter) : base(interpreter) {
            Method = method;
        }
    }
    public abstract class ReturnCodeInstance : PseudoInstance {
        public bool CalledInYieldMethod;
        public ReturnCodeInstance(Interpreter interpreter) : base(interpreter) {}
    }
    public class LoopControlReturnCode : ReturnCodeInstance {
        public readonly LoopControlType Type;
        public LoopControlReturnCode(LoopControlType type, Interpreter interpreter) : base(interpreter) {
            Type = type;
        }
    }
    public class ReturnReturnCode : ReturnCodeInstance {
        public readonly Instance ReturnValue;
        public ReturnReturnCode(Instance returnValue, Interpreter interpreter) : base(interpreter) {
            ReturnValue = returnValue;
        }
    }
    public class StopReturnCode : ReturnCodeInstance {
        public readonly bool Manual;
        public StopReturnCode(bool manual, Interpreter interpreter) : base(interpreter) {
            Manual = manual;
        }
    }
    public class ThrowReturnCode : ReturnCodeInstance {
        public readonly Instance Identifier;
        public ThrowReturnCode(Instance identifier, Interpreter interpreter) : base(interpreter) {
            Identifier = identifier;
        }
    }
    public class NilInstance : Instance {
        public override string Inspect() {
            return "nil";
        }
        public override string LightInspect() {
            return "";
        }
        public NilInstance(Class fromClass) : base(fromClass) { }
    }
    public class TrueInstance : Instance {
        public override object? Object { get { return true; } }
        public override bool Boolean { get { return true; } }
        public override string Inspect() {
            return "true";
        }
        public TrueInstance(Class fromClass) : base(fromClass) { }
    }
    public class FalseInstance : Instance {
        public override object? Object { get { return false; } }
        public override bool Boolean { get { return false; } }
        public override string Inspect() {
            return "false";
        }
        public FalseInstance(Class fromClass) : base(fromClass) { }
    }
    public class StringInstance : Instance {
        string Value;
        public override object? Object { get { return Value; } }
        public override string String { get { return Value; } }
        public override string Inspect() {
            return '"' + Value.Replace("\n", "\\n").Replace("\r", "\\r") + '"';
        }
        public override string LightInspect() {
            return Value;
        }
        public StringInstance(Class fromClass, string value) : base(fromClass) {
            Value = value;
        }
        public void SetValue(string value) {
            Value = value;
        }
    }
    public class SymbolInstance : Instance {
        readonly string Value;
        readonly bool IsStringSymbol;
        public override object? Object { get { return Value; } }
        public override string String { get { return Value; } }
        public override string Inspect() {
            if (IsStringSymbol) {
                return ":\"" + Value.Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
            }
            else {
                return ":" + Value;
            }
        }
        public override string LightInspect() {
            return Value;
        }
        public SymbolInstance(Class fromClass, string value) : base(fromClass) {
            Value = value;
            IsStringSymbol = Value.Any("(){}[]<>=+-*/%.,;@#&|~^$".Contains) || Value.Any(char.IsWhiteSpace) || (Value.Length != 0 && Value[0].IsAsciiDigit()) || Value[..^1].Any("?!".Contains);
        }
        ~SymbolInstance() {
            Interpreter.Symbols.Dict.Remove(Value);
        }
    }
    public class IntegerInstance : Instance {
        readonly DynInteger Value;
        public override object? Object { get { return Value; } }
        public override DynInteger Integer { get { return Value; } }
        public override DynFloat Float { get { return Value; } }
        public override string Inspect() {
            return Value.ToString();
        }
        public IntegerInstance(Class fromClass, DynInteger value) : base(fromClass) {
            Value = value;
        }
    }
    public class FloatInstance : Instance {
        readonly DynFloat Value;
        public override object? Object { get { return Value; } }
        public override DynFloat Float { get { return Value; } }
        public override DynInteger Integer { get { return (DynInteger)Value; } }
        public override string Inspect() {
            if (Value.IsDouble) {
                if (double.IsPositiveInfinity(Value.Double))
                    return "Infinity";
                else if (double.IsNegativeInfinity(Value.Double))
                    return "-Infinity";
                else if (double.IsNaN(Value.Double))
                    return "NaN";
            }

            string FloatString = Value.ToString();
            if (!FloatString.Contains('.'))
                FloatString += ".0";
            return FloatString;
        }
        public FloatInstance(Class fromClass, DynFloat value) : base(fromClass) {
            Value = value;
        }
    }
    public class ProcInstance : Instance {
        Method Value;
        public override object? Object { get { return Value; } }
        public override Method Proc { get { return Value; } }
        public ProcInstance(Class fromClass, Method value) : base(fromClass) {
            Value = value;
        }
        public void SetValue(Method value) {
            Value = value;
        }
    }
    public class ThreadInstance : Instance {
        public readonly ScopeThread ScopeThread;
        public override object? Object { get { return ScopeThread; } }
        public override ScopeThread Thread { get { return ScopeThread; } }
        public ThreadInstance(Class fromClass, Scope fromScope) : base(fromClass) {
            ScopeThread = new ScopeThread(fromScope);
        }
        public void SetMethod(Method method) {
            Thread.Method = method;
        }
    }
    public class RangeInstance : Instance {
        public IntegerInstance? Min;
        public IntegerInstance? Max;
        public Instance AppliedMin;
        public Instance AppliedMax;
        public bool IncludesMax;
        public override object? Object { get { return ToIntegerRange; } }
        public override IntegerRange Range { get { return ToIntegerRange; } }
        public override string Inspect() {
            return $"{(Min != null ? Min.Inspect() : "")}{(IncludesMax ? ".." : "...")}{(Max != null ? Max.Inspect() : "")}";
        }
        public RangeInstance(Class fromClass, IntegerInstance? min, IntegerInstance? max, bool includesMax) : base(fromClass) {
            Min = min;
            Max = max;
            IncludesMax = includesMax;
            (AppliedMin, AppliedMax) = Setup();
            Setup();
        }
        public void SetValue(IntegerInstance min, IntegerInstance max, bool includesMax) {
            Min = min;
            Max = max;
            IncludesMax = includesMax;
            Setup();
        }
        (Instance, Instance) Setup() {
            if (Min == null) {
                AppliedMin = Interpreter.Api.Nil;
                AppliedMax = IncludesMax ? Max! : new IntegerInstance(Interpreter.Api.Integer, Max!.Integer - 1);
            }
            else if (Max == null) {
                AppliedMin = Min;
                AppliedMax = Interpreter.Api.Nil;
            }
            else {
                AppliedMin = Min;
                AppliedMax = IncludesMax ? Max : new IntegerInstance(Interpreter.Api.Integer, Max.Integer - 1);
            }
            return (AppliedMin, AppliedMax);
        }
        IntegerRange ToIntegerRange => new(AppliedMin is IntegerInstance ? (long)AppliedMin.Integer : null, AppliedMax is IntegerInstance ? (long)AppliedMax.Integer : null);
    }
    public class ArrayInstance : Instance {
        List<Instance> Value;
        public override object? Object { get { return Value; } }
        public override List<Instance> Array { get { return Value; } }
        public override string Inspect() {
            return $"[{Value.InspectInstances()}]";
        }
        public override string LightInspect() {
            return Value.LightInspectInstances("\n");
        }
        public ArrayInstance(Class fromClass, List<Instance> value) : base(fromClass) {
            Value = value;
        }
        public void SetValue(List<Instance> value) {
            Value = value;
        }
        public override int GetHashCode() {
            unchecked {
                int CurrentHash = 19;
                foreach (Instance Item in Value) {
                    CurrentHash = CurrentHash * 31 + Item.GetHashCode();
                }
                return CurrentHash;
            }
        }
    }
    public class HashInstance : Instance {
        HashDictionary Value;
        public Instance DefaultValue;
        public override object? Object { get { return Value; } }
        public override HashDictionary Hash { get { return Value; } }
        public override string Inspect() {
            return $"{{{Value.InspectHash()}}}";
        }
        public HashInstance(Class fromClass, HashDictionary value, Instance defaultValue) : base(fromClass) {
            Value = value;
            DefaultValue = defaultValue;
        }
        public void SetValue(HashDictionary value, Instance defaultValue) {
            Value = value;
            DefaultValue = defaultValue;
        }
        public void SetValue(HashDictionary value) {
            Value = value;
        }
        public override int GetHashCode() {
            unchecked {
                int CurrentHash = 0;
                foreach (KeyValuePair<Instance, Instance> Item in Value.KeyValues) {
                    CurrentHash ^= Item.Key.GetHashCode() ^ Item.Value.GetHashCode();
                }
                return CurrentHash;
            }
        }
    }
    public class HashArgumentsInstance : Instance {
        public readonly HashInstance Value;
        public override string Inspect() {
            return $"Hash arguments instance: {{{Value.Inspect()}}}";
        }
        public HashArgumentsInstance(HashInstance value, Interpreter interpreter) : base(interpreter) {
            Value = value;
        }
    }
    public class ExceptionInstance : Instance {
        Exception Value;
        public override object? Object { get { return Value; } }
        public override Exception Exception { get { return Value; } }
        public ExceptionInstance(Class fromClass, string message) : base(fromClass) {
            Value = new Exception(message);
        }
        public ExceptionInstance(Class fromClass, Exception exception) : base(fromClass) {
            Value = exception;
        }
        public void SetValue(string message) {
            Value = new Exception(message);
        }
        public void SetValue(Exception exception) {
            Value = exception;
        }
    }
    public class TimeInstance : Instance {
        DateTimeOffset Value;
        public override object? Object { get { return Value; } }
        public override DateTimeOffset Time { get { return Value; } }
        public override string Inspect() {
            return Value.ToString(System.Globalization.CultureInfo.GetCultureInfo("ja-JP")); // yyyy/mm/dd format
        }
        public TimeInstance(Class fromClass, DateTimeOffset value) : base(fromClass) {
            Value = value;
        }
        public void SetValue(DateTimeOffset value) {
            Value = value;
        }
    }
    public class WeakRefInstance : Instance {
        WeakReference<Instance> Value;
        public override object? Object { get { return Value; } }
        public override WeakReference<Instance> WeakRef { get { return Value; } }
        public WeakRefInstance(Class fromClass, WeakReference<Instance> value) : base(fromClass) {
            Value = value;
        }
        public void SetValue(WeakReference<Instance> value) {
            Value = value;
        }
    }
    public class HttpResponseInstance : Instance {
        HttpResponseMessage Value;
        public override object? Object { get { return Value; } }
        public override HttpResponseMessage HttpResponse { get { return Value; } }
        public HttpResponseInstance(Class fromClass, HttpResponseMessage value) : base(fromClass) {
            Value = value;
        }
        public void SetValue(HttpResponseMessage value) {
            Value = value;
        }
    }
    public class ModuleReference : Instance {
        public override object? Object { get { return Module!; } }
        public override string Inspect() {
            return Module!.Name;
        }
        public override string LightInspect() {
            return Module!.Name;
        }
        public ModuleReference(Module module) : base(module) {
            // Copy changes to the parent module
            InstanceMethods = module.InstanceMethods;
            InstanceVariables = module.InstanceVariables;
        }
    }
}
