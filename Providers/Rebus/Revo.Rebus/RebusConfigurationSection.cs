using Revo.Core.Configuration;

namespace Revo.Rebus
{
    public class RebusConfigurationSection : IRevoConfigurationSection
    {
        public bool IsActive { get; set; }

        public RebusConnectionConfiguration Connection { get; set; } =
            RebusConnectionConfiguration.FromConnectionName("RabbitMQ");
    }
}
