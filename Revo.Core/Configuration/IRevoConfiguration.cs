namespace Revo.Core.Configuration
{
    public interface IRevoConfiguration
    {
        T GetSection<T>() where T : class, IRevoConfigurationSection, new();
    }
}
