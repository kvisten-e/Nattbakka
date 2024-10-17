using System.Linq;

namespace nattbakka_server.Helpers
{
    public class RandomizeRpcEndpoint
    {
        private readonly Random _rand;

        public RandomizeRpcEndpoint()
        {
            _rand = new Random();
        }

        public string RandomListApiKeys(List<string> listToShuffle)
        {
            var shuffledList = listToShuffle.OrderBy(_ => _rand.Next()).ToList();
            return shuffledList.FirstOrDefault();
        }
    }
}
