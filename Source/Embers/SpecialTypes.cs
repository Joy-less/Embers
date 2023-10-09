using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Threading.Tasks;
using System.Linq;
using static Embers.Script;

#nullable enable

namespace Embers
{
    public static class SpecialTypes
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
            public static DynInteger operator +(DynInteger Left, DynInteger Right) {
                if (Left.IsLong && Right.IsLong) {
                    long SmallResult = Left.Long + Right.Long;
                    if (SmallResult.IsSmall()) return SmallResult;
                }

                BigInteger BigLeft = Left.IsLong ? Left.Long : Left.BigInteger;
                BigInteger BigRight = Right.IsLong ? Right.Long : Right.BigInteger;

                BigInteger Result = BigLeft + BigRight;
                if (Result.IsSmall()) return (long)Result;
                else return Result;
            }
            public static DynInteger operator -(DynInteger Left, DynInteger Right) {
                if (Left.IsLong && Right.IsLong) {
                    long SmallResult = Left.Long - Right.Long;
                    if (SmallResult.IsSmall()) return SmallResult;
                }

                BigInteger BigLeft = Left.IsLong ? Left.Long : Left.BigInteger;
                BigInteger BigRight = Right.IsLong ? Right.Long : Right.BigInteger;

                BigInteger Result = BigLeft - BigRight;
                if (Result.IsSmall()) return (long)Result;
                else return Result;
            }
            public static DynInteger operator *(DynInteger Left, DynInteger Right) {
                if (Left.IsLong && Right.IsLong) {
                    long SmallResult = Left.Long * Right.Long;
                    if (SmallResult.IsSmall()) return SmallResult;
                }

                BigInteger BigLeft = Left.IsLong ? Left.Long : Left.BigInteger;
                BigInteger BigRight = Right.IsLong ? Right.Long : Right.BigInteger;

                BigInteger Result = BigLeft * BigRight;
                if (Result.IsSmall()) return (long)Result;
                else return Result;
            }
            public static DynInteger operator /(DynInteger Left, DynInteger Right) {
                if (Left.IsLong && Right.IsLong) {
                    long SmallResult = Left.Long / Right.Long;
                    if (SmallResult.IsSmall()) return SmallResult;
                }

                BigInteger BigLeft = Left.IsLong ? Left.Long : Left.BigInteger;
                BigInteger BigRight = Right.IsLong ? Right.Long : Right.BigInteger;

                BigInteger Result = BigLeft / BigRight;
                if (Result.IsSmall()) return (long)Result;
                else return Result;
            }
            public static DynInteger operator %(DynInteger Left, DynInteger Right) {
                if (Left.IsLong && Right.IsLong) {
                    long SmallResult = Left.Long % Right.Long;
                    if (SmallResult.IsSmall()) return SmallResult;
                }

                BigInteger BigLeft = Left.IsLong ? Left.Long : Left.BigInteger;
                BigInteger BigRight = Right.IsLong ? Right.Long : Right.BigInteger;

                BigInteger Result = BigLeft % BigRight;
                if (Result.IsSmall()) return (long)Result;
                else return Result;
            }
            public static bool operator <(DynInteger Left, DynInteger Right) {
                if (Left.IsLong && Right.IsLong) {
                    return Left.Long < Right.Long;
                }
                BigInteger BigLeft = Left.IsLong ? Left.Long : Left.BigInteger;
                BigInteger BigRight = Right.IsLong ? Right.Long : Right.BigInteger;
                return BigLeft < BigRight;
            }
            public static bool operator >(DynInteger Left, DynInteger Right) {
                if (Left.IsLong && Right.IsLong) {
                    return Left.Long > Right.Long;
                }
                BigInteger BigLeft = Left.IsLong ? Left.Long : Left.BigInteger;
                BigInteger BigRight = Right.IsLong ? Right.Long : Right.BigInteger;
                return BigLeft > BigRight;
            }
            public static bool operator <=(DynInteger Left, DynInteger Right) {
                return Left == Right || Left < Right;
            }
            public static bool operator >=(DynInteger Left, DynInteger Right) {
                return Left == Right || Left > Right;
            }
            public static bool operator ==(DynInteger? Left, DynInteger? Right) {
                if (Left is null || Right is null) return Left is null && Right is null;
                if (Left.Value.IsLong && Right.Value.IsLong) {
                    return Left.Value.Long == Right.Value.Long;
                }
                BigInteger BigLeft = Left.Value.IsLong ? Left.Value.Long : Left.Value.BigInteger;
                BigInteger BigRight = Right.Value.IsLong ? Right.Value.Long : Right.Value.BigInteger;
                return BigLeft == BigRight;
            }
            public static bool operator !=(DynInteger? Left, DynInteger? Right) {
                return !(Left == Right);
            }
            public static DynInteger operator -(DynInteger Value) => Value * -1;
            public static DynInteger operator ++(DynInteger Value) => Value + 1;
            public static DynInteger operator --(DynInteger Value) => Value - 1;
            public static implicit operator DynInteger(long Value) {
                return new DynInteger(Value);
            }
            public static implicit operator DynInteger(BigInteger Value) {
                return new DynInteger(Value);
            }
            public static explicit operator long(DynInteger Value) {
                return Value.IsLong ? Value.Long : (long)Value.BigInteger;
            }
            public static explicit operator BigInteger(DynInteger Value) {
                return Value.IsLong ? Value.Long : Value.BigInteger;
            }
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
            public static DynFloat operator +(DynFloat Left, DynFloat Right) {
                if (Left.IsDouble && Right.IsDouble || double.IsInfinity(Left.Double) || double.IsInfinity(Right.Double)) {
                    double SmallResult = Left.Double + Right.Double;
                    if (SmallResult.IsSmall()) return SmallResult;
                }

                BigFloat BigLeft = Left.IsDouble ? Left.Double : Left.BigFloat;
                BigFloat BigRight = Right.IsDouble ? Right.Double : Right.BigFloat;

                BigFloat Result = BigLeft + BigRight;
                if (Result.IsSmall()) return (double)Result;
                else return Result;
            }
            public static DynFloat operator -(DynFloat Left, DynFloat Right) {
                if (Left.IsDouble && Right.IsDouble || double.IsInfinity(Left.Double) || double.IsInfinity(Right.Double)) {
                    double SmallResult = Left.Double - Right.Double;
                    if (SmallResult.IsSmall()) return SmallResult;
                }

                BigFloat BigLeft = Left.IsDouble ? Left.Double : Left.BigFloat;
                BigFloat BigRight = Right.IsDouble ? Right.Double : Right.BigFloat;

                BigFloat Result = BigLeft - BigRight;
                if (Result.IsSmall()) return (double)Result;
                else return Result;
            }
            public static DynFloat operator *(DynFloat Left, DynFloat Right) {
                if (Left.IsDouble && Right.IsDouble || double.IsInfinity(Left.Double) || double.IsInfinity(Right.Double)) {
                    double SmallResult = Left.Double * Right.Double;
                    if (SmallResult.IsSmall()) return SmallResult;
                }

                BigFloat BigLeft = Left.IsDouble ? Left.Double : Left.BigFloat;
                BigFloat BigRight = Right.IsDouble ? Right.Double : Right.BigFloat;

                BigFloat Result = BigLeft * BigRight;
                if (Result.IsSmall()) return (double)Result;
                else return Result;
            }
            public static DynFloat operator /(DynFloat Left, DynFloat Right) {
                if (Left.IsDouble && Right.IsDouble || double.IsInfinity(Left.Double) || double.IsInfinity(Right.Double)) {
                    double SmallResult = Left.Double / Right.Double;
                    if (SmallResult.IsSmall()) return SmallResult;
                }

                BigFloat BigLeft = Left.IsDouble ? Left.Double : Left.BigFloat;
                BigFloat BigRight = Right.IsDouble ? Right.Double : Right.BigFloat;

                BigFloat Result = BigLeft / BigRight;
                if (Result.IsSmall()) return (double)Result;
                else return Result;
            }
            public static DynFloat operator %(DynFloat Left, DynFloat Right) {
                if (Left.IsDouble && Right.IsDouble || double.IsInfinity(Left.Double) || double.IsInfinity(Right.Double)) {
                    double SmallResult = Left.Double % Right.Double;
                    if (SmallResult.IsSmall()) return SmallResult;
                }

                BigFloat BigLeft = Left.IsDouble ? Left.Double : Left.BigFloat;
                BigFloat BigRight = Right.IsDouble ? Right.Double : Right.BigFloat;

                BigFloat Result = BigLeft % BigRight;
                if (Result.IsSmall()) return (double)Result;
                else return Result;
            }
            public static bool operator <(DynFloat Left, DynFloat Right) {
                if (Left.IsDouble && Right.IsDouble) {
                    return Left.Double < Right.Double;
                }
                BigFloat BigLeft = Left.IsDouble ? Left.Double : Left.BigFloat;
                BigFloat BigRight = Right.IsDouble ? Right.Double : Right.BigFloat;
                return BigLeft < BigRight;
            }
            public static bool operator >(DynFloat Left, DynFloat Right) {
                if (Left.IsDouble && Right.IsDouble) {
                    return Left.Double > Right.Double;
                }
                BigFloat BigLeft = Left.IsDouble ? Left.Double : Left.BigFloat;
                BigFloat BigRight = Right.IsDouble ? Right.Double : Right.BigFloat;
                return BigLeft > BigRight;
            }
            public static bool operator <=(DynFloat Left, DynFloat Right) {
                return Left == Right || Left < Right;
            }
            public static bool operator >=(DynFloat Left, DynFloat Right) {
                return Left == Right || Left > Right;
            }
            public static bool operator ==(DynFloat? Left, DynFloat? Right) {
                if (Left is null || Right is null) return Left is null && Right is null;
                if (Left.Value.IsDouble && Right.Value.IsDouble) {
                    return Left.Value.Double == Right.Value.Double;
                }
                BigFloat BigLeft = Left.Value.IsDouble ? Left.Value.Double : Left.Value.BigFloat;
                BigFloat BigRight = Right.Value.IsDouble ? Right.Value.Double : Right.Value.BigFloat;
                return BigLeft == BigRight;
            }
            public static bool operator !=(DynFloat? Left, DynFloat? Right) {
                return !(Left == Right);
            }
            public static DynFloat operator -(DynFloat Value) => Value * -1;
            public static DynFloat operator ++(DynFloat Value) => Value + 1;
            public static DynFloat operator --(DynFloat Value) => Value - 1;
            public static DynFloat operator +(DynInteger Left, DynFloat Right) {
                return (DynFloat)Left + Right;
            }
            public static DynFloat operator -(DynInteger Left, DynFloat Right) {
                return (DynFloat)Left - Right;
            }
            public static DynFloat operator *(DynInteger Left, DynFloat Right) {
                return (DynFloat)Left * Right;
            }
            public static DynFloat operator /(DynInteger Left, DynFloat Right) {
                return (DynFloat)Left / Right;
            }
            public static DynFloat operator %(DynInteger Left, DynFloat Right) {
                return (DynFloat)Left % Right;
            }
            public static implicit operator DynFloat(double Value) {
                return new DynFloat(Value);
            }
            public static implicit operator DynFloat(DynInteger Value) {
                return new DynFloat(Value);
            }
            public static implicit operator DynFloat(BigFloat Value) {
                return new DynFloat(Value);
            }
            public static explicit operator double(DynFloat Value) {
                return Value.IsDouble ? Value.Double : (double)Value.BigFloat;
            }
            public static explicit operator BigFloat(DynFloat Value) {
                return Value.IsDouble ? Value.Double : Value.BigFloat;
            }
            public static explicit operator DynInteger(DynFloat Value) {
                if (Value.IsDouble) {
                    return new DynInteger((long)Value.Double);
                }
                else {
                    return new DynInteger((BigInteger)Value.BigFloat);
                }
            }
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
        public class WeakEvent<T> {
            private readonly ConditionalWeakTable<object, Action<T>> Subscribers = new();
            public void Listen(object AttachedObject, Action<T> Listener) {
                lock (Subscribers) Subscribers.Add(AttachedObject, Listener);
            }
            public void Fire(T Argument) {
                lock (Subscribers) {
                    foreach (KeyValuePair<object, Action<T>> Subscriber in Subscribers) {
                        Subscriber.Value(Argument);
                    }
                }
            }
        }
        public class WeakEvent<T1, T2> {
            private readonly ConditionalWeakTable<object, Action<T1, T2>> Subscribers = new();
            public void Listen(object AttachedObject, Action<T1, T2> Listener) {
                lock (Subscribers) Subscribers.Add(AttachedObject, Listener);
            }
            public void Fire(T1 Argument1, T2 Argument2) {
                lock (Subscribers) {
                    foreach (KeyValuePair<object, Action<T1, T2>> Subscriber in Subscribers) {
                        Subscriber.Value(Argument1, Argument2);
                    }
                }
            }
        }
        /// <summary>A thread-safe dictionary that is locked while a key is being added or set.</summary>
        public class LockingDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull {
            public new void Add(TKey Key, TValue Value) {
                lock (this) base.Add(Key, Value);
            }
            public new bool Remove(TKey Key) {
                lock (this) return base.Remove(Key);
            }
            public new bool TryAdd(TKey Key, TValue Value) {
                lock (this) return base.TryAdd(Key, Value);
            }
            public new TValue this[TKey Key] {
                get {
                    lock (this) return base[Key];
                }
                set {
                    lock (this) base[Key] = value;
                }
            }
        }
        /// <summary>A locking dictionary with events that trigger when a key-value pair is added to or removed from the dictionary.</summary>
        public class ReactiveDictionary<TKey, TValue> : LockingDictionary<TKey, TValue> where TKey : notnull {
            public readonly WeakEvent<TKey, TValue> OnSet = new();
            public readonly WeakEvent<TKey> OnRemoved = new();

