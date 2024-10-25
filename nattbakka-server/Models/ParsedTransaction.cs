namespace nattbakka_server.Models
{
    public class ParsedTransaction
    {
        public string signature { get; set; }

        public int cex_id { get; set; }
        public string receivingAddress { get; set; }
        public string sendingAddress { get; set; }
        public double sol { get; set; }
    }
}
