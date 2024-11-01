using nattbakka_server.Models;
using Org.BouncyCastle.Crypto.Agreement.Srp;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using System.Text.Json;
using Solnet.Wallet;
using Newtonsoft.Json;
using nattbakka_server.Helpers;

namespace nattbakka_server.Services

{
    public class SolanaServices
    {
        private List<string> _apiKeys;
        private readonly RandomizeRpcEndpoint _randomizeRpcEndpoint = new RandomizeRpcEndpoint();
        public SolanaServices(List<string> apiKeys)
        {
            if (apiKeys == null || apiKeys.Count == 0)
            {
                throw new InvalidOperationException("API keys cannot be null or empty. Please configure at least one API key.");
            }
            _apiKeys = apiKeys;
        }
        public async Task<dynamic> GetConfirmedTransactionAsync(string signature)
        {
            int attempts = 0;
            while(attempts < 10)
            {
                var transactionDetails = await Rpc().GetTransactionAsync(signature);

                if (!transactionDetails.WasSuccessful)
                {
                    attempts++;
                    // Console.WriteLine($"Failed with rpc: {api}");
                    Thread.Sleep(1000);
                    continue;
                }

                return transactionDetails;
            }
            Console.WriteLine($"Failed to get confirmed signature: {signature} - rip");
            return null;
        }


        public async Task<double> GetAddressBalance(string address)
        {
            var rpc = Rpc();
            var balance = await rpc.GetBalanceAsync(new PublicKey(address));
            if (balance.WasSuccessful)
            {
                var balanceConverted = ConvertLamportsToSol(balance.Result.Value);
                return balanceConverted;
            }
            throw new Exception(balance.Reason);
        }

        public IRpcClient Rpc()
        {
            string api = _randomizeRpcEndpoint.RandomListApiKeys(_apiKeys);
            string wss = $"https://rpc.shyft.to?api_key={api}";
            return ClientFactory.GetClient(wss);
        }

        public string RotateApiList()
        {
            List<string> backupApiKeys = _apiKeys;
            string currentApi = _apiKeys[0];
            try
            {
                _apiKeys.Remove(currentApi);
                _apiKeys.Add(currentApi);
            }
            catch
            {
                _apiKeys = backupApiKeys;
            }
            return currentApi;
        }

        public static double ConvertLamportsToSol(ulong lamports)
        {
            return (double)(lamports / 1_000_000_000);
        }
    }
}