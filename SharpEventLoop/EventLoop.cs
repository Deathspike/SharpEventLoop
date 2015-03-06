using System.Threading.Tasks;

namespace System
{
    public static class EventLoop
    {
        [ThreadStatic]
        private static EventLoopInternal _eventLoop;
        private static readonly object _synchronize;

        #region Constructor

        static EventLoop()
        {
            _synchronize = new object();
        }

        #endregion

        #region Methods

        public static bool Run(Func<Task> worker)
        {
            var eventLoop = _eventLoop;
            return eventLoop != null && _eventLoop.Run(worker);
        }

        public static bool Pump(Action initializer)
        {
            try
            {
                lock (_synchronize)
                {
                    if (_eventLoop != null) return false;
                    _eventLoop = new EventLoopInternal();
                }

                initializer();
                _eventLoop.Enter();
                return true;
            }
            finally
            {
                lock (_synchronize)
                {
                    if (_eventLoop != null)
                    {
                        _eventLoop.Dispose();
                        _eventLoop = null;
                    }
                }
            }
        }

        #endregion
    }
}