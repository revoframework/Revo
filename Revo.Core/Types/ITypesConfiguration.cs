using System.Collections.Generic;
using System.Reflection;

namespace Revo.Core.Types;

public interface ITypesConfiguration
{
    IReadOnlyCollection<Assembly> Assemblies { get; }
    bool IsAppDomainTypeDiscoveryEnabled { get; }
}