using nattbakka_server.Data;
using nattbakka_server.Models;
using Solnet.Programs.TokenSwap.Models;
using WebSocketSharp;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using Newtonsoft.Json;

namespace nattbakka_server.Services
{

    public record ActiveCexWebSockets(string CexName, WebSocket ws);
    public class CexTransactionsService
    {

        private readonly DatabaseComponents _databaseComponents;
        private ConcurrentBag<ActiveCexWebSockets> _activeWs = new ConcurrentBag<ActiveCexWebSockets>();
        private List<Cex> _cexList = new();
        private bool _shouldReconnect = true;
        private ConcurrentDictionary<string, bool> _prevMessages = new ConcurrentDictionary<string, bool>();
        private SolanaServices? _solanaServices = null;

        public CexTransactionsService(DatabaseComponents databaseComponents) {
            _databaseComponents = databaseComponents;
        }

        public async Task SolanaTransactionsWebSocket(List<string> apiKeys, List<string> apiKeysHelius)
        {
            _solanaServices = new SolanaServices(apiKeys);
            _cexList = await _databaseComponents.GetCexesAsync();
            var solanaWs = new SolanaWebSocketClient(apiKeysHelius);

            foreach (Cex cex in _cexList)
            {
                await CreateAndMonitorWebSocket(solanaWs, cex.address, cex.name);
            }

            int counter = 1;
            foreach(var acw in _activeWs)
            {
                Console.WriteLine($"{counter++}. Websocket for {acw.CexName} started");
            }

        }

        private async Task CreateAndMonitorWebSocket(SolanaWebSocketClient solanaWs, string cexAddress, string cexName)
        {
            var ws = await solanaWs.CreateWebSocketConnection(cexAddress);
            ws.OnClose += async (sender, e) => await OnWebSocketClosed(cexName, cexAddress, solanaWs, e);
            ws.OnMessage += async (sender, e) => await OnWebSocketMessage(cexName, sender, e);

            ActiveCexWebSockets acw = new(cexName, ws);
            _activeWs.Add(acw);
        }

        private async Task OnWebSocketMessage(string cexName, object? sender, MessageEventArgs e)
        {
            var jsonObject = JObject.Parse(e.Data);
            string signature = (string)jsonObject["params"]?["result"]?["value"]?["signature"];
            Console.WriteLine("Signature: " + signature);

            if (!string.IsNullOrEmpty(signature) && _prevMessages.TryAdd(signature, true))
            {
                await ParseSignatureFromMessage(signature, cexName);
            }
        }

        private async Task ParseSignatureFromMessage(string signature, string cexName)
        {
            int cexId = _cexList.Where(n => n.name == cexName).Select(n => n.id).FirstOrDefault();
            var parsedTransaction = await _solanaServices.GetConfirmedTransactionAsync(signature);

            if (parsedTransaction == null || cexId == 0)
            {
                return;
            }

            await _databaseComponents.PostTransaction(parsedTransaction, cexId);
        }

        private async Task OnWebSocketClosed(string cexName, string cexAddress, SolanaWebSocketClient solanaWs, CloseEventArgs e)
        {
            Console.WriteLine($"WebSocket for {cexName} closed: {e.Reason}");

            if (_shouldReconnect)
            {
                Console.WriteLine($"Attempting to reconnect WebSocket for {cexName}...");
                await Task.Delay(5000);
                await CreateAndMonitorWebSocket(solanaWs, cexAddress, cexName); 
            }
        }

        public void CloseAllConnections()
        {
            _shouldReconnect = false;
            foreach (var acw in _activeWs)
            {
                if (acw.ws.ReadyState == WebSocketState.Open)
                {
                    acw.ws.Close();
                    _activeWs.Clear();
                }
            }
        }

        public void EnableAutoReconnect(bool enable)
        {
            _shouldReconnect = enable;
            
        }

    }
}
