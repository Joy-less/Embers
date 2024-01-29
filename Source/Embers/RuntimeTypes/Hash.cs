using System.Collections;
using System.Collections.Generic;

namespace Embers {
    public sealed class Hash : RubyObject, IEnumerable, IEnumerable<KeyValuePair<Instance, Instance>> {
        public readonly IDictionary<Instance, Instance> Inner;
        public readonly Instance DefaultValue;
        
        public Hash(CodeLocation location, int capacity = 0, Instance? default_value = null) : base(location) {
            Inner = Axis.CreateInstanceDictionary(capacity);
            DefaultValue = default_value ?? Axis.Nil;
        }
        public Hash(CodeLocation location, IDictionary<Instance, Instance> original, Instance? default_value = null) : base(location) {
            Inner = Axis.CreateInstanceDictionary(original.Count);
            foreach (KeyValuePair<Instance, Instance> Entry in original) {
                Inner[Entry.Key] = Entry.Value;
            }
            DefaultValue = default_value ?? Axis.Nil;
        }

        public Instance this[Instance Key] {
            get {
                if (Inner.TryGetValue(Key, out Instance? Instance)) {
                    return Instance!;
                }
                else {
                    return DefaultValue;
                }
            }
            set {
                Inner[Key] = value;
            }
        }
        public bool Remove(Instance Key) {
            return Inner.Remove(Key);
        }
        public bool HasKey(Instance Instance) {
            return Inner.ContainsKey(Instance);
        }
        public Hash Invert() {
            Hash Inverted = new(Location, Count, DefaultValue);
            foreach (KeyValuePair<Instance, Instance> Entry in this) {
                Inverted[Entry.Value] = Entry.Key;
            }
            return Inverted;
        }
        public void Clear() {
            Inner.Clear();
        }
        public Hash Clone() {
            return new Hash(Location, Inner, DefaultValue);
        }
        public int Count => Inner.Count;
        public override string ToString() => "hash";
        public IEnumerator<KeyValuePair<Instance, Instance>> GetEnumerator() => Inner.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Inner.GetEnumerator();

        internal class InstanceEqualityComparer : IEqualityComparer<Instance> {
            public readonly Axis Axis;
            public InstanceEqualityComparer(Axis axis) {
                Axis = axis;
            }
            public int GetHashCode(Instance Instance) {
                return Instance.Hash();
            }
            public bool Equals(Instance? First, Instance? Second) {
                if (First is null) return Second is null;
                if (Second is null) return First is null;
                return First.CallMethod("eql?", Second).Truthy;
            }
        }
    }
}
