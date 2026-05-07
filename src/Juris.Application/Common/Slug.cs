using System.Text;
using System.Text.RegularExpressions;

namespace Juris.Application.Common;

public static class Slug
{
    private static readonly Regex NonSlugChars = new("[^a-z0-9-]+", RegexOptions.Compiled);
    private static readonly Regex MultipleHyphens = new("-{2,}", RegexOptions.Compiled);

    public static string From(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(capacity: normalized.Length);
        foreach (var ch in normalized)
        {
            var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (cat != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        var s = sb.ToString().ToLowerInvariant();
        s = s.Replace('&', '-').Replace(' ', '-').Replace("'", "").Replace("\"", "");
        s = NonSlugChars.Replace(s, "-");
        s = MultipleHyphens.Replace(s, "-").Trim('-');
        return s;
    }
}
