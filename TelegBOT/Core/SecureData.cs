using System;
using System.Collections.Generic;
using System.Text;

namespace TelegBOT.Core
{
    public static class SecureData
    {
        private static string token;

        public static string GetToken()
        {
            if (token == null)
            {
                token = "5000234622:AAEu27qEh0vrFCsK4iiihUt_90yU9Sg7n-Y";
            }

            return token;
        }
    }
}
