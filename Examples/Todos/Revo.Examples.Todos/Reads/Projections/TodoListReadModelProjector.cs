using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.EFCore.DataAccess.Entities;
using Revo.EFCore.Projections;
using Revo.Examples.Todos.Domain;
using Revo.Examples.Todos.Domain.Events;
using Revo.Examples.Todos.Reads.Model;

namespace Revo.Examples.Todos.Reads.Projections
{
    public class TodoListReadModelProjector : EFCoreEntityEventToPocoProjector<TodoList, TodoListReadModel>
    {
        public TodoListReadModelProjector(IEFCoreCrudRepository repository) : base(repository)
        {
        }

        private void Apply(IEventMessage<TodoListRenamedEvent> ev)
        {
            Target.Name = ev.Event.Name;
        }

        private void Apply(IEventMessage<TodoAddedEvent> ev)
        {
            var task = new TodoReadModel()
            {
                Id = ev.Event.TodoId,
                TodoListId = ev.Event.AggregateId
            };

            Repository.Add(task);
        }

        private async Task Apply(IEventMessage<TodoTextUpdatedEvent> ev)
        {
            var task = await Repository.FindAsync<TodoReadModel>(ev.Event.TodoId);
            task.Text = ev.Event.Text;
        }

        private async Task Apply(IEventMessage<TodoIsCompleteUpdatedEvent> ev)
        {
            var task = await Repository.FindAsync<TodoReadModel>(ev.Event.TodoId);
            task.IsComplete = ev.Event.IsComplete;
        }
    }
}
