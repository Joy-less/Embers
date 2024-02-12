using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Embers {
    public static class Extensions {
        public static List<T> GetIndexRange<T>(this List<T> List, int StartIndex, int EndIndex) {
            if (StartIndex > EndIndex) {
                return new List<T>();
            }
            return List.GetRange(StartIndex, EndIndex - StartIndex + 1);
        }
        public static List<T> GetIndexRange<T>(this List<T> List, int StartIndex) {
            return List.GetIndexRange(StartIndex, List.Count - 1);
        }
        public static void RemoveIndexRange<T>(this List<T> List, int StartIndex, int EndIndex) {
            if (StartIndex <= EndIndex) {
                List.RemoveRange(StartIndex, EndIndex - StartIndex + 1);
            }
        }
        public static void RemoveIndexRange<T>(this List<T> List, int StartIndex) {
            List.RemoveIndexRange(StartIndex, List.Count - 1);
        }
        public static string ObjectsToString<T>(this IEnumerable<T>? List, string Separator = ", ", Func<T, string>? Converter = null) {
            Converter ??= Object => Object?.ToString() ?? "null";

            if (List is not null) {
                StringBuilder String = new();
                bool IsFirst = true;
                foreach (T? Object in List) {
                    if (IsFirst) {
                        IsFirst = false;
                    }
                    else {
                        String.Append(Separator);
                    }
                    String.Append(Converter(Object));
                }
                return String.ToString();
            }
            else {
                return string.Empty;
            }
        }
        public static Instance Interpret(this Expression[] Expressions, Context Context) {
            Instance LastInstance = Context.Axis.Nil;
            for (int i = 0; i < Expressions.Length; i++) {
                // Stop if thread is being cancelled
                Context.Locals.Thread?.CancelToken.ThrowIfCancellationRequested();
                // Interpret each expression
                LastInstance = Expressions[i].Interpret(Context);
                // Early exit upon control code
                if (LastInstance is ControlCode) {
                    return LastInstance;
                }
            }
            return LastInstance;
        }
        public static async Task<Instance> InterpretAsync(this Expression[] Expressions, Context Context)
            => await Task.Run(() => Expressions.Interpret(Context));

        public static string ToSnakeCase(this string PascalCase) {
            // Create snake case builder
            StringBuilder SnakeCase = new(PascalCase.Length);
            // Loop each character
            for (int i = 0; i < PascalCase.Length; i++) {
                char Chara = PascalCase[i];
                // Uppercase
                if (char.IsUpper(Chara)) {
                    // Add '_' if character is followed by a single uppercase character
                    if (i != 0 && !(i + 1 < PascalCase.Length && char.IsUpper(PascalCase[i + 1]))) {
                        SnakeCase.Append('_');
                    }
                    // Add character as lowercase
                    SnakeCase.Append(char.ToLower(Chara));
                }
                // Lowercase
                else {
                    // Add character as is
                    SnakeCase.Append(Chara);
                }
            }
            // Return snake case string
            return SnakeCase.ToString();
        }
        public static string ToPascalCase(this string SnakeCaseString) {
            // Create pascal case builder
            StringBuilder PascalBuilder = new(SnakeCaseString.Length);
            // Loop each character
            bool CapitaliseNext = true;
            foreach (char Chara in SnakeCaseString) {
                // Remove '_' and make next character upper
                if (Chara == '_') {
                    CapitaliseNext = true;
                }
                else {
                    // Add letter capitalised if follows '_'
                    if (CapitaliseNext) {
                        PascalBuilder.Append(char.ToUpper(Chara));
                        CapitaliseNext = false;
                    }
                    // Add letter as is
                    else {
                        PascalBuilder.Append(Chara);
                    }
                }
            }
            // Return pascal case string
            return PascalBuilder.ToString();
        }
        public static bool IsNamedLikeConstant(this string String) {
            return String.Length != 0 && String[0].IsAsciiLetterUpper();
        }
        public static bool IsAsciiLetterUpper(this char Chara) {
            return Chara >= 'A' && Chara <= 'Z';
        }
        public static bool IsAsciiLetterLower(this char Chara) {
            return Chara >= 'a' && Chara <= 'z';
        }
        public static bool IsAsciiDigit(this char Chara) {
            return Chara >= '0' && Chara <= '9';
        }
        public static double ToUnixTimeSecondsDouble(this DateTimeOffset DateTimeOffset) {
            return DateTimeOffset.ToUnixTimeSeconds() + (DateTimeOffset.Ticks % TimeSpan.TicksPerSecond) / (double)TimeSpan.TicksPerSecond;
        }
        public static string ReplaceFirst(this string Original, string Replace, string With) {
            int FoundIndex = Original.IndexOf(Replace);
            return FoundIndex != -1 ? Original.Remove(FoundIndex, Replace.Length).Insert(FoundIndex, With) : Original;
        }
        public static List<List<T>> Split<T>(this List<T> List, Func<T, bool> SplitBy, bool RemoveEmptyEntries = false) where T : class? {
            List<List<T>> Partitions = new();
            void Submit(int StartIndex, int EndIndex) {
                if (!RemoveEmptyEntries || StartIndex <= EndIndex) {
                    Partitions.Add(List.GetIndexRange(StartIndex, EndIndex));
                }
            }
            int LastSplitIndex = 0;
            while (true) {
                int SplitIndex = List.FindIndex(LastSplitIndex, Item => SplitBy(Item));
                if (SplitIndex != -1) {
                    Submit(LastSplitIndex, SplitIndex - 1);
                    LastSplitIndex = SplitIndex + 1;
                }
                else {
                    if (LastSplitIndex < List.Count) {
                        Submit(LastSplitIndex, List.Count - 1);
                    }
                    return Partitions;
                }
            }
        }
        public static T[]? TryCast<T>(this IList List) {
            T[] Cast = new T[List.Count];
            for (int i = 0; i < List.Count; i++) {
                if (List[i] is T Item) {
                    Cast[i] = Item;
                }
                else {
                    return null;
                }
            }
            return Cast;
        }
        public static int RemoveDuplicates(this IList<Instance> List) {
            int DuplicateCount = 0;
            HashSet<int> HashSet = new();
            for (int i = 0; i < List.Count; i++) {
                // Get the hash code of the item
                int HashCode = List[i].Hash();
                // Check if an item with the same hash code is already in the list
                if (!HashSet.Add(HashCode)) {
                    // Remove duplicate
                    DuplicateCount++;
                    List.RemoveAt(i);
                    i--;
                }
            }
            return DuplicateCount;
        }
        public static List<TTo> CastTo<TTo>(this ICollection From) {
            List<TTo> List = new(From.Count);
            foreach (object Item in From) {
                List.Add((TTo)Item);
            }
            return List;
        }
        public static void Shuffle<T>(this IList<T> List, Random Random) {
            // Fisher-Yates algorithm
            for (int i = 1; i < List.Count; i++) {
                int new_i = Random.Next(i + 1);
                (List[i], List[new_i]) = (List[new_i], List[i]);
            }
        }
        public static void Sort(this Array Array, Proc? Block) {
            Comparison<Instance> Sort = Block is not null
                ? (A, B) => (int)Block.Call(A, B).CastInteger
                : (A, B) => (int)A.CallMethod("<=>", B).CastInteger;
            Array.Inner.Sort(Sort);
        }
        public static void Sort<T>(this IList<T> list, Comparison<T> comparison) {
            ArrayList.Adapter((IList)list).Sort(new ComparisonComparer<T>(comparison));
        }
        public class ComparisonComparer<T> : IComparer<T>, IComparer {
            private readonly Comparison<T> Comparison;
            public ComparisonComparer(Comparison<T> comparison)
                => Comparison = comparison;
            public int Compare(T? x, T? y)
                => Comparison(x!, y!);
            public int Compare(object? o1, object? o2)
                => Comparison((T)o1!, (T)o2!);
        }
        public static int Count(this Range Range, int CollectionCount) {
            int Start = Range.Start.IsFromEnd ? CollectionCount - Range.Start.Value : Range.Start.Value;
            int End = Range.End.IsFromEnd ? CollectionCount - Range.End.Value : Range.End.Value;
            return End - Start;
        }
        public static Range Clamp(this Range Range, int CollectionCount) {
            Index StartIndex = new(Math.Max(Range.Start.Value, 0), Range.Start.IsFromEnd);
            Index EndIndex = new(Math.Min(Range.End.Value + 1, CollectionCount), Range.End.IsFromEnd);
            return StartIndex..EndIndex;
        }
        public static Range ClampAsRange(this int Index, int CollectionCount) {
            return (Index..Index).Clamp(CollectionCount);
        }
        public static string ReplaceRange(this string String, Range Range, string With) {
            int Start = Range.Start.GetOffset(String.Length);
            int End = Range.End.GetOffset(String.Length);
            return String[..Start] + With + String[End..];
        }
        public static string ProcessEscapeSequences(this string String) {
            return String
                .Replace("\\0", "\0") // null
                .Replace("\\a", "\a") // beep
                .Replace("\\b", "\b") // backspace
                .Replace("\\t", "\t") // tab
                .Replace("\\n", "\n") // newline
                .Replace("\\r", "\r") // carriage return
                .Replace("\\v", "\v") // vertical tab
                .Replace("\\f", "\f") // form feed
                .Replace("\\s", " ") // space
                .Replace("\\\\", "\\") // backslash
                .Replace("\\\"", "\"") // double quote
                .Replace("\\\'", "'"); // single quote
        }

        //
        // Compatibility
        //
#if !NET6_0_OR_GREATER
        public static long NextInt64(this Random Random) {
            byte[] Buffer = new byte[8];
            Random.NextBytes(Buffer);
            return BitConverter.ToInt64(Buffer);
        }
        public static long NextInt64(this Random Random, long IncludingMin, long ExcludingMax) {
            long Range = ExcludingMax - IncludingMin;
            if (Range <= 0) {
                throw new ArgumentException("range must be positive");
            }
            return Math.Abs(NextInt64(Random) % Range) + IncludingMin;
        }
#endif
    }
}
