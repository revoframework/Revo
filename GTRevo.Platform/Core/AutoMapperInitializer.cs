using System.Reflection;
using AutoMapper;
using AutoMapper.Attributes;
using GTRevo.Platform.Core.Lifecycle;

namespace GTRevo.Platform.Core
{
    public class AutoMapperInitializer : IApplicationStartListener
    {
        private readonly ITypeExplorer typeExplorer;

        public AutoMapperInitializer(ITypeExplorer typeExplorer)
        {
            this.typeExplorer = typeExplorer;
        }

        public void OnApplicationStarted()
        {
            Mapper.Initialize(config =>
            {
                foreach (Assembly assembly in typeExplorer.GetAllReferencedAssemblies())
                {
                    assembly.MapTypes(config);
                }
            });
        }
    }
}
