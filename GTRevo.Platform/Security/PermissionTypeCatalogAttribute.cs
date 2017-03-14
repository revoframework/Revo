using System;

namespace GTRevo.Platform.Security
{
    public class PermissionTypeCatalogAttribute : Attribute
    {
        public PermissionTypeCatalogAttribute(string catalogName)
        {
            CatalogName = catalogName;
        }

        public string CatalogName { get; private set; }
    }
}
