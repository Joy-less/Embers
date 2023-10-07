using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using static Embers.Phase2;
using static Embers.Script;
#if !NET6_0_OR_GREATER
    using System.Threading.Tasks;
    using System.Diagnostics;
#endif
#if !NET7_0_OR_GREATER
    using System.Runtime.CompilerServices;
#endif

#nullable enable

namespace Embers
{
    public static class ExtensionMethods {
        public static List<T> GetIndexRange<T>(this List<T> List, int StartIndex, int EndIndex) {
            if (StartIndex > EndIndex)
                return new List<T>();
            return List.GetRange(StartIndex, EndIndex - StartIndex + 1);
        }
        public static List<T> GetIndexRange<T>(this List<T> List, int StartIndex) {
            int EndIndex = List.Count - 1;
            if (StartIndex > EndIndex)
                return new List<T>();
            return List.GetRange(StartIndex, EndIndex - StartIndex + 1);
        }
        public static void RemoveIndexRange<T>(this List<T> List, int StartIndex, int EndIndex) {
            if (StartIndex <= EndIndex)
                List.RemoveRange(StartIndex, EndIndex - StartIndex + 1);
        }
        public static void RemoveIndexRange<T>(this List<T> List, int StartIndex) {
            int EndIndex = List.Count - 1;
            if (StartIndex <= EndIndex)
                List.RemoveRange(StartIndex, EndIndex - StartIndex + 1);
        }
        public static void RemoveFromEnd<T>(this List<T> List, Func<T, bool> While, bool RemoveOneOnly = false) {
            for (int i = List.Count - 1; i >= 0; i--) {
                if (While(List[i])) {
                    List.RemoveAt(i);
                    if (RemoveOneOnly) break;
                }
                else {
                    break;
                }
            }
        }
        public static string Serialise<T>(this List<T> List) where T : Phase2Object {
            string Serialised = $"new {typeof(List<T>).GetPath()}() {{";
            bool IsFirst = true;
            foreach (T Item in List) {
                if (IsFirst) IsFirst = false;
                else Serialised += ", ";
                Serialised += Item.Serialise();
            }
            return Serialised + "}";
        }
        public static string Serialise(this string? String) {
            return String != null ? '"' + String.Replace("\"", "\\\"") + '"' : "null";
        }
        public static string Inspect<T>(this List<T>? List, string Separator = ", ") where T : Phase2Object {
            string ListInspection = "";
            if (List != null) {
                foreach (Phase2Object Object in List) {
                    if (ListInspection.Length != 0)
                        ListInspection += Separator;
                    ListInspection += Object.Inspect();
                }
            }
            return ListInspection;
        }
        public static string InspectInstances<T>(this List<T>? List, string Separator = ", ") where T : Instance {
            string ListInspection = "";
            if (List != null) {
                foreach (Instance Object in List) {
                    if (ListInspection.Length != 0)
                        ListInspection += Separator;
                    ListInspection += Object.Inspect();
                }
            }
            return ListInspection;
        }
        public static string LightInspectInstances<T>(this List<T>? List, string Separator = ", ") where T : Instance {
            string ListInspection = "";
            if (List != null) {
                foreach (Instance Object in List) {
                    if (ListInspection.Length != 0)
                        ListInspection += Separator;
                    ListInspection += Object.LightInspect();
                }
            }
            return ListInspection;
        }
        public static string Serialise<T>(this LockingDictionary<T, T> Dictionary) where T : Phase2Object {
            string Serialised = $"new {typeof(LockingDictionary<T, T>).GetPath()}() {{";
            bool IsFirst = true;
            foreach (KeyValuePair<T, T> Item in Dictionary) {
                if (IsFirst) IsFirst = false;
                else Serialised += ", ";
                Serialised += "{" + Item.Key.Serialise() + ", " + Item.Value.Serialise() + "}, ";
            }
            return Serialised + "}";
        }
        public static string Inspect<T>(this LockingDictionary<T, T>? Dictionary, string Separator = ", ") where T : Phase2Object {
            string DictionaryInspection = "";
            if (Dictionary != null) {
                foreach (KeyValuePair<T, T> Object in Dictionary) {
                    if (DictionaryInspection.Length != 0)
                        DictionaryInspection += Separator;
                    DictionaryInspection += $"{Object.Key.Inspect()} => {Object.Value.Inspect()}";
                }
            }
            return DictionaryInspection;
        }
        public static string InspectInstances<T>(this LockingDictionary<T, T>? Dictionary, string Separator = ", ") where T : Instance {
            string DictionaryInspection = "";
            if (Dictionary != null) {
                foreach (KeyValuePair<T, T> Object in Dictionary) {
                    if (DictionaryInspection.Length != 0)
                        DictionaryInspection += Separator;
                    DictionaryInspection += $"{Object.Key.Inspect()} => {Object.Value.Inspect()}";
                }
            }
            return DictionaryInspection;
        }
        public static DebugLocation Location<T>(this List<T> List) where T : Phase2Object {
            if (List.Count != 0)
                return List[0].Location;
            else
                return new DebugLocation();
        }
        public static void CopyTo<TKey, TValue>(this LockingDictionary<TKey, TValue> Origin, LockingDictionary<TKey, TValue> Target) where TKey : notnull {
            foreach (KeyValuePair<TKey, TValue> Pair in Origin) {
                Target[Pair.Key] = Pair.Value;
            }
        }
        public static void CopyTo<T>(this Stack<T> Origin, Stack<T> Target) {
            foreach (T Value in Origin) {
                Target.Push(Value);
            }
        }
        public static void ReplaceContentsWith<T>(this Stack<T> Stack, Stack<T> With) {
            Stack.Clear();
            With.CopyTo(Stack);
        }
        public static void ReplaceContentsWith<T>(this Stack<T> Stack, T[] With) {
            Stack.Clear();
            for (int i = With.Length - 1; i >= 0; i--) {
                Stack.Push(With[i]);
            }
        }
        public static void EnsureArrayIndex<T>(this List<T> List, Interpreter Interpreter, int Index) where T : Instance {
            int Count = Index + 1;
            List.EnsureCapacity(Count);
            for (int i = List.Count; i < Count; i++) {
                List.Add((T)(object)Interpreter.Nil);
            }
        }
        public static LockingDictionary<T, T> ListAsHash<T>(this List<T> HashItemsList) where T : notnull {
            LockingDictionary<T, T> HashItems = new();
            for (int i2 = 0; i2 < HashItemsList.Count; i2 += 2) {
                HashItems[HashItemsList[i2]] = HashItemsList[i2 + 1];
            }
            return HashItems;
        }
        public static LockingDictionary<TKey, TValue> ToLockingDictionary<TKey, TValue>(this LockingDictionary<TKey, TValue> Dict, Func<KeyValuePair<TKey, TValue>, TKey> KeySelector, Func<KeyValuePair<TKey, TValue>, TValue> ValueSelector) where TKey : notnull {
            LockingDictionary<TKey, TValue> ConcurrentDict = new();
            foreach (KeyValuePair<TKey, TValue> KVP in Dict) {
                ConcurrentDict[KeySelector(KVP)] = ValueSelector(KVP);
            }
            return ConcurrentDict;
        }
        public static Method CloneTo(this Method Method, Module Target) {
            Method MethodClone = Method.Clone();
            MethodClone.ChangeParent(Target);
            return MethodClone;
        }
        public static void CloneTo(this Dictionary<string, Method> From, Dictionary<string, Method> To, Module TargetParent) {
            foreach (KeyValuePair<string, Method> Method in From) {
                To[Method.Key] = Method.Value.CloneTo(TargetParent);
            }
        }
        public static string GetPath(this object Self) {
            return GetPath(Self.GetType()) + "." + Self;
        }
        public static string GetPath(this Type Self) {
            StringBuilder Result = new();
            StringBuilder BuildPath(Type CurrentType, List<Type>? TypeArguments = null) {
                TypeArguments ??= CurrentType.GetGenericArguments().ToList();
                int TypeArgumentCount = CurrentType.IsGenericType ? CurrentType.GetGenericArguments().Length : 0;
                if (CurrentType.IsNested) {
                    BuildPath(CurrentType.DeclaringType!, TypeArguments);
                }
                else {
                    Result.Append(CurrentType.Namespace);
                }
                Result.Append('.');
                Result.Append(CurrentType.Name.Split('`')[0]);
                if (TypeArgumentCount > 0) {
                    Result.Append('<');
                    for (int i = 0; i < TypeArgumentCount; i++) {
                        if (i != 0) Result.Append(',');
                        BuildPath(TypeArguments[i]);
                    }
                    Result.Append('>');
                }
                return Result;
            }
            BuildPath(Self);
            return Result.ToString();
        }
        public static string ReplaceFirst(this string Original, string Replace, string With) {
            int FoundIndex = Original.IndexOf(Replace);
            if (FoundIndex != -1)
                return Original.Remove(FoundIndex, Replace.Length).Insert(FoundIndex, With);
            else
                return Original;
        }
        public static bool IsAsciiDigit(this char Chara) {
            return Chara >= '0' && Chara <= '9';
        }
        public static bool IsAsciiLetterUpper(this char Chara) {
            return Chara >= 'A' && Chara <= 'Z';
        }
        public static bool IsSmall(this long Long) {
            return long.MinValue / 2.0 < Long && Long < long.MaxValue / 2.0;
        }
        public static bool IsSmall(this double Double) {
            return long.MinValue / 2.0 < Double && Double < long.MaxValue / 2.0 || double.IsInfinity(Double);
        }
        public static bool IsSmall(this BigInteger BigInteger) {
            return (BigFloat)(long.MinValue / 2.0) < BigInteger && BigInteger < (BigFloat)(long.MaxValue / 2.0);
        }
        public static bool IsSmall(this BigFloat BigFloat) {
            return (BigFloat)(long.MinValue / 2.0) < BigFloat && BigFloat < (BigFloat)(long.MaxValue / 2.0);
        }
        public static Integer ParseInteger(this string String) {
            if (long.TryParse(String, out long Result) && Result.IsSmall()) {
                return new Integer(Result);
            }
            else {
                return new Integer(BigInteger.Parse(String));
            }
        }
        public static Float ParseFloat(this string String) {
            if (double.TryParse(String, out double Result) && Result.IsSmall()) {
                return new Float(Result);
            }
            else {
                return new Float(BigFloat.Parse(String));
            }
        }
        public static Integer ParseHexInteger(this string String) {
            if (long.TryParse(String, System.Globalization.NumberStyles.HexNumber, null, out long Result)) {
                return new Integer(Result);
            }
            else {
                return new Integer(BigInteger.Parse(String, System.Globalization.NumberStyles.HexNumber));
            }
        }
        public static long ToUnixTimeSeconds(this DateTimeOffset DateTimeOffset) {
            return DateTimeOffset.ToUnixTimeSeconds();
        }
        public static double ToUnixTimeSecondsDouble(this DateTimeOffset DateTimeOffset) {
            return DateTimeOffset.ToUnixTimeSeconds() + (DateTimeOffset.Ticks % TimeSpan.TicksPerSecond) / (double)TimeSpan.TicksPerSecond;
        }

