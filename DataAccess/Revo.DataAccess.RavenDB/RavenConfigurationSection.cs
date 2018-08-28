using Revo.Core.Configuration;

namespace Revo.DataAccess.RavenDB
{
    public class RavenConfigurationSection : IRevoConfigurationSection
    {
        public bool IsActive { get; set; }
        public bool UseAsPrimaryRepository { get; set; }
        public bool UseProjections { get; set; }

        public RavenConnectionConfiguration Connection { get; set; } =
            RavenConnectionConfiguration.FromConnectionName("RavenDB");
    }
}
