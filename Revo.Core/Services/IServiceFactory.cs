using System.Collections.Generic;

namespace Revo.Core.Services;

public interface IServiceFactory<T>
{
    T GetRequiredService();
    T GetService();
    IEnumerable<T> GetServices();
}