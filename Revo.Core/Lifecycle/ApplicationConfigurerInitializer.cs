using System;

namespace Revo.Core.Lifecycle
{
    public class ApplicationConfigurerInitializer(Func<IApplicationConfigurer[]> configurersFunc) : IApplicationConfigurerInitializer
    {
        private readonly Func<IApplicationConfigurer[]> configurersFunc = configurersFunc;

        public void ConfigureAll()
        {
            foreach (var configurer in configurersFunc())
            {
                configurer.Configure();
            }
        }
    }
}
