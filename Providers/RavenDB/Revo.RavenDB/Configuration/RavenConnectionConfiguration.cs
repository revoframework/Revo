namespace Revo.RavenDB.Configuration
{
    public class RavenConnectionConfiguration
    {
        public static RavenConnectionConfiguration FromConnectionString(string connectionString)
        {
            return new RavenConnectionConfiguration(connectionString);
        }

        private RavenConnectionConfiguration(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }
    }
}
