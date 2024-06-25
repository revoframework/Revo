using System.Linq;

namespace Revo.Core.Core
{
    public class Environment(IEnvironmentProvider[] environmentProviders) : IEnvironment
    {
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