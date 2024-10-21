using nattbakka_server.Models;
using Solnet.Rpc.Types;

namespace nattbakka_server.Helpers
{
    public class CexTransactionTemplate
    {

        public ParsedTransaction ParsedTransaction(dynamic transactionDetails)
        {
            double sol = 0;
            string receivingAddress = "";
            string sendingAddress = transactionDetails.Result.Transaction.Message.AccountKeys[0];

            switch (sendingAddress)
            {
                case "5tzFkiKscXHK5ZXCGbXZxdw7gTjjD1mBwuoFbhUvuAi9":
                    sol = (double)(transactionDetails.Result.Meta.PreBalances[0] - transactionDetails.Result.Meta.PostBalances[0]) / 1_000_000_000;
                    receivingAddress = transactionDetails.Result.Transaction.Message.AccountKeys[2];
                    break;
                case "u6PJ8DtQuPFnfmwHbGFULQ4u4EgjDiyYKjVEsynXq2w":

                    break;
                case "2AQdpHJ2JpcEgPiATUXjQxA8QmafFegfQwSLWSprPicm":

                    break;
                case "FWznbcNXWQuHTawe9RxvQ2LdCENssh12dsznf4RiouN5":

                    break;
                case "ASTyfSima4LLAdDgoFGkgqoKowG1LZFDr9fAQrg7iaJZ":

                    break;
                case "BmFdpraQhkiDQE6SnfG5omcA1VwzqfXrwtNYBwWTymy6":

                    break;
                case "AC5RDfQFmDS1deWZos921JfqscXdByf8BKHs5ACWjtW2":
                    sol = (double)transactionDetails.Result.Meta.PostBalances[2] - (transactionDetails.Result.Meta.PreBalances[2]) / 1_000_000_000;
                    receivingAddress = transactionDetails.Result.Transaction.Message.AccountKeys[2];
                    break;
                case "5VCwKtCXgCJ6kit5FybXjvriW3xELsFDhYrPSqtJNmcD":

                    break;
                case "AobVSwdW9BbpMdJvTqeCN4hPAmh4rHm7vwLnQ5ATSyrS":

                    break;
                case "A77HErqtfN1hLLpvZ9pCtu66FEtM8BveoaKbbMoZ4RiR":

                    break;
                case "G2YxRa6wt1qePMwfJzdXZG62ej4qaTC7YURzuh2Lwd3t":

                    break;
                case "5ndLnEYqSFiA5yUFHo6LVZ1eWc6Rhh11K5CfJNkoHEPs":

                    break;
                default:
                    sol = (double)(transactionDetails.Result.Meta.PreBalances[0] - transactionDetails.Result.Meta.PostBalances[0]) / 1_000_000_000;
                    receivingAddress = transactionDetails.Result.Transaction.Message.AccountKeys[1];
                    break;
            }


            return new ParsedTransaction
            {
                receivingAddress = receivingAddress,
                sendingAddress = sendingAddress,
                sol = sol
            };
        }


    }
}
