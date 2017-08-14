using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.DataAccess.Entities
{
    public interface IFilteringRepository<out T>
    {
        T IncludeFilters(params IRepositoryFilter[] repositoryFilters);
        T ExcludeFilter(params IRepositoryFilter[] repositoryFilters);
        T ExcludeFilters<TRepositoryFilter>() where TRepositoryFilter : IRepositoryFilter;
    }
}
