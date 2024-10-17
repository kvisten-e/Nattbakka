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
        public async Task<ParsedTransaction> GetConfirmedTransactionAsync(string signature)
        {
            int attempts = 0;
            while(attempts < 10)
            {
                var transactionDetails = await Rpc().GetTransactionAsync(signature);

                if (!transactionDetails.WasSuccessful)
                {
                    attempts++;
                    Console.WriteLine($"Failed to parse signature: {signature} - Attempt left: {10 - attempts}");
                    Thread.Sleep(1000);
                    continue;
                }

                double sol = (double)(transactionDetails.Result.Meta.PreBalances[0] - transactionDetails.Result.Meta.PostBalances[0]) / 1_000_000_000;
                
                string sendingAddress = transactionDetails.Result.Transaction.Message.AccountKeys[0];
                string receivingAddress = transactionDetails.Result.Transaction.Message.AccountKeys[1];
                
                if (sendingAddress == "5tzFkiKscXHK5ZXCGbXZxdw7gTjjD1mBwuoFbhUvuAi9")
                {
                    receivingAddress = transactionDetails.Result.Transaction.Message.AccountKeys[2];
                }

                var parsedTransaction = new ParsedTransaction
                {
                    tx = signature,
                    receivingAddress = receivingAddress,
                    sendingAddress = sendingAddress,
                    sol = sol
                };

                return parsedTransaction;
            }
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