namespace Revo.EF6.DataAccess
{
    public class EF6ConnectionConfiguration
    {
        public EF6ConnectionConfiguration(string nameOrConnectionString)
        {
            NameOrConnectionString = nameOrConnectionString;
        }

        public string NameOrConnectionString { get; }
    }
}
