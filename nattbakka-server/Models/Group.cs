using System.Numerics;

namespace nattbakka_server.Models
{
    public class Group
    {
        public int id { get; set; }
        public int time_different_unix { get; set; }
        public DateTime created { get; set; }

    }
}
