namespace Revo.Platforms.AspNet.Core
{
    public interface IConfiguration
    {
        T GetSection<T>(string sectionName);
    }
}
