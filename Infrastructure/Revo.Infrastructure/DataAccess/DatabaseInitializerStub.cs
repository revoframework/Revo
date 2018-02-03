namespace Revo.Infrastructure.DataAccess
{
    public abstract class DatabaseInitializerStub : IDatabaseInitializer
    {
        private bool isInitialized = false;

        public void Initialize()
        {
            if (!isInitialized)
            {
                DoInitialize();
                isInitialized = true;
            }
        }

        protected abstract void DoInitialize();
    }
}
