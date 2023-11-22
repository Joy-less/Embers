using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Linq;
using static Embers.Scope;
using static Embers.Phase2;

#nullable enable

namespace Embers
{
    public class IntRange {
        public readonly int? Min;
        public readonly int? Max;
        public IntRange(int? min = null, int? max = null) {
            Min = min;
            Max = max;
        }
        public IntRange(Range range) {
            if (range.Start.IsFromEnd) {
                Min = null;
                Max = range.End.Value;
            }
            else if (range.End.IsFromEnd) {
                Min = range.Start.Value;
                Max = null;
            }
            else {
                Min = range.Start.Value;
                Max = range.End.Value;
            }
        }
        public bool IsInRange(int Number) {
            if (Min != null && Number < Min) return false;
            if (Max != null && Number > Max) return false;
            return true;
        }
        public override string ToString() {
            if (Min == Max) {
                if (Min == null)
                    return "any";
                else
                    return $"{Min}";
            }
            else {
                if (Min == null)
                    return $"{Max}";
                else if (Max == null)
                    return $"{Min}+";
                else
                    return $"{Min}..{Max}";
            }
        }
        public string Serialise() {
            return $"new {typeof(IntRange).GetPath()}({(Min != null ? Min : "null")}, {(Max != null ? Max : "null")})";
        }
    }
    public class IntegerRange {
        public readonly DynInteger? Min;
        public readonly DynInteger? Max;
        public IntegerRange(DynInteger? min = null, DynInteger? max = null) {
            Min = min;
            Max = max;
        }
        public bool IsInRange(DynInteger Number) {
            if (Min != null && Number < Min) return false;
            if (Max != null && Number > Max) return false;
            return true;
        }
        public bool IsInRange(DynFloat Number) {
            if (Min != null && Number < (DynFloat)Min) return false;
            if (Max != null && Number > (DynFloat)Max) return false;
            return true;
        }
        public long? Count => Max != null && Min != null
            ? (long)Max - (long)Min + 1
            : null;
        public override string ToString() {
            if (Min == Max) {
                if (Min == null)
                    return "any";
                else
                    return $"{Min}";
            }
            else {
                if (Min == null)
                    return $"{Max}";
                else if (Max == null)
                    return $"{Min}+";
                else
                    return $"{Min}..{Max}";
            }
        }
        public string Serialise() {
            return $"new {typeof(IntegerRange).GetPath()}({(Min != null ? Min : "null")}, {(Max != null ? Max : "null")})";
        }
    }
    public readonly struct DynInteger {
        public readonly long Long;
        public readonly BigInteger BigInteger;
        public readonly bool IsLong;
        public DynInteger(long Long) {
            this.Long = Long;
            BigInteger = default;
            IsLong = true;
        }
        public DynInteger(BigInteger BigInteger) {
            this.BigInteger = BigInteger;
            Long = default;
            IsLong = false;
        }
        private static DynInteger Compute(DynInteger Left, DynInteger Right, Func<long, long, long> LongCalculation, Func<BigInteger, BigInteger, BigInteger> BigCalculation) {
            if (Left.IsLong && Right.IsLong) {
                long SmallResult = LongCalculation(Left.Long, Right.Long);
                if (SmallResult.IsSmall()) return SmallResult;
            }

            BigInteger BigLeft = Left.IsLong ? Left.Long : Left.BigInteger;
            BigInteger BigRight = Right.IsLong ? Right.Long : Right.BigInteger;

            BigInteger Result = BigCalculation(BigLeft, BigRight);
            if (Result.IsSmall()) return (long)Result;
            else return Result;
        }
        private static bool Compare(DynInteger Left, DynInteger Right, Func<long, long, bool> LongComparison, Func<BigInteger, BigInteger, bool> BigComparison) {
            if (Left.IsLong && Right.IsLong) {
                return LongComparison(Left.Long, Right.Long);
            }
            BigInteger BigLeft = Left.IsLong ? Left.Long : Left.BigInteger;
            BigInteger BigRight = Right.IsLong ? Right.Long : Right.BigInteger;
            return BigComparison(BigLeft, BigRight);
        }
        public static DynInteger operator +(DynInteger Left, DynInteger Right)
            => Compute(Left, Right, (L, R) => L + R, (L, R) => L + R);
        public static DynInteger operator -(DynInteger Left, DynInteger Right)
            => Compute(Left, Right, (L, R) => L - R, (L, R) => L - R);
        public static DynInteger operator *(DynInteger Left, DynInteger Right)
            => Compute(Left, Right, (L, R) => L * R, (L, R) => L * R);
        public static DynInteger operator /(DynInteger Left, DynInteger Right)
            => Compute(Left, Right, (L, R) => L / R, (L, R) => L / R);
        public static DynInteger operator %(DynInteger Left, DynInteger Right)
            => Compute(Left, Right, (L, R) => L % R, (L, R) => L % R);
        public static bool operator <(DynInteger Left, DynInteger Right)
            => Compare(Left, Right, (L, R) => L < R, (L, R) => L < R);
        public static bool operator >(DynInteger Left, DynInteger Right)
            => Compare(Left, Right, (L, R) => L > R, (L, R) => L > R);
        public static bool operator <=(DynInteger Left, DynInteger Right)
            => Compare(Left, Right, (L, R) => L <= R, (L, R) => L <= R);
        public static bool operator >=(DynInteger Left, DynInteger Right)
            => Compare(Left, Right, (L, R) => L >= R, (L, R) => L >= R);
        public static bool operator ==(DynInteger? Left, DynInteger? Right) {
            if (Left is null) return Right is null;
            if (Right is null) return Left is null;
            return Compare(Left.Value, Right.Value, (L, R) => L == R, (L, R) => L == R);
        }
        public static bool operator !=(DynInteger? Left, DynInteger? Right) => !(Left == Right);
        public static DynInteger operator -(DynInteger Value) => Value * -1;
        public static DynInteger operator ++(DynInteger Value) => Value + 1;
        public static DynInteger operator --(DynInteger Value) => Value - 1;
        public static implicit operator DynInteger(long Value) => new(Value);
        public static implicit operator DynInteger(BigInteger Value) => new(Value);
        public static explicit operator long(DynInteger Value) => Value.IsLong ? Value.Long : (long)Value.BigInteger;
        public static explicit operator BigInteger(DynInteger Value) => Value.IsLong ? Value.Long : Value.BigInteger;
        public override string ToString() {
            return IsLong ? Long.ToString() : BigInteger.ToString();
        }
        public override bool Equals(object? Obj) {
            if (Obj is DynInteger ObjInteger) return this == ObjInteger;
            return base.Equals(Obj);
        }
        public override int GetHashCode() {
            return IsLong ? Long.GetHashCode() : BigInteger.GetHashCode();
        }
    }
    public readonly struct DynFloat {
        public readonly double Double;
        public readonly BigFloat BigFloat;
        public readonly bool IsDouble;
        public DynFloat(double Double) {
            this.Double = Double;
            BigFloat = default;
            IsDouble = true;
        }
        public DynFloat(long Long) {
            Double = Long;
            BigFloat = default;
            IsDouble = true;
        }
        public DynFloat(BigFloat BigFloat) {
            this.BigFloat = BigFloat;
            Double = default;
            IsDouble = false;
        }
        public DynFloat(DynInteger Integer) {
            BigFloat = Integer.IsLong ? default : new BigFloat(Integer.BigInteger);
            Double = Integer.IsLong ? Integer.Long : default;
            IsDouble = Integer.IsLong;
        }
        private static DynFloat Compute(DynFloat Left, DynFloat Right, Func<double, double, double> DoubleCalculation, Func<BigFloat, BigFloat, BigFloat> BigCalculation) {
            if (Left.IsDouble && Right.IsDouble || Left.IsDouble && double.IsInfinity(Left.Double) || Right.IsDouble && double.IsInfinity(Right.Double)) {
                double SmallResult = DoubleCalculation(Left.Double, Right.Double);
                if (SmallResult.IsSmall()) return SmallResult;
            }

            BigFloat BigLeft = Left.IsDouble ? Left.Double : Left.BigFloat;
            BigFloat BigRight = Right.IsDouble ? Right.Double : Right.BigFloat;

            BigFloat Result = BigCalculation(BigLeft, BigRight);
            if (Result.IsSmall()) return (double)Result;
            else return Result;
        }
        private static bool Compare(DynFloat Left, DynFloat Right, Func<double, double, bool> DoubleComparison, Func<BigFloat, BigFloat, bool> BigComparison) {
            if (Left.IsDouble && Right.IsDouble) {
                return DoubleComparison(Left.Double, Right.Double);
            }
            BigFloat BigLeft = Left.IsDouble ? Left.Double : Left.BigFloat;
            BigFloat BigRight = Right.IsDouble ? Right.Double : Right.BigFloat;
            return BigComparison(BigLeft, BigRight);
        }
        public static DynFloat operator +(DynFloat Left, DynFloat Right)
            => Compute(Left, Right, (L, R) => L + R, (L, R) => L + R);
        public static DynFloat operator -(DynFloat Left, DynFloat Right)
            => Compute(Left, Right, (L, R) => L - R, (L, R) => L - R);
        public static DynFloat operator *(DynFloat Left, DynFloat Right)
            => Compute(Left, Right, (L, R) => L * R, (L, R) => L * R);
        public static DynFloat operator /(DynFloat Left, DynFloat Right)
            => Compute(Left, Right, (L, R) => L / R, (L, R) => L / R);
        public static DynFloat operator %(DynFloat Left, DynFloat Right)
            => Compute(Left, Right, (L, R) => L % R, (L, R) => L % R);
        public static bool operator <(DynFloat Left, DynFloat Right)
            => Compare(Left, Right, (L, R) => L < R, (L, R) => L < R);
        public static bool operator >(DynFloat Left, DynFloat Right)
            => Compare(Left, Right, (L, R) => L > R, (L, R) => L > R);
        public static bool operator <=(DynFloat Left, DynFloat Right)
            => Compare(Left, Right, (L, R) => L <= R, (L, R) => L <= R);
        public static bool operator >=(DynFloat Left, DynFloat Right)
            => Compare(Left, Right, (L, R) => L >= R, (L, R) => L >= R);
        public static bool operator ==(DynFloat? Left, DynFloat? Right) {
            if (Left is null) return Right is null;
            if (Right is null) return Left is null;
            return Compare(Left.Value, Right.Value, (L, R) => L == R, (L, R) => L == R);
        }
        public static bool operator !=(DynFloat? Left, DynFloat? Right) => !(Left == Right);
        public static DynFloat operator -(DynFloat Value) => Value * -1;
        public static DynFloat operator ++(DynFloat Value) => Value + 1;
        public static DynFloat operator --(DynFloat Value) => Value - 1;
        public static DynFloat operator +(DynInteger Left, DynFloat Right) => (DynFloat)Left + Right;
        public static DynFloat operator -(DynInteger Left, DynFloat Right) => (DynFloat)Left - Right;
        public static DynFloat operator *(DynInteger Left, DynFloat Right) => (DynFloat)Left * Right;
        public static DynFloat operator /(DynInteger Left, DynFloat Right) => (DynFloat)Left / Right;
        public static DynFloat operator %(DynInteger Left, DynFloat Right) => (DynFloat)Left % Right;
        public static implicit operator DynFloat(double Value) => new(Value);
        public static implicit operator DynFloat(DynInteger Value) => new(Value);
        public static implicit operator DynFloat(BigFloat Value) => new(Value);
        public static explicit operator double(DynFloat Value) => Value.IsDouble ? Value.Double : (double)Value.BigFloat;
        public static explicit operator BigFloat(DynFloat Value) => Value.IsDouble ? Value.Double : Value.BigFloat;
        public static explicit operator DynInteger(DynFloat Value) => Value.IsDouble ? Value.Double.IsSmall() ? new((long)Value.Double) : new((BigInteger)Value.Double) : new((BigInteger)Value.BigFloat);
        public override string ToString() {
            return IsDouble ? Double.ToString() : BigFloat.ToString();
        }
        public override bool Equals(object? Obj) {
            if (Obj is DynFloat ObjInteger) return this == ObjInteger;
            return base.Equals(Obj);
        }
        public override int GetHashCode() {
            return IsDouble ? Double.GetHashCode() : BigFloat.GetHashCode();
        }
    }

