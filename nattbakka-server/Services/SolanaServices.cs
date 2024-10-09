using nattbakka_server.Models;
using Org.BouncyCastle.Crypto.Agreement.Srp;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using System.Text.Json;

namespace nattbakka_server.Services

{
    public class SolanaServices
    {
        //private readonly IRpcClient _rpcClient;
        private readonly List<string> _apiKeys;

        public SolanaServices(List<string> apiKeys)
        {
            _apiKeys = apiKeys;

        }
        //
        public async Task<ParsedTransaction> GetConfirmedTransactionAsync(string signature)
        {
            var rpc = Rpc();
            if (rpc is null) return null;

            // Fetch the transaction details using the RPC client
            var transactionDetails = await rpc.GetTransactionAsync(signature);

            if (!transactionDetails.WasSuccessful)
            {
                throw new Exception(transactionDetails.Reason);
            }

            string jsonString = JsonSerializer.Serialize(transactionDetails);

            //Console.WriteLine("transactionDetails: " + jsonString);

            double sol = (double)(transactionDetails.Result.Meta.PreBalances[0] - transactionDetails.Result.Meta.PostBalances[0]) / 1_000_000_000;

            var parsedTransaction = new ParsedTransaction
            {
                tx = signature,
                receivingAddress = transactionDetails.Result.Transaction.Message.AccountKeys[1],
                sendingAddress = transactionDetails.Result.Transaction.Message.AccountKeys[0],
                sol = sol
            };

            return parsedTransaction;
        }



        public IRpcClient Rpc()
        {
            string api = RotateApiList();
            Console.WriteLine("Api: " + api);
            if (api is null) return null;

            return ClientFactory.GetClient($"https://rpc.shyft.to?api_key={api}");
        }

        public string RotateApiList()
        {
            if (_apiKeys.Count == 0) return null;

            string currentApi = _apiKeys[0];
            _apiKeys.Remove(currentApi);
            _apiKeys.Add(currentApi);
            return currentApi;
        }
    }
}