        //
        // Compatibility
        //
        #if !NET5_0_OR_GREATER
            public static long NextInt64(this Random Random) {
                byte[] Buffer = new byte[8];
                Random.NextBytes(Buffer);
                long result = BitConverter.ToInt64(Buffer, 0);
                return result;
            }
            public static long NextInt64(this Random Random, long IncludingMin, long ExcludingMax) {
                if (IncludingMin > ExcludingMax)
                    throw new ArgumentException("MinValue should be less than or equal to MaxValue");

                // Calculate the range of possible values
                long Range = ExcludingMax - IncludingMin;

                if (Range <= 0)
                    throw new ArgumentException("Range must be greater than zero");

                // Generate a random value within the specified range
                byte[] Buffer = new byte[8];
                Random.NextBytes(Buffer);
                long Result = BitConverter.ToInt64(Buffer, 0);

                // Make sure the result is non-negative and within the range
                Result = Math.Abs(Result % Range) + IncludingMin;

                return Result;
            }
        #endif
        #if !NET6_0_OR_GREATER
            public static Task WaitForExitAsync(this Process Process) {
                if (Process.HasExited) return Task.CompletedTask;

                TaskCompletionSource<object?> TCS = new();
                Process.EnableRaisingEvents = true;
                Process.Exited += (Sender, Args) => TCS.TrySetResult(null);

                return Process.HasExited ? Task.CompletedTask : TCS.Task;
            }
        #endif
        #if !NET7_0_OR_GREATER
            public static bool TryAdd<T1, T2>(this ConditionalWeakTable<T1, T2> Table, T1 Key, T2 Value) where T1 : class where T2 : class {
                if (Table.TryGetValue(Key, out _)) {
                    return false;
                }
                else {
                    Table.Add(Key, Value);
                    return true;
                }
            }
        #endif
    }
}
