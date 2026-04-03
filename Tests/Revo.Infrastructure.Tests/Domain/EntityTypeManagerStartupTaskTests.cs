using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Revo.Domain.Entities;
using Revo.Infrastructure.Domain;
using Xunit;

namespace Revo.Infrastructure.Tests.Domain;

public class EntityTypeManagerStartupTaskTests
{
    [Fact]
    public async Task ExecuteAsync_ClearsEntityTypeManagerCache()
    {
        var entityTypeManager = Substitute.For<IEntityTypeManager>();
        var sut = new EntityTypeManagerStartupTask(entityTypeManager);

        await sut.ExecuteAsync(CancellationToken.None);

        entityTypeManager.Received(1).ClearCache();
    }
}
