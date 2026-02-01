using System;

namespace Revo.Core.Lifecycle
{
    public class ApplicationLifecycleNotifier(Func<IApplicationStartingListener[]> startingListenersFunc,
        Func<IApplicationStartedListener[]> startedListenersFunc,
        Func<IApplicationStoppingListener[]> stoppingListenersFunc) : IApplicationLifecycleNotifier
    {
        private readonly Func<IApplicationStartingListener[]> startingListenersFunc = startingListenersFunc;
        private readonly Func<IApplicationStartedListener[]> startedListenersFunc = startedListenersFunc;
        private readonly Func<IApplicationStoppingListener[]> stoppingListenersFunc = stoppingListenersFunc;

        public void NotifyStarting()
        {
            foreach (var listener in startingListenersFunc())
            {
                listener.OnApplicationStarting();
            }
        }

        public void NotifyStarted()
        {
            foreach (var listener in startedListenersFunc())
            {
                listener.OnApplicationStarted();
            }
        }

        public void NotifyStopping()
        {
            foreach (var listener in stoppingListenersFunc())
            {
                listener.OnApplicationStopping();
            }
        }
    }
}
