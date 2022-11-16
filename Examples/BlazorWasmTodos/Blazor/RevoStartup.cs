using Ninject;
using Ninject.Activation;
using Ninject.Extensions.ContextPreservation;
using Ninject.Extensions.Factory;
using Ninject.Infrastructure.Disposal;
using Revo.Core.Commands;
using Revo.Core.Configuration;
using Revo.Core.Types;
using Revo.Infrastructure.DataAccess.Migrations;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Revo.Examples.BlazorWasmTodos.Blazor
{
    public abstract class RevoStartup
    {
        private static readonly AsyncLocal<Scope> scopeProvider = new AsyncLocal<Scope>();
        public static object RequestScope(IContext context) => scopeProvider.Value;

        private KernelBootstrapper kernelBootstrapper;

        public RevoStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public IKernel Kernel { get; private set; }
        private object Resolve(Type type) => Kernel.Get(type);

        public virtual void ConfigureServices(IServiceCollection services)
        {
            CreateKernel();
            var revoConfiguration = CreateRevoConfiguration();

            /** NOTE: these assemblies containing these modules are usually not directly referenced
             * and thus would other not get loaded into the app domain */
            Type[] ninjectExtModules = new[] { typeof(FuncModule), typeof(ContextPreservationModule) };
            foreach (Type ninjectExtModule in ninjectExtModules)
            {
                if (!revoConfiguration
                    .GetSection<KernelConfigurationSection>()
                    .LoadedModuleOverrides.ContainsKey(ninjectExtModule))
                {
                    revoConfiguration.OverrideModuleLoading(ninjectExtModule, true);
                }
            }

            services.AddSingleton(sp => Kernel);

            kernelBootstrapper = new KernelBootstrapper(Kernel, revoConfiguration);

            var typeExplorer = new TypeExplorer();

            kernelBootstrapper.Configure();

            var assemblies = typeExplorer
                .GetAllReferencedAssemblies()
                .Where(a => !a.GetName().Name.StartsWith("System."))
                .Where(a => !a.IsDynamic).ToList();

            kernelBootstrapper.LoadAssemblies(assemblies);

            ActivationExtensions.RegisterComponentTypes(Kernel);
            services.AddCustomComponentActivation();
        }

        public virtual async Task Configure()
        {
            kernelBootstrapper.RunAppConfigurers();

            await Kernel.Get<ICommandBus>()
	            .SendAsync(new ExecuteDatabaseMigrationsCommand());

            kernelBootstrapper.RunAppStartListeners();
        }

        protected abstract IRevoConfiguration CreateRevoConfiguration();

        private void CreateKernel()
        {
            Kernel = new StandardKernel();

            Kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => Kernel);
            Kernel.Bind<StandardKernel>().ToMethod(ctx => Kernel as StandardKernel);
            //NinjectBindingExtensions.Current = new AspNetCoreNinjectBindingExtension();
        }

        private sealed class Scope : DisposableObject { }
    }
}
