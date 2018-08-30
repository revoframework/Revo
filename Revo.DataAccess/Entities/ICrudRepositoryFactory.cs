using System;
using System.Collections.Generic;
using System.Text;

namespace Revo.DataAccess.Entities
{
    public interface ICrudRepositoryFactory<out TRepository> where TRepository : IReadRepository
    {
        TRepository Create();
    }
}
