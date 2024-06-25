using System;

namespace Revo.Core.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoLoadModuleAttribute(bool autoLoad) : Attribute
    {


        public bool AutoLoad { get; } = autoLoad;
    }
}
