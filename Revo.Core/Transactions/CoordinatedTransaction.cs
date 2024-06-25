using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Core.Transactions
{
    public class CoordinatedTransaction(ITransaction innerTransaction) : ITransactionCoordinator, ITransaction
    {
        private int transactionCounter = 0;

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
            var usedParticipants = new HashSet<ITransactionParticipant>();

            while (usedParticipants.Count < Participants.Count)
            {
                foreach (var participant in Participants.ToArray())
                {
                    await participant.OnBeforeCommitAsync();
                    usedParticipants.Add(participant);
                }
            }

            try
            {
                await innerTransaction.CommitAsync();
            }
            catch (Exception)
            {
                foreach (var participant in usedParticipants)
                {
                    await participant.OnCommitFailedAsync();
                }

                throw;
            }

            foreach (var participant in usedParticipants)
            {
                await participant.OnCommitSucceededAsync();
            }
        }

        public void Dispose()
        {
        }
    }
}
