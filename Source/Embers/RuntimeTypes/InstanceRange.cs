using System;
using System.Collections;
using System.Collections.Generic;

namespace Embers {
    public sealed class InstanceRange : RubyObject, IEnumerable<Integer> {
        public readonly Instance Min;
        public readonly Instance Max;
        public readonly bool ExcludeEnd;
        public InstanceRange(CodeLocation location, Instance? min, Instance? max, bool exclude_end = false) : base(location) {
            Min = min ?? Axis.Nil;
            Max = max ?? Axis.Nil;
            ExcludeEnd = exclude_end;

            // Validate range
            if (Min.Value is not (null or Integer or Float) || Max.Value is not (null or Integer or Float)) {
                throw new RuntimeError($"{Location}: bad limits for range ({this})");
            }
            // Adjust max if exclude end
            if (ExcludeEnd) {
                if (Max.Value is Integer MaxInteger) {
                    Max.Value = MaxInteger - 1;
                }
                else if (Max.Value is Float MaxFloat) {
                    Max.Value = MaxFloat - 1;
                }
            }
        }
        public Integer? Count {
            get {
                if (Max.Value is null || Min.Value is null) {
                    return null;
                }
                return Max.CastInteger - Min.CastInteger + 1;
            }
        }
        public bool IsInRange(Integer Integer) {
            if (Min.Value is not null && Integer < Min.CastInteger) return false;
            if (Max.Value is not null && Integer > Max.CastInteger) return false;
            return true;
        }
        public static explicit operator Range(InstanceRange InstanceRange) {
            return new Range(
                InstanceRange.Min.Value is null ? new Index(0) : new Index(Math.Abs((int)InstanceRange.Min.CastInteger), (int)InstanceRange.Min.CastInteger < 0),
                InstanceRange.Max.Value is null ? new Index(0, true) : new Index(Math.Abs((int)InstanceRange.Max.CastInteger), (int)InstanceRange.Max.CastInteger < 0)
            );
        }
        public IEnumerator<Integer> GetEnumerator() {
            if (Min.Value is null || Max.Value is null) {
                yield break;
            }
            else {
                Integer IntMin = Min.CastInteger;
                Integer IntMax = Max.CastInteger;
                for (Integer Index = IntMin; Index <= IntMax; Index++) {
                    yield return Index;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
        public override string ToString()
            => $"{(Min.Value is not null ? Min.Inspect() : "")}..{(Max.Value is not null ? Max.Inspect() : "")}";
    }
    public sealed class IntRange {
        public readonly int? Min = 0;
        public readonly int? Max = 0;
        public IntRange(int? min = null, int? max = null) {
            Min = min;
            Max = max;
        }
        public Integer? Count {
            get {
                if (Max is null || Min is null) {
                    return null;
                }
                return Max - Min + 1;
            }
        }
        public bool IsInRange(int Number) {
            if (Min is not null && Number < Min) return false;
            if (Max is not null && Number > Max) return false;
            return true;
        }
        public override string ToString()
            => (Min?.ToString() ?? "") + ".." + (Max?.ToString() ?? "");
    }
}
