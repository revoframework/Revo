namespace Revo.Rebus
{
    public class RebusConnectionConfiguration
    {
        public static RebusConnectionConfiguration FromConnectionName(string connectionName)
        {
            return new RebusConnectionConfiguration(connectionName, null);
        }

        public static RebusConnectionConfiguration FromConnectionString(string connectionString)
        {
            return new RebusConnectionConfiguration(null, connectionString);
        }

        private RebusConnectionConfiguration(string connectionName, string connectionString)
        {
            ConnectionName = connectionName;
            ConnectionString = connectionString;
        }

        public string ConnectionName { get; }
        public string ConnectionString { get; }
    }
}
