using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventSubWebsocket.Types
{
    public class CharityNotfication
    {
        /// <summary>
        /// The name of the charity.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Information on the amount donated.
        /// </summary>
        public Amount Amount { get; }

        public CharityNotfication(string name, Amount amount)
        {
            Name = name;
            Amount = amount;
        }
    }

    public class Amount
    {
        /// <summary>
        /// The amount of money donated. This is without decimal information, so 5.00 USD would be shown as 500.
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// The amount of decimal places the currency uses. So for USD that has 2 decimal places, the value would be 2. A currency without decimal places would have a value of 0.
        /// </summary>
        public int DecimalPlaces { get; }

        /// <summary>
        /// The ISO-4217 of the currency donated.
        /// </summary>
        public string CurrencyCode { get; }

        public Amount(int value, int decimalPlaces, string currencyCode)
        {
            Value = value;
            DecimalPlaces = decimalPlaces;
            CurrencyCode = currencyCode;
        }
    }
}
