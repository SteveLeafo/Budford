namespace Budford.Utilities
{
    internal static class Nintendo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        internal static string GetRegion(string region)
        {
            switch (region)
            {
                case "00000001":
                    return "JPN";
                case "00000002":
                    return "USA";
                case "00000004":
                    return "EUR";
                case "FFFFFFFF":
                    return "ALL";
            }
            return "??";
        }
    }
}
