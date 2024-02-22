using System;
using System.Threading;
using System.Threading.Tasks;

namespace Embers {
    public sealed class Thread : RubyObject {
        public readonly Action<Thread> Action;
        public readonly Task Task;
        public readonly CancellationToken CancelToken;

        private readonly CancellationTokenSource CancelTokenSource;

        public Thread(CodeLocation location, Action<Thread> action) : base(location) {
            Action = action;
            CancelTokenSource = new CancellationTokenSource();
            CancelToken = CancelTokenSource.Token;

            // Run task
            Task = Task.Run(() => {
                // Run action in background
                Action(this);
                // Dispose cancel token source
                CancelTokenSource.Dispose();
            }, CancelTokenSource.Token);
        }
        public void Stop() {
            // Cancel task
            try {
                CancelTokenSource.Cancel();
            } catch (ObjectDisposedException) { }
        }
        public void Wait() {
            Task.Wait();
        }
        public async Task WaitAsync() {
            await Task;
        }
        public override string ToString()
            => "thread";

        ~Thread() {
            CancelTokenSource.Dispose();
        }
    }
}
