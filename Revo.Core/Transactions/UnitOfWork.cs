using System;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Events;

namespace Revo.Core.Transactions
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ITransaction[] innerTransactions;
        private readonly IUnitOfWorkListener[] unitOfWorkListeners;

        private bool disposedValue = false; // To detect redundant calls

        public UnitOfWork(ITransaction[] innerTransactions,
            IUnitOfWorkListener[] unitOfWorkListeners,
            IPublishEventBuffer publishEventBuffer)
        {
            this.innerTransactions = innerTransactions;
            this.unitOfWorkListeners = unitOfWorkListeners;

            EventBuffer = publishEventBuffer;

            foreach (var listener in unitOfWorkListeners)
            {
                listener.OnWorkBegin(this);
            }
        }
        
        public IPublishEventBuffer EventBuffer { get; }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public async Task CommitAsync()
        {
            foreach (var listener in unitOfWorkListeners)
            {
                await listener.OnBeforeWorkCommitAsync(this);
            }

            foreach (ITransaction transaction in innerTransactions)
            {
                await transaction.CommitAsync();
            }

            // TODO flushing events even on an exception?
            await EventBuffer.FlushAsync(new CancellationToken()); // TODO cancellation token

            foreach (var listener in unitOfWorkListeners)
            {
                await listener.OnWorkSucceededAsync(this);
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
        // ~UnitOfWork() {
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
