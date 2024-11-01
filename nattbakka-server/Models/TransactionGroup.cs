namespace nattbakka_server.Models
{
    public class TransactionGroup
    {
        public string Id { get; set; } 
        public DateTime Created { get; set; } 
        public int TimeDifferentUnix { get; set; } 
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
