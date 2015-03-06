using System.Threading;

namespace System
{
    internal sealed class EventLoopSynchronizationContext : SynchronizationContext
    {
        private readonly Func<Action, bool> _enqueue;

        #region Constructor

        public EventLoopSynchronizationContext(Func<Action, bool> enqueue)
        {
            _enqueue = enqueue;
        }

        #endregion

        #region Overrides of SynchronizationContext

        public override void Post(SendOrPostCallback callback, object state)
        {
            _enqueue(() => callback(state));
        }

        public override void Send(SendOrPostCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}