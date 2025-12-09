using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetBuddy.Domain;

public static class Extensions
{
    public static string ToMoney(this decimal amount, string currency)
        => string.Format(CultureInfo.InvariantCulture, "{0:N2} {1}", amount, currency);

    public static string MonthKey(this DateTime dt)
        => dt.ToString("yyyy-MM");

    public static bool TryDec(this string input, out decimal value)
        => decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out value);

    public static bool TryDate(this string input, out DateTime value)
        => DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out value);

    public static decimal SumAbs(this IEnumerable<decimal> seq)
        => seq.Select(Math.Abs).Sum();

    public static decimal AverageAbs(this IEnumerable<decimal> seq)
        => seq.Select(Math.Abs).DefaultIfEmpty(0).Average();
}
