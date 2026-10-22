using Microsoft.Extensions.DependencyInjection;

namespace Revo.Core.Services;

public interface IServiceConfigurator
{
    void Configure(IServiceCollection services);
}