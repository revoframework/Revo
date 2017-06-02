using System;
using System.Threading.Tasks;
using GTRevo.Infrastructure.History.ChangeTracking;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.History.ChangeTracking
{
    public class EntityAttributeChangeBatchTests
    {
        private readonly EntityAttributeChangeBatch sut;
        private readonly IEntityAttributeChangeLogger entityAttributeChangeLogger;
        private readonly Guid aggregateId = Guid.NewGuid();
        private readonly Guid aggregateClassId = Guid.NewGuid();
        private readonly Guid entityId = Guid.NewGuid();
        private readonly Guid entityClassId = Guid.NewGuid();

        public EntityAttributeChangeBatchTests()
        {
            entityAttributeChangeLogger = Substitute.For<IEntityAttributeChangeLogger>();

            sut = new EntityAttributeChangeBatch(entityAttributeChangeLogger, aggregateId,
                aggregateClassId, entityId, entityClassId);
        }

        [Fact]
        public async Task Change_And_FinishAsync_ChangesAttributes()
        {
            sut.Change("first", "value");
            sut.Change("second", 5);

            await sut.FinishAsync();

            entityAttributeChangeLogger.Received(1).ChangeAttribute("first", "value",
                aggregateId, aggregateClassId, entityId, entityClassId);
            entityAttributeChangeLogger.Received(1).ChangeAttribute("second", 5,
                aggregateId, aggregateClassId, entityId, entityClassId);
        }
    }
}
