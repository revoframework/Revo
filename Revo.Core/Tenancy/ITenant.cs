using System;

namespace Revo.Core.Tenancy
{
    public interface ITenant
    {
        Guid Id { get; }
        string Name { get; }
    }
}
