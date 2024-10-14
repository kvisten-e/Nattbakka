using System.Numerics;

namespace nattbakka_server.Models
{
    public class Group
    {
        public int id { get; set; }
        public int total_wallets { get; set; }
        public int inactive_wallets { get; set; }
        public int time_different_unix { get; set; }
        public DateTime created { get; set; }

        public ICollection<Transaction> transactions { get; set; }
    }
}
