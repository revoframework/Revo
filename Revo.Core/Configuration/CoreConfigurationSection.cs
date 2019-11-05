namespace Revo.Core.Configuration
{
    public class CoreConfigurationSection : IRevoConfigurationSection
    {
        /// <summary>
        /// Can override if the current environment is treated as a development environment (contrary to production environment).
        /// This may affect logging, database migration, etc.
        /// </summary>
        public bool? IsDevelopmentEnvironment { get; set; }

        public bool AutoDiscoverCommandHandlers { get; set; } = true;
        public bool AutoDiscoverAutoMapperProfiles { get; set; } = true;
    }
}
