using System;
using System.Threading.Tasks;

namespace GTRevo.Transactions
{
    public class UnitOfWorkTransaction : ITransaction
    {
        private readonly ITransaction[] innerTransactions;
        private readonly IUnitOfWorkListener[] unitOfWorkListeners;

        private bool disposedValue = false; // To detect redundant calls

        public UnitOfWorkTransaction(ITransaction[] innerTransactions,
            IUnitOfWorkListener[] unitOfWorkListeners)
        {
            this.innerTransactions = innerTransactions;
            this.unitOfWorkListeners = unitOfWorkListeners;
        }

        public void Commit()
        {
            //TODO: failure handling
            foreach (ITransaction transaction in innerTransactions)
            {
                transaction.Commit();
            }

            throw new NotImplementedException();
        }

        public async Task CommitAsync()
        {
            //TODO: failure handling
            foreach (ITransaction transaction in innerTransactions)
            {
                await transaction.CommitAsync();
            }

            foreach (var listener in unitOfWorkListeners)
            {
                await listener.OnTransactionSucceededAsync(this);
            }
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~UnitOfWorkTransaction() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
