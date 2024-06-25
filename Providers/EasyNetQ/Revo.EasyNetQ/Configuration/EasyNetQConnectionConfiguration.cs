namespace Revo.EasyNetQ.Configuration
{
    public class EasyNetQConnectionConfiguration(string connectionString)
    {
        public string ConnectionString { get; } = connectionString;
    }
}
