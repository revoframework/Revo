using System.Linq;
using Microsoft.EntityFrameworkCore;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities.Basic;
using Revo.EFCore.DataAccess.Conventions;

namespace Revo.EFCore.Domain
{
    public class BasicDomainModelConvention : IEFCoreConvention
    {
        public void Initialize(ModelBuilder modelBuilder)
        {
        }

        public void Finalize(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes()
                .Where(x => typeof(IRowVersioned).IsAssignableFrom(x.ClrType)))
            {
                entity.FindProperty(nameof(IRowVersioned.Version)).IsConcurrencyToken = true;
            }

            foreach (var entity in modelBuilder.Model.GetEntityTypes()
                .Where(x => typeof(IManuallyRowVersioned).IsAssignableFrom(x.ClrType)))
            {
                entity.FindProperty(nameof(IManuallyRowVersioned.Version)).IsConcurrencyToken = true;
            }

            foreach (var entity in modelBuilder.Model.GetEntityTypes()
                .Where(x => typeof(BasicAggregateRoot).IsAssignableFrom(x.ClrType)))
            {
                entity.RemoveProperty(nameof(BasicAggregateRoot.IsChanged));
                entity.RemoveProperty(nameof(BasicAggregateRoot.IsDeleted));
                entity.RemoveProperty(nameof(BasicAggregateRoot.UncommittedEvents));
            }
        }
    }
}
