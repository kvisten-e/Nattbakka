namespace nattbakka_server.Helpers
{
    public class GetTransactionSolDecimals
    {
        public string GetTransactionSolDecimal(double sol)
        {
            string solString = sol.ToString();
            char separator = solString.Contains('.') ? '.' : ',';
            
            int firstDecimalIndex = solString.IndexOf(separator);

            if (firstDecimalIndex < 0) return "0";
            
            string decimals = solString.Split(separator)[1];
            decimals = (decimals.Length > 3) ? decimals[..3] : decimals;
            return decimals;
        }

    }
}
