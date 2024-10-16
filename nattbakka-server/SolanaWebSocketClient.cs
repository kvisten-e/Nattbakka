using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using WebSocketSharp;

namespace nattbakka_server
{
    public class SolanaWebSocketClient
    {
        private List<string> _apiKeys;
        public SolanaWebSocketClient(List<string> apiKeys) {
            
            _apiKeys = new List<string>(apiKeys);
        }

        public async Task<WebSocket> CreateWebSocketConnection(string cex)
        {
            if (_apiKeys.Count == 0)
            {
                throw new InvalidOperationException("No API keys available.");
            }

            string apiKey = _apiKeys[new Random().Next(_apiKeys.Count)];
            string wssUri = $"wss://rpc.shyft.to?api_key={apiKey}";
            string wssHelius = "wss://mainnet.helius-rpc.com/?api-key=6f9b778a-be64-496b-b57e-158b59c3fd25";
            var websocketRequest = new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "logsSubscribe",
                @params = new object[]
                {
                    new
                    {
                        mentions = new[] { cex }
                    },
                    new
                    {
                        commitment = "confirmed"
                    }
                }
            };
            string requestJson = JsonConvert.SerializeObject(websocketRequest);
            byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);

            WebSocket ws = new WebSocket(wssHelius);
            ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            ws.Log.Level = WebSocketSharp.LogLevel.Trace;
            ws.Log.Output = (data, file) => Console.WriteLine($"[{file}] {data}");
            ws.OnError += Ws_OnError;
            ws.OnClose += Ws_OnClose;

            await Task.Run(() =>
            {
                ws.Connect();
            });

            if (ws.ReadyState == WebSocketState.Open)
            {
                ws.Send(requestBytes);
            }

            return ws;

        }

        private void Ws_OnError(object? sender, WebSocketSharp.ErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Ws_OnClose(object? sender, CloseEventArgs e)
        {
            Console.WriteLine("WebSocket closed: " + e.Reason);
        }
    }
}
