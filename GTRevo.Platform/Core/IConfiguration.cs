namespace GTRevo.Platform.Core
{
    public interface IConfiguration
    {
        T GetSection<T>(string sectionName);
    }
}
