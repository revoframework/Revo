using System;
using System.Collections.Generic;
using System.Text;

namespace Revo.Infrastructure.Events.Async
{
    public class PerTenantAsyncEventSequencer<TListener> : PerTenantAsyncEventSequencer
        where TListener : IAsyncEventListener
    {
        protected PerTenantAsyncEventSequencer() : base(typeof(TListener).Name)
        {
        }
    }
}
