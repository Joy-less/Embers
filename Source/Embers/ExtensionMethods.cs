using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using static Embers.Phase2;
using static Embers.Script;

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
            string Serialised = $"new List<{typeof(T).PathTo()}>() {{";
            bool IsFirst = true;
            foreach (T Item in List) {
                if (IsFirst) IsFirst = false;
                else Serialised += ", ";
                Serialised += Item.Serialise();
            }
            return Serialised + "}";
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
        public static string Serialise<T>(this Dictionary<T, T> Dictionary) where T : Phase2Object {
            string Serialised = $"new Dictionary<{typeof(T).PathTo()}, {typeof(T).PathTo()}>() {{";
            bool IsFirst = true;
            foreach (KeyValuePair<T, T> Item in Dictionary) {
                if (IsFirst) IsFirst = false;
                else Serialised += ", ";
                Serialised += "{" + Item.Key.Serialise() + ", " + Item.Value.Serialise() + "}, ";
            }
            return Serialised + "}";
        }
        public static string Inspect<T>(this Dictionary<T, T>? Dictionary, string Separator = ", ") where T : Phase2Object {
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
        public static string InspectInstances<T>(this Dictionary<T, T>? Dictionary, string Separator = ", ") where T : Instance {
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
        public static void CopyTo<TKey, TValue>(this Dictionary<TKey, TValue> Origin, Dictionary<TKey, TValue> Target) where TKey : notnull {
            foreach (KeyValuePair<TKey, TValue> Pair in Origin) {
                Target[Pair.Key] = Pair.Value;
            }
        }
        public static void CopyTo<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> Origin, Dictionary<TKey, TValue> Target) where TKey : notnull {
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
        public static Dictionary<T, T> ListAsHash<T>(this List<T> HashItemsList) where T : notnull {
            Dictionary<T, T> HashItems = new();
            for (int i2 = 0; i2 < HashItemsList.Count; i2 += 2) {
                HashItems[HashItemsList[i2]] = HashItemsList[i2 + 1];
            }
            return HashItems;
        }
        public static string PathTo(this object Self) => PathTo(Self.GetType());
        public static string PathTo(this Type Self) => (Self.FullName ?? "").Replace('+', '.');
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
        public static long ParseLong(this string String) {
            return long.Parse(String.ToString());
        }
        public static double ParseDouble(this string String) {
            return double.Parse(String.ToString());
        }
        public static long ParseHexLong(this string String) {
            return long.Parse(String.ToString(), System.Globalization.NumberStyles.HexNumber);
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
        #if !NET6_0_OR_GREATER
            public static Task WaitForExitAsync(this Process Process) {
                if (Process.HasExited) return Task.CompletedTask;

                TaskCompletionSource<object?> TCS = new();
                Process.EnableRaisingEvents = true;
                Process.Exited += (sender, args) => TCS.TrySetResult(null);

                return Process.HasExited ? Task.CompletedTask : TCS.Task;
            }
        #endif
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
