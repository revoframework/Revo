using System;

namespace Revo.Core.Security
{
    public class PermissionTypeCatalogAttribute : Attribute
    {
        public PermissionTypeCatalogAttribute(string catalogName)
        {
            CatalogName = catalogName;
        }

        public string CatalogName { get; }
    }
}
