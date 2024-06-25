using System;

namespace Revo.Infrastructure.DataAccess
{
    /// <summary>
    /// This attribute can be used for modelling dependencies between initializers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class InitializeAfterAttribute(Type initializerType) : Attribute
    {
        public Type InitializerType { get; } = initializerType;
    }
}
