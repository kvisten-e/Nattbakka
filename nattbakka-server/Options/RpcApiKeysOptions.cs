namespace nattbakka_server.Options
{
    public class RpcApiKeysOptions
    {
        public const string ApiKeysShyft = "ApiKeysShyft";
        public const string ApiKeysHelius = "ApiKeysHelius";

        public List<string> ShyftApiKeys { get; set; }
        public List<string> HeliusApiKeys { get; set; }

    }

}
