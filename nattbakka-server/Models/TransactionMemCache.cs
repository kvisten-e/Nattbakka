namespace nattbakka_server.Models
{
    public class TransactionMemCache
    {
        public string signature { get; set; }
        public string address { get; set; }
        public double sol { get; set; }
        public bool sol_changed { get; set; }
        public int cex_id { get; set; }
        public int group_id { get; set; }

        public DateTime timestamp { get; set; }

    }
}
