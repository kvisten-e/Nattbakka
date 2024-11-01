namespace nattbakka_server.Models
{
    public class Transaction
    {
        public string Id { get; set; }
        public string Tx { get; set; }
        public string Address { get; set;}
        public double Sol { get; set;}
        public bool SolChanged { get; set; }
        public int CexId { get; set;}
        public string GroupId { get; set;}
        public TransactionGroup Group { get; set; }

        public DateTime Timestamp { get; set; }

    }
}
