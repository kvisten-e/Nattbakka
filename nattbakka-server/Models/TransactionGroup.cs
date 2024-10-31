namespace nattbakka_server.Models
{
    public class TransactionGroup
    {
        public int Id { get; set; } 
        public DateTime Created { get; set; } 
        public int TimeDifferentUnix { get; set; } 
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
