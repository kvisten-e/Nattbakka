using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace nattbakka_server
{
    public class WebSockets
    {
        public async void StartWebsockets()
        {
            string apiKey = "OytKNDHdJokVRhcx";
            string wssUri = $"wss://rpc.shyft.to?api_key={apiKey}";
            string accountPublicKey = "E7CaDzECPzftEPTtdbjrUGP4yThofKCqBpu9u4X2MaFh";

            List<string> dexes = new List<string>
            {
                "E7CaDzECPzftEPTtdbjrUGP4yThofKCqBpu9u4X2MaFh",
                "HpnkysyPxaiiXraoG5Ggc82EhAVfWFR9EK6hwHSGQi3U",
            };

            using (ClientWebSocket ws = new ClientWebSocket())
            {
                Console.WriteLine("Connecting to websocket");
                await ws.ConnectAsync(new Uri(wssUri), CancellationToken.None);
                Console.WriteLine("Connected");


                var websocketRequest = new
                {
                    jsonrpc = "2.0",
                    id = 1,
                    method = "logsSubscribe",
                    @params = new object[]
                    {
                new
                {
                    mentions = new[] { accountPublicKey }
                },
                new
                {
                    commitment = "finalized"
                }
                            }
                        };

                string requestJson = JsonConvert.SerializeObject(websocketRequest);
                byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);

                Console.WriteLine("Sending account subscription request...");
                await ws.SendAsync(new ArraySegment<byte>(requestBytes), WebSocketMessageType.Text, true, CancellationToken.None);

                byte[] buffer = new byte[1024 * 4];

                while (ws.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    string responseJson = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    var jsonObject = JObject.Parse(responseJson);
                    string signature = (string)jsonObject["params"]?["result"]?["value"]?["signature"];
                    Console.WriteLine("Signature: " + signature);


                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        Console.WriteLine("WebSocket closed.");
                    }
                }
            }
        }

    }
}
