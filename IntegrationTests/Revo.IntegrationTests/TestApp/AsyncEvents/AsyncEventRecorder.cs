using System.Collections.Concurrent;

namespace Revo.IntegrationTests.TestApp.AsyncEvents
{
    /// <summary>
    /// Collects what <see cref="RecordingAsyncEventListener"/> received (registered as a singleton).
    /// </summary>
    public class AsyncEventRecorder
    {
        public ConcurrentQueue<string> RecordedNames { get; } = new();
    }
}