    public class Method {
        public string? Name {get; private set;}
        public Func<MethodInput, Task<Instance>> Function {get; private set;}
        public readonly IntRange ArgumentCountRange;
        public readonly List<MethodArgumentExpression> ArgumentNames;
        public readonly bool Unsafe;
        public AccessModifier AccessModifier { get; private set; }
        public Method(Func<MethodInput, Task<Instance>> function, IntRange? argumentCountRange, List<MethodArgumentExpression>? argumentNames = null, bool IsUnsafe = false, AccessModifier accessModifier = AccessModifier.Public) {
            Function = function;
            ArgumentCountRange = argumentCountRange ?? new IntRange();
            ArgumentNames = argumentNames ?? new();
            Unsafe = IsUnsafe;
            AccessModifier = accessModifier;
        }
        public Method(Func<MethodInput, Task<Instance>> function, int argumentCount, List<MethodArgumentExpression>? argumentNames = null, bool IsUnsafe = false, AccessModifier accessModifier = AccessModifier.Public) {
            Function = function;
            ArgumentCountRange = new IntRange(argumentCount, argumentCount);
            ArgumentNames = argumentNames ?? new();
            Unsafe = IsUnsafe;
            AccessModifier = accessModifier;
        }
        public async Task<Instance> Call(Scope Scope, Instance? OnInstance, Instances? Arguments = null, Method? OnYield = null, BreakHandleType BreakHandleType = BreakHandleType.Invalid, bool CatchReturn = true, bool BypassAccessModifiers = false) {
            if (Unsafe && !Scope.AllowUnsafeApi) {
                throw new RuntimeException($"{Scope.ApproximateLocation}: The method '{Name}' is unavailable since AllowUnsafeApi is disabled for this script.");
            }
            if (!BypassAccessModifiers) {
                if (AccessModifier == AccessModifier.Private) {
                    if (OnInstance != null && Scope.CurrentModule != OnInstance.Module!) {
                        throw new RuntimeException($"{Scope.ApproximateLocation}: Private method '{Name}' called for {OnInstance.Module!.Name}.");
                    }
                }
                else if (AccessModifier == AccessModifier.Protected) {
                    if (OnInstance != null && !Scope.CurrentModule.InheritsFrom(OnInstance.Module!)) {
                        throw new RuntimeException($"{Scope.ApproximateLocation}: Protected method '{Name}' called for {OnInstance.Module!.Name}.");
                    }
                }
            }
            Arguments ??= Instances.None;
            if (ArgumentCountRange.IsInRange(Arguments.Count)) {
                // Create call scope
                Scope CallScope = new(Scope) { CurrentModule = OnInstance?.Module!, CurrentInstance = OnInstance!, CurrentMethod = this };
                // Create method input
                MethodInput Input = new(CallScope, OnInstance, Arguments, OnYield);
                // Set argument variables
                await SetArgumentVariables(CallScope, Input);
                // Call method
                Instance ReturnValue = await Function(Input);
                // Handle return codes
                if (ReturnValue is ReturnCodeInstance ReturnCodeInstance) {
                    if (ReturnCodeInstance.CalledInYieldMethod) {
                        ReturnCodeInstance.CalledInYieldMethod = false;
                    }
                    else {
                        if (ReturnValue is LoopControlReturnCode LoopControlReturnCode) {
                            // Break
                            if (LoopControlReturnCode.Type == LoopControlType.Break) {
                                if (BreakHandleType != BreakHandleType.Rethrow) {
                                    if (BreakHandleType == BreakHandleType.Destroy)
                                        ReturnValue = Scope.Api.Nil;
                                    else
                                        throw new SyntaxErrorException($"{Scope.ApproximateLocation}: Invalid break (break must be in a loop)");
                                }
                            }
                        }
                        // Return
                        else if (ReturnValue is ReturnReturnCode ReturnReturnCode) {
                            if (CatchReturn) {
                                ReturnValue = ReturnReturnCode.ReturnValue;
                            }
                        }
                    }
                }
                // Return method return value
                return ReturnValue;
            }
            else {
                throw new RuntimeException($"{Scope.ApproximateLocation}: Wrong number of arguments for '{Name}' (given {Arguments.Count}, expected {ArgumentCountRange})");
            }
        }
        public void SetName(string? name) {
            Name = name;
            if (name == "initialize") AccessModifier = AccessModifier.Private;
        }
        public void ChangeFunction(Func<MethodInput, Task<Instance>> function) {
            Function = function;
        }
        public async Task SetArgumentVariables(Scope Scope, MethodInput Input) {
            Instances Arguments = Input.Arguments;
            // Set argument variables
            int ArgumentNameIndex = 0;
            int ArgumentIndex = 0;
            while (ArgumentNameIndex < ArgumentNames.Count) {
                MethodArgumentExpression ArgumentName = ArgumentNames[ArgumentNameIndex];
                string ArgumentIdentifier = ArgumentName.ArgumentName.Value!;
                // Declare argument as variable in local scope
                if (ArgumentIndex < Arguments.Count) {
                    // Splat argument
                    if (ArgumentName.SplatType == SplatType.Single) {
                        // Add splat arguments while there will be enough remaining arguments
                        List<Instance> SplatArguments = new();
                        while (Arguments.Count - ArgumentIndex >= ArgumentNames.Count - ArgumentNameIndex) {
                            SplatArguments.Add(Arguments[ArgumentIndex]);
                            ArgumentIndex++;
                        }
                        if (SplatArguments.Count != 0)
                            ArgumentIndex--;
                        // Add extra ungiven double splat argument if available
                        if (ArgumentNameIndex + 1 < ArgumentNames.Count && ArgumentNames[ArgumentNameIndex + 1].SplatType == SplatType.Double
                            && Arguments[^1] is not HashArgumentsInstance)
                        {
                            SplatArguments.Add(Arguments[ArgumentIndex]);
                            ArgumentIndex++;
                        }
                        // Create array from splat arguments
                        ArrayInstance SplatArgumentsArray = new(Input.Api.Array, SplatArguments);
                        // Add array to scope
                        Scope.LocalVariables[ArgumentIdentifier] = SplatArgumentsArray;
                    }
                    // Double splat argument
                    else if (ArgumentName.SplatType == SplatType.Double && Arguments[^1] is HashArgumentsInstance DoubleSplatArgumentsHash) {
                        // Add hash to scope
                        Scope.LocalVariables[ArgumentIdentifier] = DoubleSplatArgumentsHash.Value;
                    }
                    // Normal argument
                    else {
                        Scope.LocalVariables[ArgumentIdentifier] = Arguments[ArgumentIndex];
                    }
                }
                // Optional argument not given
                else {
                    Instance DefaultValue = ArgumentName.DefaultValue != null ? (await Input.Scope.InterpretExpressionAsync(ArgumentName.DefaultValue)) : Input.Api.Nil;
                    Scope.LocalVariables[ArgumentIdentifier] = DefaultValue;
                }
                ArgumentNameIndex++;
                ArgumentIndex++;
            }
        }
    }
    public class MethodInput {
        public Scope Scope { get; private set; }
        public readonly Api Api;
        public readonly Instances Arguments;
        public readonly Method? OnYield;
        readonly Instance? InputInstance;
        public MethodInput(Scope scope, Instance? instance, Instances arguments, Method? onYield = null) {
            Scope = scope;
            Api = scope.Api;
            InputInstance = instance;
            Arguments = arguments;
            OnYield = onYield;
        }
        public DebugLocation Location => Scope.ApproximateLocation;
        public Interpreter Interpreter => Scope.Interpreter;
        public Instance Instance => InputInstance!;
        public void OverrideScope(Scope NewScope) {
            Scope = NewScope;
        }
    }
    public class Instances {
        // At least one of Instance or InstanceList will be null
        readonly Instance? Instance;
        readonly List<Instance>? InstanceList;
        public readonly int Count;

