using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revo.Core.Lifecycle
{
    public class ApplicationConfigurerInitializer : IApplicationConfigurerInitializer
    {
        private readonly Func<IApplicationConfigurer[]> configurersFunc;

        public ApplicationConfigurerInitializer(Func<IApplicationConfigurer[]> configurersFunc)
        {
            this.configurersFunc = configurersFunc;
        }

        public void ConfigureAll()
        {
            foreach (var configurer in configurersFunc())
            {
                configurer.Configure();
            }
        }
    }
}
