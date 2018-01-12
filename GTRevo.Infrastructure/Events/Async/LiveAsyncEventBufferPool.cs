using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.Events.Async
{
    public class LiveAsyncEventBufferPool
    {
        private readonly ConcurrentDictionary<string, LiveAsyncEventBuffer> buffers =
            new ConcurrentDictionary<string, LiveAsyncEventBuffer>();

        public void AddEvents(string eventSequenceName, IReadOnlyCollection<IEventMessage> events)
        {
            LiveAsyncEventBuffer buffer = LockBuffer(eventSequenceName);
            try
            {
                buffer.AddEvents(events);
            }
            finally
            {
                buffer.Unlock();
            }
        }

        private LiveAsyncEventBuffer LockBuffer(string eventSequenceName)
        {
            LiveAsyncEventBuffer buffer = null;
            do
            {
                buffer = TryLockBuffer(eventSequenceName);
            } while (buffer == null);

            return buffer;
        }

        private LiveAsyncEventBuffer TryLockBuffer(string eventSequenceName)
        {
            LiveAsyncEventBuffer buffer;
            if (!buffers.TryGetValue(eventSequenceName, out buffer))
            {
                buffer = new LiveAsyncEventBuffer();
                buffer.Lock();
                bool shouldUnlock = true;
                try
                {
                    if (!buffers.TryAdd(eventSequenceName, buffer))
                    {
                        return null;
                    }
                    else
                    {
                        shouldUnlock = false;
                        return buffer;
                    }
                }
                finally
                {
                    if (shouldUnlock)
                    {
                        buffer.Unlock();
                    }
                }
            }
            else
            {
                buffer.Lock();
                bool shouldUnlock = true;
                try
                {
                    if (!buffers.TryGetValue(eventSequenceName, out var currentBuffer)
                        || buffer != currentBuffer)
                    {
                        return null;
                    }
                    else
                    {
                        shouldUnlock = false;
                        return buffer;
                    }
                }
                finally
                {
                    if (shouldUnlock)
                    {
                        buffer.Unlock();
                    }
                }
            }
        }
    }
}