        public static readonly Instances None = new();

        public Instances(Instance? instance = null) {
            Instance = instance;
            Count = instance != null ? 1 : 0;
        }
        public Instances(List<Instance> instanceList) {
            InstanceList = instanceList;
            Count = instanceList.Count;
        }
        public Instances(params Instance[] instanceArray) {
            InstanceList = instanceArray.ToList();
            Count = InstanceList.Count;
        }
        public static implicit operator Instances(Instance Instance) {
            return new Instances(Instance);
        }
        public static implicit operator Instances(List<Instance> InstanceList) {
            return new Instances(InstanceList);
        }
        public static implicit operator Instance(Instances Instances) {
            if (Instances.Count != 1) {
                if (Instances.Count == 0)
                    throw new RuntimeException($"Cannot implicitly cast Instances to Instance because there are none");
                else
                    throw new RuntimeException($"Cannot implicitly cast Instances to Instance because {Instances.Count - 1} instances would be overlooked");
            }
            return Instances[0];
        }
        public Instance this[Index i] => InstanceList != null
            ? InstanceList[i]
            : (i.Value == 0 && Instance != null ? Instance : throw new ApiException("Index was outside the range of the instances"));
        public IEnumerator<Instance> GetEnumerator() {
            if (InstanceList != null) {
                for (int i = 0; i < InstanceList.Count; i++) {
                    yield return InstanceList[i];
                }
            }
            else if (Instance != null) {
                yield return Instance;
            }
        }
        public Instance SingleInstance => Count == 1
            ? this[0]
            : throw new SyntaxErrorException($"Unexpected instances (expected one, got {Count})");
        public List<Instance> MultiInstance => InstanceList ?? (Instance != null
            ? new List<Instance>() { Instance }
            : new List<Instance>());
    }

