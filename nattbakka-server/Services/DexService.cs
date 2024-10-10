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

        public async Task MonitorDexesAsync(List<string> apiKeys)
        {
            List<Dex> dexes = await _databaseComponents.GetDexesAsync();
            SolanaServices solanaServices = new SolanaServices(apiKeys);

            foreach (Dex dex in dexes)
            {
                var webSocketClient = new WebSocketClient(apiKeys);
                await StartWebSocket(webSocketClient, dex);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        while (true)
                        {
                            string signature = await webSocketClient.ReceiveMessageAsync();

                            if (prevMessages.TryAdd(signature, true) && signature.Length > 0)
                            {
                                var parsedTransaction = await solanaServices.GetConfirmedTransactionAsync(signature);
                                await _databaseComponents.PostTransaction(parsedTransaction, dex.id);
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

        public async Task StartWebSocket(WebSocketClient webSocketClient, Dex dex)
        {
            await webSocketClient.ConnectAsync();
            await webSocketClient.SendAsync(dex.address);
            _activeWebSockets.Add(dex.name);
        }

        public List<string> GetActiveDexNames()
        {
            Console.WriteLine($"Checking active WebSockets. Current count: {_activeWebSockets.Count()}");
            return _activeWebSockets;
        }


    }
}
