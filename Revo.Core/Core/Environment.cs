using System.Linq;

namespace Revo.Core.Core
{
    public class Environment : IEnvironment
    {
        private readonly IEnvironmentProvider[] environmentProviders;

        public Environment(IEnvironmentProvider[] environmentProviders)
        {
            this.environmentProviders = environmentProviders;
        }

        public bool IsDevelopment
        {
            get
            {
                return IsDevelopmentOverride
                    ?? environmentProviders.Any(x => x.IsDevelopment == true);
            }
        }

        public bool? IsDevelopmentOverride { get; set; }
    }
}