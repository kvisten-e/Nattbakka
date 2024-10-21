namespace nattbakka_server.Helpers
{
    public class GetTransactionSolDecimals
    {
        public string GetTransactionSolDecimal(double sol)
        {
            int decimalsMax = 3;
            int firstDecimalIndex = sol.ToString().IndexOf(",");

            if (firstDecimalIndex < 0)
            {
                return "0";
            }
            string solToString = sol.ToString();
            string decimals = solToString.Split(',')[1];
            decimals = (decimals.Length > 3) ? decimals[..decimalsMax] : decimals;
            return decimals;
        }

    }
}
