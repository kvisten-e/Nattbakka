using nattbakka_server.Data;
using nattbakka_server.Models;
using System.Collections.Concurrent;


namespace nattbakka_server.Services
{
    public class DexService
    {
        private readonly DexRepository _dexRepository;
        private readonly ConcurrentDictionary<string, WebSocketClient> _activeWebSockets;
        //private readonly TransactionRepository _transactionRepository;

        public DexService(DexRepository dexRepository)
        {
            _dexRepository = dexRepository;
            _activeWebSockets = new ConcurrentDictionary<string, WebSocketClient>();
            //_transactionRepository = transactionRepository;
        }

        public async Task MonitorDexesAsync()
        {
            List<Dex> dexes = await _dexRepository.GetDexesAsync();

            foreach (Dex dex in dexes)
            {
                if (dex.active)
                {
                    Console.WriteLine(dex.name);
                    var webSocketClient = new WebSocketClient();
                    await webSocketClient.ConnectAsync();
                    await webSocketClient.SendAsync(dex.address);

                    _activeWebSockets.TryAdd(dex.name, webSocketClient);
                    Console.WriteLine($"Active WebSockets: {_activeWebSockets.Count}");
                    GetActiveDexNames();

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            while (true)
                            {
                                string message = await webSocketClient.ReceiveMessageAsync();

                                Console.WriteLine("Message: " + message);


                                //var transaction = ParseTransaction(message, dex.Id);
                                //await _transactionRepository.SaveTransactionAsync(transaction);
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"Error receiving message from {dex.name}: {ex.Message}");
                        }
                        finally
                        {
                            _activeWebSockets.TryRemove(dex.address, out _);
                            Console.WriteLine($"WebSocket closed. Active WebSockets: {_activeWebSockets.Count}");
                        }


                    });
                }
            }
        }

        public List<string> GetActiveDexNames()
        {
            Console.WriteLine($"Checking active WebSockets. Current count: {_activeWebSockets.Count()}");

            List<string> activeDexes = new List<string>();
            foreach (string name in _activeWebSockets.Keys)
            {
                Console.WriteLine($"Active DEX name: {name}");
                activeDexes.Add(name);
            }

            return activeDexes;
        }



        //private Transaction ParseTransaction(string message, int dexId)
        //{
        //    // Parse the WebSocket message and return a transaction object
        //    // This is a placeholder for actual message parsing logic
        //    return new Transaction
        //    {
        //        tx = dexId,
        //        TxHash = "parsed_tx_hash",   // Extract tx_hash from message
        //        Amount = 0.5M,               // message amount from message
        //        Timestamp = DateTime.Now     // Extract timestamp from message
        //    };
        //}
    }


}
