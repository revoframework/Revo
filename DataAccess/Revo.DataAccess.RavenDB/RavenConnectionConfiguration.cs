namespace Revo.DataAccess.RavenDB
{
    public class RavenConnectionConfiguration
    {
        public static RavenConnectionConfiguration FromConnectionName(string connectionName)
        {
            return new RavenConnectionConfiguration(connectionName, null);
        }

        public static RavenConnectionConfiguration FromConnectionString(string connectionString)
        {
            return new RavenConnectionConfiguration(null, connectionString);
        }

        private RavenConnectionConfiguration(string connectionName, string connectionString)
        {
            ConnectionName = connectionName;
            ConnectionString = connectionString;
        }

        public string ConnectionName { get; }
        public string ConnectionString { get; }
    }
}
