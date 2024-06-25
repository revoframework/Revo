using System;

namespace Revo.EFCore.DataAccess.Entities
{
    /// <summary>
    /// Attribute that set Entity Framework Core BaseType property for the model mapping
    /// of the annotated class.
    /// </summary>
    public class BaseTypeAttribute(Type baseType) : Attribute
    {
        public Type BaseType { get; set; } = baseType;
    }
}
