using System;
using System.Threading.Tasks;

namespace Ink_Canvas.Helpers.Persistence
{
    /// <summary>One queue for sync and async saves. Accepted writers never call the Dispatcher.</summary>
    public sealed class SaveCoordinator
    {
        private readonly object _gate = new object();
        private Task _tail = Task.CompletedTask;
        private bool _stopped;

        // Capture runs on the caller (UI) thread, before returning, including queued manual saves.
        // A null result means the automatic request was busy, or shutdown rejected the request.
        public Task Submit(Func<Action> capture, bool automatic)
        {
            lock (_gate)
            {
                if (_stopped || (automatic && !_tail.IsCompleted)) return null;
                var write = capture();
                var previous = _tail;
                _tail = Task.Run(async () =>
                {
                    try { await previous.ConfigureAwait(false); }
                    catch { /* A failed save must not discard the next accepted manual save. */ }
                    write();
                });
                return _tail;
            }
        }

        public Task StopAndDrainAsync()
        {
            lock (_gate)
            {
                _stopped = true;
                return _tail;
            }
        }
    }
}
