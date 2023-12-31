﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using static Embers.Phase2;
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
        public static void RemoveFromEndWhile<T>(this List<T> List, Func<T, bool> While, bool RemoveOneOnly = false) {
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
                    ListInspection += Object is ArrayInstance ? Object.Array.LightInspectInstances(Separator) : Object.LightInspect();
                }
            }
            return ListInspection;
        }
        public static string InspectHash(this HashDictionary? Hash, string Separator = ", ") {
            string HashInspection = "";
            if (Hash != null) {
                foreach (KeyValuePair<Instance, Instance> Match in Hash.KeyValues) {
                    if (HashInspection.Length != 0)
                        HashInspection += Separator;
                    HashInspection += $"{Match.Key.Inspect()} => {Match.Value.Inspect()}";
                }
            }
            return HashInspection;
        }
        public static bool TryFindMethod(this LockingDictionary<string, Method> MethodsDict, string MethodName, out Method? Method) {
            if (MethodsDict.TryGetValue(MethodName, out Method)) {
                return true;
            }
            else if (MethodsDict.TryGetValue("method_missing", out Method)) {
                return true;
            }
            return false;
        }
        public static string Serialise<T>(this LockingDictionary<T, T> Dictionary) where T : Phase2Object {
            string Serialised = $"new {typeof(LockingDictionary<T, T>).GetPath()}() {{";
            bool IsFirst = true;
            foreach (KeyValuePair<T, T> Item in Dictionary) {
                if (IsFirst) IsFirst = false;
                else Serialised += ", ";
                Serialised += "{" + Item.Key.Serialise() + ", " + Item.Value.Serialise() + "}";
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
        public static DebugLocation Location<T>(this List<T> List) where T : Phase2Object {
            if (List.Count != 0)
                return List[0].Location;
            else
                return new DebugLocation();
        }
        public static void CopyTo<TKey, TValue>(this Dictionary<TKey, TValue> Origin, Dictionary<TKey, TValue> Target) where TKey : notnull {
            Target.EnsureCapacity(Origin.Count);
            foreach (KeyValuePair<TKey, TValue> Pair in Origin) {
                Target[Pair.Key] = Pair.Value;
            }
        }
        public static void ReplaceContentsWith<T>(this Stack<T> Stack, T[] With) {
            Stack.Clear();
            Stack.EnsureCapacity(With.Length);
            for (int i = With.Length - 1; i >= 0; i--) {
                Stack.Push(With[i]);
            }
        }
        public static void EnsureArrayIndex<T>(this List<T> List, Api Api, int Index) where T : Instance {
            int Count = Index + 1;
            List.EnsureCapacity(Count);
            for (int i = List.Count; i < Count; i++) {
                List.Add((T)(Instance)Api.Nil);
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
            LockingDictionary<TKey, TValue> LockingDict = new();
            foreach (KeyValuePair<TKey, TValue> KVP in Dict) {
                LockingDict[KeySelector(KVP)] = ValueSelector(KVP);
            }
            return LockingDict;
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
        public static bool IsConstantIdentifier(this string String) {
            return String.Length != 0 && String[0].IsAsciiLetterUpper();
        }
        public static bool IsSmall(this long Long) {
            return long.MinValue / 2 < Long && Long < long.MaxValue / 2;
        }
        public static bool IsSmall(this double Double) {
            return (long.MinValue / 2 < Double && Double < long.MaxValue / 2) || !double.IsFinite(Double);
        }
        public static bool IsSmall(this BigInteger BigInteger) {
            return long.MinValue / 2 < BigInteger && BigInteger < long.MaxValue / 2;
        }
        public static bool IsSmall(this BigFloat BigFloat) {
            return long.MinValue / 2 < BigFloat && BigFloat < long.MaxValue / 2;
        }
        public static DynInteger ParseInteger(this string String) {
            if (long.TryParse(String, out long Result) && Result.IsSmall()) {
                return new DynInteger(Result);
            }
            else {
                return new DynInteger(BigInteger.Parse(String));
            }
        }
        public static DynFloat ParseFloat(this string String) {
            if (double.TryParse(String, out double Result) && Result.IsSmall()) {
                return new DynFloat(Result);
            }
            else {
                return new DynFloat(BigFloat.Parse(String));
            }
        }
        public static DynInteger ParseHexInteger(this string String) {
            if (long.TryParse(String, System.Globalization.NumberStyles.HexNumber, null, out long Result)) {
                return new DynInteger(Result);
            }
            else {
                return new DynInteger(BigInteger.Parse(String, System.Globalization.NumberStyles.HexNumber));
            }
        }
        public static double ToUnixTimeSecondsDouble(this DateTimeOffset DateTimeOffset) {
            return DateTimeOffset.ToUnixTimeSeconds() + (DateTimeOffset.Ticks % TimeSpan.TicksPerSecond) / (double)TimeSpan.TicksPerSecond;
        }
        public static TItem First<TStack, TItem>(this Stack<TStack> Stack) where TItem : TStack {
            foreach (TStack Item in Stack) {
                if (Item is TItem ItemAsT) return ItemAsT;
            }
            throw new InternalErrorException("Item not found in stack");
        }
        public static async Task QuickSort(this List<Instance> Items, Func<Instance, Instance, Task<bool>> SortBlock) {
            async Task QuickSort(List<Instance> Items, int Low, int High) {
                if (Low < High) {
                    // Choose a pivot
                    int PivotIndex = (Low + High) / 2;
                    Instance Pivot = Items[PivotIndex];
                    // Initialise the pointers
                    int Left = Low;
                    int Right = High;
                    // Check if there are items on either side of the pivot
                    while (Left <= Right) {
                        // Find the first item on the left that should be to the right of the pivot
                        while (await SortBlock(Items[Left], Pivot)) {
                            Left++;
                        }
                        // Find the first item on the right that should be to the left of the pivot
                        while (await SortBlock(Pivot, Items[Right])) {
                            Right--;
                        }
                        // Check if there are items that need to be swapped
                        if (Left <= Right) {
                            // Swap the items to the correct side of the pivot
                            (Items[Left], Items[Right]) = (Items[Right], Items[Left]);
                            Left++;
                            Right--;
                        }
                    }
                    // Sort the sides
                    if (Low < Right) await QuickSort(Items, Low, Right);
                    if (Left < High) await QuickSort(Items, Left, High);
                }
            }
            await QuickSort(Items, 0, Items.Count - 1);
        }
        public static async Task InsertionSort(this List<Instance> Items, Func<Instance, Instance, Task<bool>> SortBlock) {
            for (int i = 1; i < Items.Count; i++) {
                Instance Item = Items[i];
                int i2;
                for (i2 = i - 1; i2 >= 0; i2--) {
                    if (await SortBlock(Item, Items[i2])) {
                        Items[i2 + 1] = Items[i2];
                    }
                    else {
                        break;
                    }
                }
                Items[i2 + 1] = Item;
            }
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
