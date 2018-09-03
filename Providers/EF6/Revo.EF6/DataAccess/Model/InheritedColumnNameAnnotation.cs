using System;

namespace Revo.EF6.DataAccess.Model
{
    public class InheritedColumnNameAnnotation
    {
        public InheritedColumnNameAnnotation(Type baseType, string name)
        {
            this.BaseType = baseType;
            this.Name = name;
        }

        public Type BaseType { get; private set; }
        public string Name { get; private set; }
    }
}
