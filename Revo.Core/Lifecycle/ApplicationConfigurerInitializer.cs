using System;

namespace Revo.Core.Lifecycle
{
    public class ApplicationConfigurerInitializer(Func<IApplicationConfigurer[]> configurersFunc) : IApplicationConfigurerInitializer
    {
        public void ConfigureAll()
        {
            foreach (var configurer in configurersFunc())
            {
                configurer.Configure();
            }
        }
    }
}
