namespace Revo.DataAccess.Entities
{
    public interface IFilteringRepository<out T>
    {
        T IncludeFilters(params IRepositoryFilter[] repositoryFilters);
        T ExcludeFilter(params IRepositoryFilter[] repositoryFilters);
        T ExcludeFilters<TRepositoryFilter>() where TRepositoryFilter : IRepositoryFilter;
    }
}
