using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Ninject;
using Revo.Core.Types;

namespace Revo.Examples.BlazorWasmTodos.Blazor
{
    public static class ActivationExtensions
	{
        public static void AddCustomComponentActivation(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IComponentActivator, DelegatingComponentActivator>();
        }

        public static void RegisterComponentTypes(IKernel kernel)
        {
            var typeExplorer = new TypeExplorer();

            var componentTypes = typeExplorer
                .GetAllTypes()
                .Where(x => typeof(IComponent).IsAssignableFrom(x));
            foreach (var componentType in componentTypes)
            {
                kernel.Bind(componentType).ToSelf().InTransientScope();
            }
        }

        internal sealed class DelegatingComponentActivator : IComponentActivator
        {
	        private readonly IKernel kernel;

	        public DelegatingComponentActivator(IKernel kernel)
	        {
		        this.kernel = kernel;
	        }

	        /// <inheritdoc />
	        public IComponent CreateInstance([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type componentType)
	        {
		        if (!typeof(IComponent).IsAssignableFrom(componentType))
		        {
			        throw new ArgumentException($"The type {componentType.FullName} does not implement {nameof(IComponent)}.", nameof(componentType));
		        }

		        return (IComponent)kernel.Get(componentType)!;
	        }
        }
	}
}
