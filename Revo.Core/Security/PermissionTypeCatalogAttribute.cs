using System;

namespace Revo.Core.Security
{
    public class PermissionTypeCatalogAttribute(string catalogName) : Attribute
    {
        public string CatalogName { get; } = catalogName;
    }
}