    /// <summary>A thread-safe dictionary that is locked while a key is being changed.</summary>
    public class LockingDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull {
        public new void Add(TKey Key, TValue Value) {
            TrySetMethodName(Key, Value);
            lock (this) base.Add(Key, Value);
        }
        public new bool Remove(TKey Key) {
            lock (this) return base.Remove(Key);
        }
        public new bool TryAdd(TKey Key, TValue Value) {
            TrySetMethodName(Key, Value);
            lock (this) return base.TryAdd(Key, Value);
        }
        public new TValue this[TKey Key] {
            get {
                lock (this) return base[Key];
            }
            set {
                TrySetMethodName(Key, value);
                lock (this) base[Key] = value;
            }
        }
        public TValue this[TKey FirstKey, params TKey[] Keys] {
            set {
                TrySetMethodName(FirstKey, value);
                this[FirstKey] = value;
                foreach (TKey Key in Keys) {
                    this[Key] = value;
                }
            }
        }
        public new void Clear() {
            lock (this) base.Clear();
        }
        static void TrySetMethodName(TKey Key, TValue Value) {
            if (Value is Method Method && Key is string MethodName) {
                Method.SetName(MethodName);
            }
        }
    }
    /// <summary>A locking dictionary that supports events when a key is set or removed.</summary>
    public class ReactiveDictionary<TKey, TValue> : LockingDictionary<TKey, TValue> where TKey : notnull {
        readonly Action<TKey, TValue> OnSet;
        readonly Action<TKey> OnRemoved;
        public ReactiveDictionary(Action<TKey, TValue> onSet, Action<TKey> onRemoved) {
            OnSet = onSet;
            OnRemoved = onRemoved;
        }
        public new void Add(TKey Key, TValue Value) {
            OnSet.Invoke(Key, Value);
            lock (this) base.Add(Key, Value);
        }
        public new bool Remove(TKey Key) {
            OnRemoved.Invoke(Key);
            lock (this) return base.Remove(Key);
        }
        public new bool TryAdd(TKey Key, TValue Value) {
            OnSet.Invoke(Key, Value);
            lock (this) return base.TryAdd(Key, Value);
        }
        public new TValue this[TKey Key] {
            get {
                lock (this) return base[Key];
            }
            set {
                OnSet.Invoke(Key, value);
                lock (this) base[Key] = value;
            }
        }
        public new TValue this[TKey FirstKey, params TKey[] Keys] {
            set {
                OnSet.Invoke(FirstKey, value);
                foreach (TKey Key in Keys) {
                    OnSet.Invoke(Key, value);
                }
                base[FirstKey, Keys] = value;
            }
        }
        public new void Clear() {
            lock (this) {
                foreach (TKey Key in Keys) {
                    OnRemoved.Invoke(Key);
                }
                base.Clear();
            }
        }
    }
    public class HashDictionary {
        private readonly LockingDictionary<DynInteger, HashSet<KeyValuePair<Instance, Instance>>> Dict;
        public HashDictionary() {
            Dict = new();
        }
        public HashDictionary(LockingDictionary<DynInteger, HashSet<KeyValuePair<Instance, Instance>>> dict) {
            Dict = dict;
        }
        public async Task<Instance?> Lookup(Scope Scope, Instance Key) {
            DynInteger HashKey = (await Key.CallInstanceMethod(Scope, "hash")).Integer;
            if (Dict.TryGetValue(HashKey, out HashSet<KeyValuePair<Instance, Instance>>? Entry)) {
                foreach (KeyValuePair<Instance, Instance> Match in Entry) {
                    if ((await Match.Key.CallInstanceMethod(Scope, "eql?", Key)).IsTruthy) {
                        return Match.Value;
                    }
                }
            }
            return null;
        }
        public async Task<Instance?> ReverseLookup(Scope Scope, Instance Value) {
            foreach (KeyValuePair<DynInteger, HashSet<KeyValuePair<Instance, Instance>>> Entry in Dict) {
                foreach (KeyValuePair<Instance, Instance> Match in Entry.Value) {
                    if ((await Match.Value.CallInstanceMethod(Scope, "==", Value)).IsTruthy) {
                        return Match.Key;
                    }
                }
            }
            return null;
        }
        public async Task Store(Scope Scope, Instance Key, Instance Value) {
            DynInteger HashKey = (await Key.CallInstanceMethod(Scope, "hash")).Integer;
            if (!Dict.TryGetValue(HashKey, out HashSet<KeyValuePair<Instance, Instance>>? Entry)) {
                Entry = new HashSet<KeyValuePair<Instance, Instance>>();
                lock (Dict) Dict[HashKey] = Entry;
            }
            await Remove(Scope, Key);
            lock (Entry) Entry.Add(new KeyValuePair<Instance, Instance>(Key, Value));
        }
        public async Task<Instance?> Remove(Scope Scope, Instance Key) {
            DynInteger HashKey = (await Key.CallInstanceMethod(Scope, "hash")).Integer;
            if (Dict.TryGetValue(HashKey, out HashSet<KeyValuePair<Instance, Instance>>? Entry)) {
                foreach (KeyValuePair<Instance, Instance> Match in Entry) {
                    if ((await Match.Key.CallInstanceMethod(Scope, "eql?", Key)).IsTruthy) {
                        lock (Entry) Entry.Remove(Match);
                        return Match.Value;
                    }
                }
            }
            return null;
        }
        public void Clear() {
            Dict.Clear();
        }
        public List<KeyValuePair<Instance, Instance>> KeyValues { get {
            List<KeyValuePair<Instance, Instance>> KeyValues = new();
            foreach (HashSet<KeyValuePair<Instance, Instance>> Entry in Dict.Values) {
                foreach (KeyValuePair<Instance, Instance> Match in Entry) {
                    KeyValues.Add(Match);
                }
            }
            return KeyValues;
        } }
        public List<Instance> Keys { get {
            List<Instance> Keys = new();
            foreach (HashSet<KeyValuePair<Instance, Instance>> Entry in Dict.Values) {
                foreach (KeyValuePair<Instance, Instance> Match in Entry) {
                    Keys.Add(Match.Key);
                }
            }
            return Keys;
        } }
        public List<Instance> Values { get {
            List<Instance> Values = new();
            foreach (HashSet<KeyValuePair<Instance, Instance>> Entry in Dict.Values) {
                foreach (KeyValuePair<Instance, Instance> Match in Entry) {
                    Values.Add(Match.Value);
                }
            }
            return Values;
        } }
        public int Count => KeyValues.Count;
    }
    /// <summary>A dictionary cache of weak references. Values should be removed from the cache in their finalisers.</summary>
    public class WeakCache<TKey, TValue> where TKey : notnull where TValue : class {
        public readonly LockingDictionary<TKey, WeakReference<TValue>> Dict = new();

        public TValue? this[TKey Key] { get {
            Dict.TryGetValue(Key, out WeakReference<TValue>? CacheValue);
            if (CacheValue != null && CacheValue.TryGetTarget(out TValue? Value)) {
                return Value;
            }
            return null;
        } }
        public TValue Store(TKey Key, TValue Value) {
            Dict[Key] = new WeakReference<TValue>(Value);
            return Value;
        }
    }
}
