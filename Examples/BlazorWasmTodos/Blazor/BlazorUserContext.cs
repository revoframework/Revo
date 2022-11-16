using Revo.Core.Security;

namespace Revo.Examples.BlazorWasmTodos.Blazor;

public class BlazorUserContext : IUserContext
{
	public Task<IUser> GetUserAsync()
	{
		return Task.FromResult<IUser>(null);
	}

	public Task<IReadOnlyCollection<Permission>> GetPermissionsAsync()
	{
		return Task.FromResult<IReadOnlyCollection<Permission>>(Array.Empty<Permission>());
	}

	public bool IsAuthenticated => false;
	public Guid? UserId => null;
}