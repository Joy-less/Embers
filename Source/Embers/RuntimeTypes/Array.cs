using System.Collections;
using System.Collections.Generic;

namespace Embers {
    public sealed class Array : RubyObject, IEnumerable<Instance> {
        public readonly IList<Instance> Inner;
        public readonly Instance DefaultValue;
        
        public Array(CodeLocation location, int capacity = 0, Instance? default_value = null) : base(location) {
            Inner = Axis.CreateInstanceList(capacity);
            DefaultValue = default_value ?? Axis.Nil;
        }
        public Array(CodeLocation location, IList<Instance> original, Instance? default_value = null) : base(location) {
            Inner = Axis.CreateInstanceList(original.Count);
            foreach (Instance Item in original) {
                Inner.Add(Item);
            }
            DefaultValue = default_value ?? Axis.Nil;
        }

        public Instance this[int Index] {
            get {
                Index = AbsoluteIndex(Index);
                if (Index >= 0 && Index < Inner.Count) {
                    return Inner[Index];
                }
                else {
                    return DefaultValue;
                }
            }
            set {
                EnsureIndex(Index);
                Inner[Index] = value;
            }
        }
        public void Add(Instance Instance) {
            Inner.Add(Instance);
        }
        public void Insert(int Index, Instance Instance) {
            Index = AbsoluteIndex(Index);
            EnsureIndex(Index);
            Inner.Insert(Index, Instance);
        }
        public void InsertRange(int Index, IEnumerable<Instance> Instances) {
            foreach (Instance Instance in Instances) {
                Insert(Index, Instance);
            }
        }
        public bool Remove(Instance Instance) {
            return Inner.Remove(Instance);
        }
        public void RemoveAt(int Index) {
            Index = AbsoluteIndex(Index);
            if (Index < Count) {
                Inner.RemoveAt(Index);
            }
        }
        public void EnsureIndex(int Index) {
            Index = AbsoluteIndex(Index);
            while (Index >= Inner.Count) {
                Inner.Add(DefaultValue);
            }
        }
        public void Clear() {
            Inner.Clear();
        }
        public Array Clone() {
            return new Array(Location, Inner, DefaultValue);
        }
        public int Count => Inner.Count;
        public override string ToString() => "array";
        public IEnumerator<Instance> GetEnumerator() => Inner.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Inner.GetEnumerator();

        private int AbsoluteIndex(int Index) {
            // Convert index (-1) to (array.count - 1)
            return Index >= 0 ? Index : Count + Index;
        }
    }
}
