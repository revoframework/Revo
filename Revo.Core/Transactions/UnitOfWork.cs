using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Events;

namespace Revo.Core.Transactions
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly List<ITransaction> innerTransactions = new List<ITransaction>();
        private readonly Lazy<IUnitOfWorkListener[]> unitOfWorkListeners;
        
        private bool disposedValue = false; // To detect redundant calls

        public UnitOfWork(Lazy<IUnitOfWorkListener[]> unitOfWorkListeners,
            IPublishEventBuffer publishEventBuffer)
        {
            this.unitOfWorkListeners = unitOfWorkListeners;

            EventBuffer = publishEventBuffer;
        }
        
        public IPublishEventBuffer EventBuffer { get; }
        public bool IsWorkBegun { get; private set; }

        public void Begin()
        {
            if (IsWorkBegun)
            {
                throw new InvalidOperationException($"This {this} has already been started");
            }

            foreach (var listener in unitOfWorkListeners.Value)
            {
                listener.OnWorkBegin(this);
            }

            IsWorkBegun = true;
        }

        public void AddInnerTransaction(ITransaction innerTransaction)
        {
            innerTransactions.Add(innerTransaction);
        }

        public async Task CommitAsync()
        {
            if (!IsWorkBegun)
            {
                throw new InvalidOperationException($"This {this} has not been started yet");
            }
            
            foreach (var listener in unitOfWorkListeners.Value)
            {
                await listener.OnBeforeWorkCommitAsync(this);
            }

            foreach (ITransaction transaction in innerTransactions)
            {
                await transaction.CommitAsync();
            }
            
            await EventBuffer.FlushAsync(new CancellationToken());

            foreach (var listener in unitOfWorkListeners.Value)
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
