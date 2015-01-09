using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SharpEventLoop
{
    public class EventLoop : IDisposable
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

        private void Enter()
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

        private EventLoop()
        {
            _actions = new BlockingCollection<Action>();
        }

        #endregion

        #region Methods
        
        public bool Run(Func<Task> task)
        {
            _numberOfConcurrentTasks++;

            return Enqueue(async () =>
            {
                try
                {
                    await task();
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
        }

        #endregion

        #region Statics

        public static void Pump(Action<EventLoop> initializer)
        {
            using (var eventLoop = new EventLoop())
            {
                initializer(eventLoop);
                eventLoop.Enter();
            }
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