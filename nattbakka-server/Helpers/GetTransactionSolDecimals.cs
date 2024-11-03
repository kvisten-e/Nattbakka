namespace nattbakka_server.Helpers
{
    public class GetTransactionSolDecimals
    {
        public string GetTransactionSolDecimal(double sol)
        {
            Console.WriteLine($"Sol: {sol}");
            int decimalsMax = 3;
            int firstDecimalIndex = sol.ToString().IndexOf(".");
            Console.WriteLine($"firstDecimalIndex: {firstDecimalIndex}");

            if (firstDecimalIndex < 0)
            {
                Console.WriteLine("Index är 0, retunerar 0");
                return "0";
            }
            
            string solToString = sol.ToString();
            Console.WriteLine($"solToString: {solToString}");

            string decimals = solToString.Split('.')[1];
            Console.WriteLine($"decimals: {decimals}");
            decimals = (decimals.Length > 3) ? decimals[..decimalsMax] : decimals;
            Console.WriteLine($"decimals2: {decimals}");
            return decimals;
        }

    }
}
