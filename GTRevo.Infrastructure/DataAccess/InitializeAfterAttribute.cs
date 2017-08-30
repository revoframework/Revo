using System;

namespace GTRevo.Infrastructure.DataAccess
{
    /// <summary>
    /// This attribute can be used for modelling dependencies between initializers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class InitializeAfterAttribute: Attribute
    {
        public InitializeAfterAttribute(Type initializerType)
        {
            InitializerType = initializerType;
        }

        public Type InitializerType { get; }
    }
}
