using nattbakka_server.Data;
using nattbakka_server.Models;
using System.Collections.Concurrent;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Types;
using System.Text.Json;

namespace nattbakka_server.Services
{
    public class DexService
    {
        private readonly DatabaseComponents _databaseComponents;
        private readonly List<string> _activeWebSockets = new List<string>();
        private readonly ConcurrentDictionary<string, bool> prevMessages = new ConcurrentDictionary<string, bool>();

        public DexService(DatabaseComponents databaseComponents)
        {
            _databaseComponents = databaseComponents;
        }

        public async Task MonitorDexesAsync(List<string> apiKeysSection)
        {
            List<Dex> dexes = await _databaseComponents.GetDexesAsync();

            foreach (Dex dex in dexes)
            { 
                if (dex.active)
                {
                    var webSocketClient = new WebSocketClient(apiKeysSection);
                    SolanaServices solanaServices = new SolanaServices(apiKeysSection);
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
                                string signature = await webSocketClient.ReceiveMessageAsync();

                                if (prevMessages.TryAdd(signature, true) && signature.Length > 0)
                                {
                                    Console.WriteLine($"Message from {dex.name}: {signature}");
                                    var parsedTransaction = await solanaServices.GetConfirmedTransactionAsync(signature);
                                    Console.WriteLine("Starting job for PostTransaction: " + parsedTransaction.sol);
                                    await _databaseComponents.PostTransaction(parsedTransaction, dex.id);

                                    //string testing = JsonSerializer.Serialize(parsedTransaction);

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


    }
}
