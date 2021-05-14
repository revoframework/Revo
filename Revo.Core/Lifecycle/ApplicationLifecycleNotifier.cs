using System;

namespace Revo.Core.Lifecycle
{
    public class ApplicationLifecycleNotifier : IApplicationLifecycleNotifier
    {
        private readonly Func<IApplicationStartingListener[]> startingListenersFunc;
        private readonly Func<IApplicationStartedListener[]> startedListenersFunc;
        private readonly Func<IApplicationStoppingListener[]> stoppingListenersFunc;

        public ApplicationLifecycleNotifier(Func<IApplicationStartingListener[]> startingListenersFunc,
            Func<IApplicationStartedListener[]> startedListenersFunc,
            Func<IApplicationStoppingListener[]> stoppingListenersFunc)
        {
            this.startingListenersFunc = startingListenersFunc;
            this.startedListenersFunc = startedListenersFunc;
            this.stoppingListenersFunc = stoppingListenersFunc;
        }

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
