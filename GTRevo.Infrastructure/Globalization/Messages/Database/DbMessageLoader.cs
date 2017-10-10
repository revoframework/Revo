using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Tenancy;

namespace GTRevo.Infrastructure.Globalization.Messages.Database
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

        public Task Handle(LocalizationMessageModifiedEvent notification)
        {
            Invalidate();
            return Task.FromResult(0);
        }

        public Task Handle(LocalizationMessageDeletedEvent notification)
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
