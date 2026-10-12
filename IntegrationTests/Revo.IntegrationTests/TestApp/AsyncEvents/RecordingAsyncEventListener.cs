using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Infrastructure.Events.Async;
using Revo.IntegrationTests.TestApp.Domain.Events;

namespace Revo.IntegrationTests.TestApp.AsyncEvents
{
    /// <summary>
    /// Records the names of renamed lists; is never dispatched synchronously, so it only gets the events
    /// through the asynchronous processing (in-memory job queue).
    /// </summary>
    public class RecordingAsyncEventListener(
        RecordingAsyncEventListener.Sequencer eventSequencer,
        AsyncEventRecorder recorder)
        : IAsyncEventListener<TestListRenamedEvent>
    {
        private readonly List<string> names = new();

        public IAsyncEventSequencer EventSequencer { get; } = eventSequencer;

        public Task HandleAsync(IEventMessage<TestListRenamedEvent> message, string sequenceName)
        {
            names.Add(message.Event.Name);
            return Task.CompletedTask;
        }

        public Task OnFinishedEventQueueAsync(string sequenceName)
        {
            foreach (var name in names)
            {
                recorder.RecordedNames.Enqueue(name);
            }

            names.Clear();
            return Task.CompletedTask;
        }

        public class Sequencer : AsyncEventSequencer<TestListRenamedEvent>
        {
            protected override IEnumerable<EventSequencing> GetEventSequencing(
                IEventMessage<TestListRenamedEvent> message)
            {
                yield return new EventSequencing { SequenceName = nameof(RecordingAsyncEventListener) };
            }

            protected override bool ShouldAttemptSynchronousDispatch(IEventMessage<TestListRenamedEvent> message)
            {
                return false;
            }
        }
    }
}
