using nattbakka_server.Data;
using nattbakka_server.Models;
using System.Collections.Concurrent;

namespace nattbakka_server.Services
{
    public class DexService
    {
        private readonly DexRepository _dexRepository;
        private readonly List<string> _activeWebSockets = new List<string>();
        private readonly ConcurrentDictionary<string, bool> prevMessages = new ConcurrentDictionary<string, bool>();

        public DexService(DexRepository dexRepository)
        {
            _dexRepository = dexRepository;
        }

        public async Task MonitorDexesAsync(List<string> apiKeysSection)
        {
            List<Dex> dexes = await _dexRepository.GetDexesAsync();

            foreach (Dex dex in dexes)
            {
                if (dex.active)
                {
                    var webSocketClient = new WebSocketClient(apiKeysSection);
                    await webSocketClient.ConnectAsync();
                    await webSocketClient.SendAsync(dex.address);
                    _activeWebSockets.Add(dex.name);

                    Console.WriteLine($"Active WebSockets: {_activeWebSockets.Count}");
                    GetActiveDexNames();

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            while (true)
                            {
                                string message = await webSocketClient.ReceiveMessageAsync();

                                if (prevMessages.TryAdd(message, true))
                                {
                                    Console.WriteLine($"Message from {dex.name}: {message}");

                                }
                                else
                                {
                                    Console.WriteLine("Exist! " + message);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error receiving message from {dex.name}: {ex.Message}");
                        }
                        finally
                        {
                            _activeWebSockets.Remove(dex.address);
                            Console.WriteLine($"WebSocket closed. Active WebSockets: {_activeWebSockets.Count}");
                        }
                    });
                }
            }
        }

        public List<string> GetActiveDexNames()
        {
            Console.WriteLine($"Checking active WebSockets. Current count: {_activeWebSockets.Count()}");
            return _activeWebSockets;
        }

        // private Transaction ParseTransaction(string message, int dexId)
        // {
        //     // Parse the WebSocket message and return a transaction object
        //     // This is a placeholder for actual message parsing logic
        //     return new Transaction
        //     {
        //         tx = dexId,
        //         TxHash = "parsed_tx_hash",   // Extract tx_hash from message
        //         Amount = 0.5M,               // message amount from message
        //         Timestamp = DateTime.Now     // Extract timestamp from message
        //     };
        // }
    }
}
