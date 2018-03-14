namespace Revo.Core.Core
{
    public interface IConfiguration
    {
        T GetSection<T>(string sectionName);
    }
}
