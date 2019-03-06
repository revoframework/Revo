namespace Revo.EasyNetQ.Configuration
{
    public class EasyNetQConnectionConfiguration
    {
        public EasyNetQConnectionConfiguration(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }
    }
}
