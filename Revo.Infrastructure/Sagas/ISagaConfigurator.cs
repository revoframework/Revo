namespace Revo.Infrastructure.Sagas
{
    public interface ISagaConfigurator
    {
        void ConfigureSagas(ISagaRegistry sagaRegistry);
    }
}
