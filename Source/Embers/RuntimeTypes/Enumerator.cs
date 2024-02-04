using System;
using System.Collections;
using System.Collections.Generic;

namespace Embers {
    public sealed class Enumerator : IEnumerator<Instance>, IEnumerable<Instance> {
        private readonly IEnumerable<Instance> Original;
        private IEnumerator<Instance> Inner;
        private Instance? CurrentPeek;

        public Enumerator(IEnumerable<Instance> inner) {
            Original = inner;
            Inner = Original.GetEnumerator();
        }
        public Enumerator(Context context, IEnumerable inner) {
            IEnumerable<Instance> CreateEnumerator() {
                foreach (object Item in inner) {
                    yield return Adapter.GetInstance(context, Item);
                }
            }
            Original = CreateEnumerator();
            Inner = Original.GetEnumerator();
        }
        public Enumerator(IEnumerator<Instance> inner) {
            IEnumerable<Instance> CreateEnumerator() {
                while (inner.MoveNext()) {
                    yield return inner.Current;
                }
            }
            Original = CreateEnumerator();
            Inner = Original.GetEnumerator();
        }
        public Enumerator(Context context, IEnumerator inner) {
            IEnumerable<Instance> CreateEnumerator() {
                while (inner.MoveNext()) {
                    yield return Adapter.GetInstance(context, inner.Current);
                }
            }
            Original = CreateEnumerator();
            Inner = Original.GetEnumerator();
        }
        public Enumerator(CodeLocation location, Integer min, Integer max, Integer step) {
            IEnumerable<Instance> CreateEnumerator() {
                for (Integer Index = min; Index < max; Index += step) {
                    yield return new Instance(location.Axis.Integer, Index);
                }
            }
            Original = CreateEnumerator();
            Inner = Original.GetEnumerator();
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
        public void Rewind() {
            Inner = Original.GetEnumerator();
            CurrentPeek = null;
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
        void IEnumerator.Reset() => Rewind();
        IEnumerator<Instance> IEnumerable<Instance>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }
}
