using nattbakka_server.Data;
using nattbakka_server.Models;
using WebSocketSharp;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using nattbakka_server.Helpers;
using Microsoft.Extensions.Options;
using nattbakka_server.Options;
using Microsoft.EntityFrameworkCore;


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
        private readonly CexTransactionTemplate _transactionTemplate = new CexTransactionTemplate();
        private readonly List<string> _apiKeysShyft;
        private readonly List<string> _apiKeysHelius;
        


        public CexTransactionsService(DatabaseComponents databaseComponents, IOptions<RpcApiKeysOptions> rpcApiKeysOptions)
        {
            _databaseComponents = databaseComponents;
            _apiKeysShyft = rpcApiKeysOptions.Value.ShyftApiKeys;
            _apiKeysHelius = rpcApiKeysOptions.Value.HeliusApiKeys;
            
        }

        public async Task SolanaTransactionsWebSocket()
        {
            _solanaServices = new SolanaServices(_apiKeysShyft);
            _cexList = await _databaseComponents.GetCexesAsync();
            var solanaWs = new SolanaWebSocketClient(_apiKeysHelius);

            foreach (Cex cex in _cexList)
            {
                await CreateAndMonitorWebSocket(solanaWs, cex);
            }

            int counter = 1;
            foreach(var acw in _activeWs)
            {
                Console.WriteLine($"{counter++}. Websocket for {acw.CexName} started");
            }

        }

        private async Task CreateAndMonitorWebSocket(SolanaWebSocketClient solanaWs, Cex cex)
        {
            string address = cex.address;
            string name = cex.name;
            var ws = await solanaWs.CreateWebSocketConnection(address);
            ws.OnClose += async (sender, e) => await OnWebSocketClosed(cex, solanaWs, e);
            ws.OnMessage += async (sender, e) => await OnWebSocketMessage(cex, sender, e);

            ActiveCexWebSockets acw = new(name, ws);
            _activeWs.Add(acw);
        }

        private async Task OnWebSocketMessage(Cex cex, object? sender, MessageEventArgs e)
        {
            var jsonObject = JObject.Parse(e.Data);
            string signature = (string)jsonObject["params"]?["result"]?["value"]?["signature"];

            if (!string.IsNullOrEmpty(signature) && _prevMessages.TryAdd(signature, true))
            {
                await ParseSignatureFromMessage(signature, cex);
            }
        }

        private async Task ParseSignatureFromMessage(string signature, Cex cex)
        {
            var transactionDetails = await _solanaServices.GetConfirmedTransactionAsync(signature);

            if (transactionDetails == null ||
                transactionDetails?.Result.Meta.InnerInstructions.Length != 0 ||
                transactionDetails?.Result.Meta.PreTokenBalances.Length != 0 ||
                transactionDetails?.Result.Meta.PostTokenBalances.Length != 0)
            {
                return;
            };


            var parsedTransaction = _transactionTemplate.ParsedTransaction(transactionDetails, cex);

            if (parsedTransaction is null || parsedTransaction.sendingAddress != cex.address || parsedTransaction.sol < 0.01 && parsedTransaction.sol > 5000) {
                return;
            };


            // Save to database directly
            parsedTransaction.signature = signature;
            parsedTransaction.cex_id = cex.id;


            await _databaseComponents.PostTransactionInMemory(parsedTransaction);




        }

        private async Task OnWebSocketClosed(Cex cex, SolanaWebSocketClient solanaWs, CloseEventArgs e)
        {
            Console.WriteLine($"WebSocket for {cex.name} closed: {e.Reason}");

            if (_shouldReconnect)
            {
                Console.WriteLine($"Attempting to reconnect WebSocket for {cex.name}...");
                await Task.Delay(5000);
                await CreateAndMonitorWebSocket(solanaWs, cex); 
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
