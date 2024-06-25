using System.Data.Entity.Infrastructure;

namespace Revo.EF6.DataAccess
{
    public class EF6ConnectionConfiguration(IDbConnectionFactory connectionFactory, string nameOrConnectionString)
    {
        public IDbConnectionFactory ConnectionFactory { get; } = connectionFactory;
        public string NameOrConnectionString { get; } = nameOrConnectionString;
    }
}
