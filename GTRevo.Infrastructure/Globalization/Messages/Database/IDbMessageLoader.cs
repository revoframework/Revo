namespace GTRevo.Infrastructure.Globalization.Messages.Database
{
    public interface IDbMessageLoader
    {
        void EnsureLoaded();
        void Reload();
    }
}