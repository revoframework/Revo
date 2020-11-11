namespace Revo.Rebus
{
    public class RebusConnectionConfiguration
    {
        public static RebusConnectionConfiguration FromConnectionString(string connectionString)
        {
            return new RebusConnectionConfiguration(connectionString);
        }

        private RebusConnectionConfiguration(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }
    }
}
