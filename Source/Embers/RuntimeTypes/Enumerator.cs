using System;
using System.Collections;
using System.Collections.Generic;

namespace Embers {
    public sealed class Enumerator : IEnumerator<Instance> {
        private readonly IEnumerator<Instance> Original;
        private Instance? CurrentPeek;

        public Enumerator(IEnumerator<Instance> original) {
            Original = original;
        }
        public Enumerator(IEnumerable<Instance> original) {
            Original = original.GetEnumerator();
        }
        public Enumerator(Context context, IEnumerable original) {
            IEnumerator<Instance> CreateEnumerator() {
                foreach (object Item in original) {
                    yield return Adapter.GetInstance(context, Item);
                }
            }
            Original = CreateEnumerator();
        }
        public Enumerator(CodeLocation location, Integer min, Integer max, Integer step) {
            IEnumerator<Instance> CreateEnumerator() {
                for (Integer Index = min; Index < max; Index += step) {
                    yield return new Instance(location.Axis.Integer, Index);
                }
            }
            Original = CreateEnumerator();
        }

        public bool MoveNext() {
            if (CurrentPeek is not null) {
                CurrentPeek = null;
                return true;
            }
            else {
                return Original.MoveNext();
            }
        }
        public void Reset() {
            Original.Reset();
            CurrentPeek = null;
        }
        public void Dispose() {
            Original.Dispose();
            GC.SuppressFinalize(this);
        }
        public bool Peek(out Instance? Item) {
            if (CurrentPeek is not null) {
                Item = CurrentPeek;
                return true;
            }
            else if (Original.MoveNext()) {
                CurrentPeek = Original.Current;
                Item = CurrentPeek;
                return true;
            }
            else {
                Item = null;
                return false;
            }
        }
        public Instance Current => Original.Current;
        object? IEnumerator.Current => Current;
        public override string ToString()
            => $"#<Enumerator:0x{GetHashCode():x16}>";
    }
}
