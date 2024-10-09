using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Text;

namespace nattbakka_server
{
    public class WebSocketClient
    {
        private readonly ClientWebSocket _clientWebSocket;
        public WebSocketClient()
        {
            _clientWebSocket = new ClientWebSocket();
        }

        // Method to connect to the WebSocket server
        public async Task ConnectAsync()
        {
            string apiKey = "OytKNDHdJokVRhcx";
            string wssUri = $"wss://rpc.shyft.to?api_key={apiKey}";

            Uri serverUri = new Uri(wssUri);
            try
            {
                await _clientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
                Console.WriteLine("WebSocket connected successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while connecting: {ex.Message}");
                throw;  // Re-throwing for external handling
            }
        }

        // Method to send a message through the WebSocket
        public async Task SendAsync(string dexAddress)
        {
            var websocketRequest = new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "logsSubscribe",
                @params = new object[]
                {
                    new
                    {
                        mentions = new[] { dexAddress }
                    },
                    new
                    {
                        commitment = "finalized"
                    }
                }
            };

            string requestJson = JsonConvert.SerializeObject(websocketRequest);
            byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);

            try
            {
                if (_clientWebSocket.State == WebSocketState.Open)
                {
                    await _clientWebSocket.SendAsync(new ArraySegment<byte>(requestBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    Console.WriteLine("Message sent successfully.");
                }
                else
                {
                    Console.WriteLine("WebSocket is not connected.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while sending message: {ex.Message}");
                throw;  // Re-throwing for external handling
            }
        }

        // Method to receive messages from the WebSocket
        public async Task<string> ReceiveMessageAsync()
        {
            var buffer = new byte[1024];
            var resultBuffer = new List<byte>();

            try
            {
                WebSocketReceiveResult result;
                do
                {
                    result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    // Add the received data to the result buffer
                    resultBuffer.AddRange(buffer.Take(result.Count));

                } while (!result.EndOfMessage);  // Continue receiving until the entire message is received

                // Convert the accumulated byte array to a string
                string responseJson = Encoding.UTF8.GetString(resultBuffer.ToArray());

                // Parse the JSON string
                var jsonObject = JObject.Parse(responseJson);
                string signature = (string)jsonObject["params"]?["result"]?["value"]?["signature"];

                // Return the signature if it's found, otherwise return an empty string
                return signature ?? "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while receiving message: {ex.Message}");
                throw;  // Re-throw for external handling
            }
        }


        // Method to close the WebSocket connection
        public async Task CloseAsync()
        {
            try
            {
                if (_clientWebSocket.State == WebSocketState.Open)
                {
                    await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                    Console.WriteLine("WebSocket closed successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while closing WebSocket: {ex.Message}");
                throw;  // Re-throwing for external handling
            }
        }
    }
}