            public new TValue this[TKey Key] {
                get => base[Key];
                set {
                    TrySetMethodName(Key, value);
                    if (!IsValueAlreadySet(Key, value)) {
                        base[Key] = value;
                        OnSet.Fire(Key, value);
                    }
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
            public new void Add(TKey Key, TValue Value) {
                TrySetMethodName(Key, Value);
                if (!IsValueAlreadySet(Key, Value)) {
                    lock (this) base.Add(Key, Value);
                    OnSet.Fire(Key, Value);
                }
            }
            public new bool Remove(TKey Key) {
                bool Success;
                lock (this) Success = Remove(Key, out _);
                if (Success) OnRemoved.Fire(Key);
                return Success;
            }
            
            bool IsValueAlreadySet(TKey Key, TValue Value) {
                bool Exists = TryGetValue(Key, out TValue? CurrentValue);
                return Exists && Equals(CurrentValue, Value);
            }
            static void TrySetMethodName(TKey Key, TValue Value) {
                if (Key is string MethodName && Value is Method Method) {
                    Method.Name = MethodName;
                }
            }
        }
        public class Cache<TKey, TValue> where TKey : notnull {
            public const int Limit = 50_000;
            private readonly LockingDictionary<TKey, TValue> CacheDictionary = new();
            private readonly Queue<TKey> Keys = new();
            public TValue Store(TKey Key, TValue Value) {
                lock (CacheDictionary) {
                    if (CacheDictionary.Count > Limit) {
                        CacheDictionary.Remove(Keys.Dequeue());
                    }
                    Keys.Enqueue(Key);
                    CacheDictionary.Add(Key, Value);
                    return Value;
                }
            }
            public TValue? this[TKey Key] {
                get {
                    CacheDictionary.TryGetValue(Key, out TValue? Value);
                    return Value;
                }
            }
        }
    }
}
