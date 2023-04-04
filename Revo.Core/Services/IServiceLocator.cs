using System;
using System.Collections.Generic;

namespace Revo.Core.Services;

/// <summary>
/// Resolves all registered implementations of a service type (e.g. for multi-registration scenarios).
/// </summary>
public interface IServiceLocator
{
    IEnumerable<object> GetAll(Type serviceType);
}
