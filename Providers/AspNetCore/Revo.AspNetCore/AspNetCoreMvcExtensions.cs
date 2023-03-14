using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Ninject;
using Revo.Core.Types;

namespace Revo.AspNetCore
{
    public static class AspNetCoreMvcExtensions
    {
        public static void AddCustomControllerActivation(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IControllerActivator, DelegatingControllerActivator>();
        }
        
        public static void AddCustomHubActivation(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddScoped(typeof(IHubActivator<>), typeof(DelegatingHubActivator<>));
        }

        public static void AddCustomViewComponentActivation(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IViewComponentActivator, DelegatingViewComponentActivator>();
        }
        
        public static Type[] GetControllerTypes(this IApplicationBuilder builder)
        {
            var manager = builder.ApplicationServices.GetRequiredService<ApplicationPartManager>();

            var feature = new ControllerFeature();
            manager.PopulateFeature(feature);

            return feature.Controllers.Select(t => t.AsType()).ToArray();
        }

        public static Type[] GetHubTypes(this IApplicationBuilder builder, ITypeExplorer typeExplorer)
        {
            return typeExplorer.GetAllTypes()
                .Where(x => x.IsClass && !x.IsAbstract && typeof(Hub).IsAssignableFrom(x))
                .ToArray();
        }

        public static Type[] GetViewComponentTypes(this IApplicationBuilder builder)
        {
            var manager = builder.ApplicationServices.GetRequiredService<ApplicationPartManager>();

            var feature = new ViewComponentFeature();
            manager.PopulateFeature(feature);

            return feature.ViewComponents.Select(t => t.AsType()).ToArray();
        }
    }

    internal sealed class DelegatingControllerActivator : IControllerActivator
    {
        private readonly IKernel kernel;

        public DelegatingControllerActivator(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public object Create(ControllerContext context) => kernel.Get(context.ActionDescriptor.ControllerTypeInfo.AsType());

        public void Release(ControllerContext context, object controller)
        {
        }
    }

    internal sealed class DelegatingHubActivator<T> : IHubActivator<T>
        where T : Hub
    {
        private readonly IKernel kernel;

        public DelegatingHubActivator(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public T Create() => kernel.Get<T>();
        public void Release(T hub)
        {
        }
    }

    internal sealed class DelegatingViewComponentActivator : IViewComponentActivator
    {
        private readonly IKernel kernel;

        public DelegatingViewComponentActivator(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public object Create(ViewComponentContext context) => kernel.Get(context.ViewComponentDescriptor.TypeInfo.AsType());

        public void Release(ViewComponentContext context, object viewComponent)
        {
        }
    }

    internal sealed class DelegatingTagHelperActivator : ITagHelperActivator
    {
        private readonly Predicate<Type> customCreatorSelector;
        private readonly Func<Type, object> customTagHelperCreator;
        private readonly ITagHelperActivator defaultTagHelperActivator;

        public DelegatingTagHelperActivator(Predicate<Type> customCreatorSelector, Func<Type, object> customTagHelperCreator,
            ITagHelperActivator defaultTagHelperActivator)
        {
            this.customCreatorSelector = customCreatorSelector ?? throw new ArgumentNullException(nameof(customCreatorSelector));
            this.customTagHelperCreator = customTagHelperCreator ?? throw new ArgumentNullException(nameof(customTagHelperCreator));
            this.defaultTagHelperActivator = defaultTagHelperActivator ?? throw new ArgumentNullException(nameof(defaultTagHelperActivator));
        }

        public TTagHelper Create<TTagHelper>(ViewContext context) where TTagHelper : ITagHelper =>
            this.customCreatorSelector(typeof(TTagHelper))
                ? (TTagHelper)this.customTagHelperCreator(typeof(TTagHelper))
                : defaultTagHelperActivator.Create<TTagHelper>(context);
    }
}
