using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revo.Core.Lifecycle
{
    public class ApplicationStartListenerInitializer : IApplicationStartListenerInitializer
    {
        private readonly Func<IApplicationStartListener[]> startListenersFunc;
        private readonly Func<IApplicationStopListener[]> stopListenersFunc;

        public ApplicationStartListenerInitializer(Func<IApplicationStartListener[]> startListenersFunc,
            Func<IApplicationStopListener[]> stopListenersFunc)
        {
            this.startListenersFunc = startListenersFunc;
            this.stopListenersFunc = stopListenersFunc;
        }

        public void InitializeStarted()
        {
            foreach (var listener in startListenersFunc())
            {
                listener.OnApplicationStarted();
            }
        }

        public void DeinitializeStopping()
        {
            foreach (var listener in stopListenersFunc())
            {
                listener.OnApplicationStopping();
            }
        }
    }
}
