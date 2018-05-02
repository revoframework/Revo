using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revo.Core.Types
{
    public class TypeVersionAttribute : Attribute
    {
        public TypeVersionAttribute(string name, int version)
        {
            Name = name;
            Version = version;
        }

        public TypeVersionAttribute(int version)
        {
            Version = version;
        }

        public string Name { get; }
        public int Version { get; }
    }
}
