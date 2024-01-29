namespace System.Collections.Generic {
    public class SynchronizedCollection<T> : IList<T>, IList {
        public readonly object Sync = new();

        private readonly List<T> Items;

        public SynchronizedCollection() {
            Items = new List<T>();
        }
        public SynchronizedCollection(int Capacity) {
            Items = new List<T>(Capacity);
        }
        public SynchronizedCollection(IEnumerable<T> List) {
            Items = new List<T>(List);
        }
        
        public int Count {
            get { lock (Sync) return Items.Count; }
        }
        public T this[int Index] {
            get {
                lock (Sync) {
                    return Items[Index];
                }
            }
            set {
                lock (Sync) {
                    Items[Index] = value;
                }
            }
        }
        public void Add(T Item) {
            lock (Sync) {
                Items.Add(Item);
            }
        }
        public void Clear() {
            lock (Sync) {
                Items.Clear();
            }
        }
        public bool Contains(T Item) {
            lock (Sync) {
                return Items.Contains(Item);
            }
        }
        public void CopyTo(T[] Array, int Index) {
            lock (Sync) {
                Items.CopyTo(Array, Index);
            }
        }
        public IEnumerator<T> GetEnumerator() {
            lock (Sync) {
                return Items.GetEnumerator();
            }
        }
        public int IndexOf(T Item) {
            lock (Sync) {
                int Count = Items.Count;
                for (int i = 0; i < Count; i++) {
                    if (Equals(Items[i], Item)) {
                        return i;
                    }
                }
                return -1;
            }
        }
        public void Insert(int Index, T Item) {
            lock (Sync) {
                Items.Insert(Index, Item);
            }
        }
        public bool Remove(T Item) {
            lock (Sync) {
                return Items.Remove(Item);
            }
        }
        public void RemoveAt(int Index) {
            lock (Sync) {
                Items.RemoveAt(Index);
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        bool ICollection<T>.IsReadOnly => false;
        bool ICollection.IsSynchronized => true;
        object ICollection.SyncRoot => Sync;
        void ICollection.CopyTo(Array Array, int Index) {
            lock (Sync) {
                ((IList)Items).CopyTo(Array, Index);
            }
        }
        object? IList.this[int Index] {
            get => this[Index];
            set => this[Index] = (T)value!;
        }
        bool IList.IsReadOnly => false;
        bool IList.IsFixedSize => false;
        int IList.Add(object? Value) {
            lock (Sync) {
                Add((T)Value!);
                return Count - 1;
            }
        }
        bool IList.Contains(object? Value) {
            return Contains((T)Value!);
        }
        int IList.IndexOf(object? Value) {
            return IndexOf((T)Value!);
        }
        void IList.Insert(int Index, object? Value) {
            Insert(Index, (T)Value!);
        }
        void IList.Remove(object? Value) {
            Remove((T)Value!);
        }
    }
}