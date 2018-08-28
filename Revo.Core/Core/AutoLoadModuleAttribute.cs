using System;
using System.Collections.Generic;
using System.Text;

namespace Revo.Core.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoLoadModuleAttribute : Attribute
    {
        public AutoLoadModuleAttribute(bool autoLoad)
        {
            AutoLoad = autoLoad;
        }

        public bool AutoLoad { get; }
    }
}
