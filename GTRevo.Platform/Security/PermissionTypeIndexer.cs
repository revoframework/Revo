using System;
using System.Collections.Generic;
using System.Linq;
using GTRevo.Core;
using GTRevo.Core.Core;
using GTRevo.Core.Core.Lifecycle;

namespace GTRevo.Platform.Security
{
    public class PermissionTypeIndexer : IApplicationStartListener
    {
        private readonly IPermissionTypeRegistry permissionTypeRegistry;
        private readonly ITypeExplorer typeExplorer;
        private readonly HashSet<Type> registeredCatalogTypes = new HashSet<Type>();

        public PermissionTypeIndexer(ITypeExplorer typeExplorer,
            IPermissionTypeRegistry permissionTypeRegistry)
        {
            this.typeExplorer = typeExplorer;
            this.permissionTypeRegistry = permissionTypeRegistry;
        }

        public void OnApplicationStarted()
        {
            Index();
        }

        public void EnsureIndexed()
        {
            if (registeredCatalogTypes.Count == 0)
            {
                Index();
            }
        }

        private void Index()
        {
            foreach (Type catalogType in typeExplorer.GetAllTypes()
                                            .Where(x => x.IsClass
                                                && x.IsAbstract /* static in C# = abstract + sealed */
                                                && x.IsSealed
                                                && x.GetCustomAttributes(typeof(PermissionTypeCatalogAttribute), false).Any()))
            {
                if (!registeredCatalogTypes.Contains(catalogType))
                {
                    RegisterCatalog(catalogType);
                    registeredCatalogTypes.Add(catalogType);
                }
            }
        }

        private void RegisterCatalog(Type catalogType)
        {
            PermissionTypeCatalogAttribute catalogAttribute = (PermissionTypeCatalogAttribute)catalogType
                .GetCustomAttributes(typeof(PermissionTypeCatalogAttribute), false)
                .First();

            foreach (var field in catalogType.GetFields(System.Reflection.BindingFlags.Static
                                    | System.Reflection.BindingFlags.Public)
                                    .Where(x => x.IsLiteral && !x.IsInitOnly /* exclude readonly */
                                        && x.FieldType == typeof(string)))
            {
                try
                {

                    PermissionType permissionType = new PermissionType(
                        Guid.Parse((string)field.GetValue(null)),
                        catalogAttribute.CatalogName + "." + field.Name);
                    permissionTypeRegistry.RegisterPermissionType(permissionType);
                }
                catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException)
                {
                    throw new InvalidOperationException("Permission type definition is not a valid GUID string: " + field.ToString());
                }
            }
        }
    }
}
