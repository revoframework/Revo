using System;

namespace GTRevo.Infrastructure.Core.Tenancy
{
    public interface ITenant
    {
        Guid Id { get; }
        string Name { get; }
    }
}
