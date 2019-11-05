namespace Revo.Core.Core
{
    public interface IEnvironmentProvider
    {
        bool? IsDevelopment { get; }
    }
}