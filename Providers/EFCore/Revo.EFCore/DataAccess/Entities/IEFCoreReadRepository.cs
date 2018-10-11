using System;
using Microsoft.EntityFrameworkCore;
using Revo.DataAccess.Entities;

namespace Revo.EFCore.DataAccess.Entities
{
    public interface IEFCoreReadRepository : IReadRepository
    {
        IEFCoreDatabaseAccess DatabaseAccess { get; }
    }
}
