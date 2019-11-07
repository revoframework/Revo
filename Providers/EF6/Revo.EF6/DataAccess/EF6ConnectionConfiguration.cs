using System.Data.Entity.Infrastructure;

namespace Revo.EF6.DataAccess
{
    public class EF6ConnectionConfiguration
    {
        public EF6ConnectionConfiguration(IDbConnectionFactory connectionFactory, string nameOrConnectionString)
        {
            ConnectionFactory = connectionFactory;
            NameOrConnectionString = nameOrConnectionString;
        }

        public IDbConnectionFactory ConnectionFactory { get; }
        public string NameOrConnectionString { get; }
    }
}
