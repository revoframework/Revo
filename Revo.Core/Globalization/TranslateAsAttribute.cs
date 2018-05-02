using System;

namespace Revo.Core.Globalization
{
    /// <summary>
    /// This attribute says the decorated type should be translated using translations for another type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TranslateAsAttribute: Attribute
    {
        public TranslateAsAttribute(Type type)
        {
            Type = type;
        }
        public Type Type { get; private set; }
    }
}
