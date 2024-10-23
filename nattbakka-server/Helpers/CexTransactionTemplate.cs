using nattbakka_server.Models;
using Solnet.Rpc.Types;

namespace nattbakka_server.Helpers
{
    public class CexTransactionTemplate
    {
        private int _AccountKeysLength;
        private int _PostBalancesLength;
        private double _solReceiver = 0;
        private string? _receivingAddress = null;
        private string? _sendingAddress = null;
        private string _cexAddress = "";
        private dynamic _transactionDetails;

        public ParsedTransaction ParsedTransaction(dynamic transactionDetails, Cex cex)
        {
            _transactionDetails = transactionDetails;
            _AccountKeysLength = transactionDetails.Result.Transaction.Message.AccountKeys.Length;
            _PostBalancesLength = transactionDetails.Result.Meta.PostBalances.Length;
            _cexAddress = cex.address;

            int indexSender = FindSenderAddress();

            if(indexSender >= 0)
            {
                int indexReceiving = FindReceivingAddress(indexSender);
                
                if(indexSender > indexReceiving)
                {
                    return null;
                }
            }

            return new ParsedTransaction
            {
                receivingAddress = _receivingAddress,
                sendingAddress = _sendingAddress,
                sol = _solReceiver
            };
        }

        private int FindSenderAddress()
        {
            for (int i = 0; i < _AccountKeysLength; i++)
            {
                string address = _transactionDetails.Result.Transaction.Message.AccountKeys[i];
                if (address == _cexAddress)
                {
                    _sendingAddress = address;
                    return i;
                }
            }
            return -1;
        }


        private int FindReceivingAddress(int index)
        {
            for (int i = index + 1; i < _PostBalancesLength; i++)
            {
                double potentialReceivingSol = ((double)_transactionDetails.Result.Meta.PostBalances[i] - _transactionDetails.Result.Meta.PreBalances[i]) / 1_000_000_000;


                if (potentialReceivingSol >= 0.05)
                {
                    _receivingAddress = _transactionDetails.Result.Transaction.Message.AccountKeys[i];
                    _solReceiver = potentialReceivingSol;
                    return i;
                }
            }
            return -1;
        }
    }
}
