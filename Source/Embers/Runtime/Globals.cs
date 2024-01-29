using System;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;

namespace Embers {
    public sealed class Globals {
        public readonly Axis Axis;

        public Random Random;
        public Integer RandomSeed;

        public readonly ConcurrentDictionary<string, Instance> GlobalVariables = new();

        private readonly ConcurrentDictionary<string, Instance> ImmortalSymbols = new();
        private readonly ConcurrentDictionary<string, Instance> MortalSymbols = new();

        private long LastObjectId;

        internal Globals(Axis axis) {
            Axis = axis;

            RandomSeed = new Random().NextInt64();
            Random = new Random(RandomSeed.GetHashCode());
        }

        public Instance GetImmortalSymbol(string Value) {
            return ImmortalSymbols.GetOrAdd(Value, Key => new Instance(Axis.Symbol, Value));
        }
        public Instance GetMortalSymbol(string Value) {
            // Try reuse symbol
            return MortalSymbols.GetOrAdd(Value, Key => {
                // Check symbol overflow
                if (MortalSymbols.Count >= Axis.Options.MaxMortalSymbols) {
                    // Kill random symbol
                    MortalSymbols.TryRemove(MortalSymbols.Keys.First(), out _);
                }
                // Create new symbol
                return new Instance(Axis.Symbol, Value);
            });
        }

        internal long NewObjectId() {
            return Interlocked.Increment(ref LastObjectId);
        }
    }
}
