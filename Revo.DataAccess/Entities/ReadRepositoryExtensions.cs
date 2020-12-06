using System;
using System.Linq;

namespace Revo.DataAccess.Entities
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

        public static T ExcludeFilters<T>(this T repository,
            params IRepositoryFilter[] repositoryFilters) where T : IReadRepository
        {
            IFilteringRepository<IReadRepository> filteringRepository =
                repository as IFilteringRepository<IReadRepository>;
            if (filteringRepository == null)
            {
                throw new ArgumentException(
                    $"Repository type {repository.GetType().FullName} does not implement IFilteringRepository<>");
            }

            return (T)filteringRepository.ExcludeFilter(repositoryFilters);
        }

        public static T ExcludeFilters<T>(this T repository,
            params Type[] repositoryFilterTypes) where T : IReadRepository
        {
            IFilteringRepository<IReadRepository> filteringRepository =
                repository as IFilteringRepository<IReadRepository>;
            if (filteringRepository == null)
            {
                throw new ArgumentException(
                    $"Repository type {repository.GetType().FullName} does not implement IFilteringRepository<>");
            }

            var filters = repository.DefaultFilters.Where(x => repositoryFilterTypes.Any(y => y.IsInstanceOfType(x)));
            return (T)filteringRepository.ExcludeFilter(filters.ToArray());
        }
    }
}
