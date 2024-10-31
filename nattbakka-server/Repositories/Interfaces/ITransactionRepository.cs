using nattbakka_server.Models;


namespace nattbakka_server.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        Task<TransactionGroup> AddTransactionGroupAsync(TransactionGroup group);
        Task<Transaction> AddTransactionAsync(Transaction transaction);

        // Task<TransactionGroup> GetTransactionGroupAsync(int groupId);

    }
}
