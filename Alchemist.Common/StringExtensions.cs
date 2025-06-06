using System.Text.RegularExpressions;

namespace Alchemist.Common;

public static class StringExtensions
{
    public static string RemoveSpecialCharacters(this string input)
    {
        var r = new Regex(
                      "(?:[^а-яА-ЯёЁa-zA-Z0-9() ]|(?<=['\"])s)",
                      RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        return r.Replace(input.Trim(), string.Empty);
    }
}
