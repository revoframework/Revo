using Revo.Core.Configuration;

namespace Revo.RavenDB.Configuration
{
    public class RavenConfigurationSection : IRevoConfigurationSection
    {
        public bool AutoDiscoverProjectors { get; set; }
        public bool IsActive { get; set; }
        public bool UseAsPrimaryRepository { get; set; }
        public bool UseProjections { get; set; }

        public RavenConnectionConfiguration Connection { get; set; } = null;
    }
}
