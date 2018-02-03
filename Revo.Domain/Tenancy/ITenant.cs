using System;

namespace Revo.Domain.Tenancy
{
    public interface ITenant
    {
        Guid Id { get; }
        string Name { get; }
    }
}
