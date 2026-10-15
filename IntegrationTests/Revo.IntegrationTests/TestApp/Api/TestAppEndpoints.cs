using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Revo.Core.Commands;
using Revo.IntegrationTests.TestApp.Api.Requests;
using Revo.IntegrationTests.TestApp.Messages.Commands;
using Revo.IntegrationTests.TestApp.Messages.Queries;
using Revo.IntegrationTests.TestApp.Tenancy;

namespace Revo.IntegrationTests.TestApp.Api
{
    public static class TestAppEndpoints
    {
        public static void MapTestAppEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/lists", (ICommandBus commandBus, CancellationToken ct) =>
                commandBus.SendAsync(new GetTestListsQuery(), ct));

            endpoints.MapPost("/lists", (CreateTestListRequest request, ICommandBus commandBus,
                    CancellationToken ct) =>
                commandBus.SendAsync(new CreateTestListCommand(request.Id, request.Name), ct));

            endpoints.MapPut("/lists/{id:guid}", (Guid id, RenameTestListRequest request, ICommandBus commandBus,
                    CancellationToken ct) =>
                commandBus.SendAsync(new RenameTestListCommand(id, request.Name), ct));

            endpoints.MapPost("/lists/{id:guid}/trigger-saga", (Guid id, ICommandBus commandBus,
                    CancellationToken ct) =>
                commandBus.SendAsync(new TriggerTestListSagaCommand(id), ct));

            endpoints.MapPost("/lists/with-children", (CreateTestListWithChildrenRequest request,
                    ICommandBus commandBus, CancellationToken ct) =>
                commandBus.SendAsync(
                    new CreateTestListWithChildrenCommand(request.Id, request.Name, request.ChildIds), ct));

            endpoints.MapPost("/lists/then-fail", (CreateTestListThenFailRequest request, ICommandBus commandBus,
                    CancellationToken ct) =>
                commandBus.SendAsync(new CreateTestListThenFailCommand(request.Id, request.NestedId), ct));

            endpoints.MapPost("/lists/two-in-one-request", async (CreateTwoTestListsRequest request,
                ICommandBus commandBus, CancellationToken ct) =>
            {
                await commandBus.SendAsync(new CreateTestListCommand(request.FirstId, request.Name), ct);
                await commandBus.SendAsync(new CreateTestListCommand(request.SecondId, request.Name), ct);
            });

            endpoints.MapPost("/authenticated-only", (ICommandBus commandBus, CancellationToken ct) =>
                commandBus.SendAsync(new AuthenticatedOnlyCommand(), ct));

            endpoints.MapGet("/tenant", (Guid? tenantId, ICommandBus commandBus, CancellationToken ct) =>
                commandBus.SendAsync(new GetCurrentTenantIdQuery(),
                    tenantId == null
                        ? CommandExecutionOptions.Default
                        : new CommandExecutionOptions(
                            tenantContext: new CommandTenantContextOverride(new TestTenant(tenantId.Value, "test"))),
                    ct));
        }
    }
}
