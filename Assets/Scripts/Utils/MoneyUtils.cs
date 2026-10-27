using UnityEngine;

namespace Game.Utils
{
    public static class MoneyUtils
    {
        /// <summary>
        /// Formats a money value with leading zeros to a minimum number of digits.
        /// Useful for UI displays.
        /// </summary>
        /// <param name="money">The money value to format.</param>
        /// <param name="minimumDigits">The minimum number of digits to display.</param>
        /// <returns>The formatted money value.</returns>
        public static string FormatMoney(int money, int displayedDigits)
        {
            return money.ToString($"D{displayedDigits}");
        }
    }
}
