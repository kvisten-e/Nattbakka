namespace nattbakka_server.Models
{
    public class TransactionWithGroup
    {
        public int id { get; set; }
        public string tx { get; set; }
        public string address { get; set; }
        public double sol { get; set; }
        public bool sol_changed { get; set; }
        public int dex_id { get; set; }
        public int group_id { get; set; }

        public DateTime timestamp { get; set; }

        public int total_wallets { get; set; }
        public int inactive_wallets { get; set; }
        public int time_different_unix { get; set; }
        public DateTime created { get; set; }
    }
}
