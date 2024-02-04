using System;
using System.Collections;
using System.Collections.Generic;

namespace Embers {
    public sealed class Enumerator : IEnumerator<Instance>, IEnumerable<Instance> {
        private readonly IEnumerator<Instance> Inner;
        private Instance? CurrentPeek;

        public Enumerator(IEnumerator<Instance> inner) {
            Inner = inner;
        }
        public Enumerator(IEnumerable<Instance> inner) {
            Inner = inner.GetEnumerator();
        }
        public Enumerator(Context context, IEnumerator inner) {
            IEnumerator<Instance> CreateEnumerator() {
                while (inner.MoveNext()) {
                    yield return Adapter.GetInstance(context, inner.Current);
                }
            }
            Inner = CreateEnumerator();
        }
        public Enumerator(Context context, IEnumerable inner) {
            IEnumerator<Instance> CreateEnumerator() {
                foreach (object Item in inner) {
                    yield return Adapter.GetInstance(context, Item);
                }
            }
            Inner = CreateEnumerator();
        }
        public Enumerator(CodeLocation location, Integer min, Integer max, Integer step) {
            IEnumerator<Instance> CreateEnumerator() {
                for (Integer Index = min; Index < max; Index += step) {
                    yield return new Instance(location.Axis.Integer, Index);
                }
            }
            Inner = CreateEnumerator();
        }
        public bool MoveNext() {
            if (CurrentPeek is not null) {
                CurrentPeek = null;
                return true;
            }
            else {
                return Inner.MoveNext();
            }
        }
        public Instance? Peek() {
            if (CurrentPeek is not null) {
                return CurrentPeek;
            }
            else if (Inner.MoveNext()) {
                CurrentPeek = Inner.Current;
                return CurrentPeek;
            }
            else {
                return null;
            }
        }
        public void Dispose() {
            Inner.Dispose();
            GC.SuppressFinalize(this);
        }
        public Instance Current => Inner.Current;
        public override string ToString()
            => $"#<Enumerator:0x{GetHashCode():x16}>";

        object? IEnumerator.Current => Current;
        void IEnumerator.Reset() => Inner.Reset();
        IEnumerator<Instance> IEnumerable<Instance>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }
}
