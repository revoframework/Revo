using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Transactions;

namespace Revo.EFCore.UnitOfWork
{
    public class CoordinatedTransaction : ITransactionCoordinator, ITransaction
    {
        private readonly ITransaction innerTransaction;
        private int transactionCounter = 0;

        public CoordinatedTransaction(ITransaction innerTransaction)
        {
            this.innerTransaction = innerTransaction;
        }

        protected List<ITransactionParticipant> Participants { get; set; } = new List<ITransactionParticipant>();

        public void AddTransactionParticipant(ITransactionParticipant participant)
        {
            Participants.Add(participant);
        }

        public async Task CommitAsync()
        {
            transactionCounter++;

            try
            {
                if (transactionCounter == 1)
                {
                    await DoCommitAsync();
                }
            }
            finally
            {
                transactionCounter--;
            }
        }

        protected virtual async Task DoCommitAsync()
        {
            foreach (var participant in Participants)
            {
                await participant.OnBeforeCommitAsync();
            }

            try
            {
                await innerTransaction.CommitAsync();
            }
            catch (Exception)
            {
                foreach (var participant in Participants)
                {
                    await participant.OnCommitFailedAsync();
                }

                throw;
            }

            foreach (var participant in Participants)
            {
                await participant.OnCommitSucceededAsync();
            }
        }

        public void Dispose()
        {
        }
    }
}
