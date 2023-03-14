using System;
using System.Collections.Generic;
using System.Linq;

namespace Revo.Core.Types
{
    public class TypeIndexer : ITypeIndexer
    {
        private readonly ITypeExplorer typeExplorer;

        public TypeIndexer(ITypeExplorer typeExplorer)
        {
            this.typeExplorer = typeExplorer;
        }

        public IEnumerable<VersionedType> IndexTypes<TBase>()
        {
            var types = typeExplorer.GetAllTypes()
                .Where(x => typeof(TBase).IsAssignableFrom(x))
                .Where(x => !x.IsAbstract && !x.IsGenericTypeDefinition
                            && !x.IsConstructedGenericType);

            foreach (Type type in types)
            {
                TypeVersionAttribute versionAttribute = (TypeVersionAttribute)type
                    .GetCustomAttributes(typeof(TypeVersionAttribute), false)
                    .FirstOrDefault();

                int version;
                string name;

                if (versionAttribute != null)
                {
                    version = versionAttribute.Version;
                    name = versionAttribute.Name ?? type.Name;
                }
                else
                {
                    ExtractVersionFromName(type.Name, out name, out version);
                }

                yield return new VersionedType(new VersionedTypeId(name, version), type);
            }
        }

        private void ExtractVersionFromName(string typeName, out string name, out int version)
        {
            version = 1;
            name = typeName;

            int versionBegin = typeName.Length;
            while (versionBegin > 1 && char.IsDigit(typeName[versionBegin - 1]))
            {
                versionBegin--;
            }

            if (versionBegin < 2 || versionBegin == typeName.Length || typeName[versionBegin - 1] != 'V')
            {
                return;
            }

            name = typeName.Substring(0, versionBegin - 1);
            version = int.Parse(typeName.Substring(versionBegin));
        }
    }
}
