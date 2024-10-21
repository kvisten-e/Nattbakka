﻿using nattbakka_server.Data;
using nattbakka_server.Models;
using Solnet.Programs.TokenSwap.Models;
using WebSocketSharp;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using Newtonsoft.Json;
using nattbakka_server.Helpers;

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
            if (transactionDetails == null) return;

            string json = JsonConvert.SerializeObject(transactionDetails, Formatting.Indented);
            Console.WriteLine(json);

            var parsedTransaction = _transactionTemplate.ParsedTransaction(transactionDetails);

            if (parsedTransaction.sendingAddress != cex.address || parsedTransaction.sol < 0.01 && parsedTransaction.sol > 5000) return;

            parsedTransaction.signature = signature;

            Console.WriteLine($"ParsedTransaction: {parsedTransaction.signature} \nReceiving: {parsedTransaction.receivingAddress} \nSender: {(cex.address == parsedTransaction.sendingAddress ? cex.name : parsedTransaction.sendingAddress)}");

            //await _databaseComponents.PostTransaction(parsedTransaction, cex.id);

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
