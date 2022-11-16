using Revo.Core.Core;

namespace Revo.Examples.BlazorWasmTodos.Blazor;

public class BlazorActorContext : IActorContext
{
	public string CurrentActorName => "BlazorUser";
}