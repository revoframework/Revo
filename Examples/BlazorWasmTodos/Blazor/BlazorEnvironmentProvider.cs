using Revo.Core.Core;

namespace Revo.Examples.BlazorWasmTodos.Blazor;

public class BlazorEnvironmentProvider : IEnvironmentProvider
{
	public bool? IsDevelopment => false;
}