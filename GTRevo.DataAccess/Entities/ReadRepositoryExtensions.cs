using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.DataAccess.Entities
{
    public static class ReadRepositoryExtensions
    {
        public static T IncludeFilters<T>(this T repository,
            params IRepositoryFilter[] repositoryFilters) where T : IReadRepository
        {
            IFilteringRepository<IReadRepository> filteringRepository =
                repository as IFilteringRepository<IReadRepository>;
            if (filteringRepository == null)
            {
                throw new ArgumentException(
                    $"Repository type {repository.GetType().FullName} does not implement IFilteringRepository<>");
            }

            return (T) filteringRepository.IncludeFilters(repositoryFilters);
        }
    }
}
