using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.DataAccess.Entities;

namespace Revo.Infrastructure.Globalization.Messages.Database
{
    public class DbMessageLoader : IDbMessageLoader,
        IEventListener<LocalizationMessageModifiedEvent>,
        IEventListener<LocalizationMessageDeletedEvent>
    {
        private readonly IDbMessageCache dbMessageCache;
        private readonly IReadRepository readRepository;

        public DbMessageLoader(IDbMessageCache dbMessageCache, IReadRepository readRepository)
        {
            this.dbMessageCache = dbMessageCache;
            this.readRepository = readRepository;
        }

        public void EnsureLoaded()
        {
            if (!dbMessageCache.IsInitialized)
            {
                Reload();
            }
        }

        public void Reload()
        {
            lock (dbMessageCache)
            {
                List<LocalizationMessage> messages = readRepository.FindAll<LocalizationMessage>().ToList();
                dbMessageCache.ReplaceMessages(messages);
            }
        }

        public Task HandleAsync(IEventMessage<LocalizationMessageModifiedEvent> message, CancellationToken cancellationToken)
        {
            Invalidate();
            return Task.FromResult(0);
        }

        public Task HandleAsync(IEventMessage<LocalizationMessageDeletedEvent> message, CancellationToken cancellationToken)
        {
            Invalidate();
            return Task.FromResult(0);
        }

        private void Invalidate()
        {
            // TODO emit event and reload only eventually in a background job
            Reload();
        }
    }
}
