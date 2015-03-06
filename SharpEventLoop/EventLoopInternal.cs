using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace System
{
    internal sealed class EventLoopInternal : IDisposable
    {
        private readonly BlockingCollection<Action> _actions;
        private int _numberOfConcurrentTasks;

        #region Abstract

        private bool Enqueue(Action action)
        {
            try
            {
                _actions.Add(action);
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
        
        private void TryLeave()
        {
            if (_actions.Count != 0)
            {
                Enqueue(TryLeave);
                return;
            }

            if (_numberOfConcurrentTasks == 0)
            {
                Dispose();
            }
        }

        #endregion

        #region Constructor

        public EventLoopInternal()
        {
            _actions = new BlockingCollection<Action>();
        }

        #endregion

        #region Methods

        public void Enter()
        {
            var currentContext = new EventLoopSynchronizationContext(Enqueue);
            var enumerator = _actions.GetConsumingEnumerable().GetEnumerator();
            var previousContext = SynchronizationContext.Current;

            SynchronizationContext.SetSynchronizationContext(currentContext);

            while (true)
            {
                try
                {
                    enumerator.MoveNext();
                }
                catch (InvalidOperationException)
                {
                    break;
                }

                try
                {
                    enumerator.Current();
                }
                catch
                {
                    SynchronizationContext.SetSynchronizationContext(previousContext);
                    throw;
                }
            }

            SynchronizationContext.SetSynchronizationContext(previousContext);
        }

        public bool Run(Func<Task> worker)
        {
            var success = Enqueue(async () =>
            {
                try
                {
                    await worker();
                }
                catch (Exception e)
                {
                    Enqueue(() => { throw e; });
                }
                finally
                {
                    _numberOfConcurrentTasks--;
                    if (_numberOfConcurrentTasks == 0) Enqueue(TryLeave);
                }
            });

            if (success)
            {
                _numberOfConcurrentTasks++;
            }

            return success;
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            _actions.Dispose();
        }

        #endregion
    }
}