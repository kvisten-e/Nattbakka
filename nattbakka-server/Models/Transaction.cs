namespace nattbakka_server.Models
{
    public class Transaction
    {
        public int id { get; set; }
        public string tx { get; set; }
        public string address { get; set;}
        public double sol { get; set;}
        public bool sol_changed { get; set; }
        public int dex_id { get; set;}
        public int group_id { get; set;}

        public DateTime timestamp { get; set; }

        public Group dex_groups { get; set; }
    }
}
