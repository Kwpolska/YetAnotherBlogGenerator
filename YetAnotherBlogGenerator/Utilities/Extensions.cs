// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Text;
using System.Text.RegularExpressions;

namespace YetAnotherBlogGenerator.Utilities;

internal static partial class Extensions {
  private static readonly Regex SlugifyInvalidCharacterRegex = SlugifyInvalidCharacterRegexSource();
  private static readonly Regex SlugifySpaceCharacterRegex = SlugifySpaceCharacterRegexSource();

  public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : notnull
    => enumerable.Where(x => x != null).Cast<T>();

  /// <summary>
  /// Convert to ASCII if 'allowUnicode' is false. Convert spaces or repeated
  /// dashes to single dashes. Remove characters that aren't alphanumerics,
  /// underscores, or hyphens. Convert to lowercase. Also strip leading and
  /// trailing whitespace, dashes, and underscores.
  /// </summary>
  /// <remarks>
  /// Converted from Python implementation in Django (BSD-3-Clause license)
  /// https://github.com/django/django/blob/c72001644fa794b82fa88a7d2ecc20197b01b6f2/django/utils/text.py#L436
  /// </remarks>
  public static string Slugify(this string value, Dictionary<string, string>? specialSlugs = null) {
    if (specialSlugs?.TryGetValue(value, out var specialValue) == true) {
      return specialValue;
    }

    value = new string(value.Normalize(NormalizationForm.FormKD).Where(c => c <= 0x7f).ToArray())
        .ToLowerInvariant();

    value = SlugifyInvalidCharacterRegex.Replace(value, "");
    return SlugifySpaceCharacterRegex.Replace(value, "-").Trim('-', '_');
  }

  private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

  extension(Encoding) {
    // 2001 truly was a different time, and I feel weird adding extensions to a static class.
    public static Encoding UTF8NoBom => Utf8NoBom;
  }

  [GeneratedRegex(@"[^\w\s-]")]
  private static partial Regex SlugifyInvalidCharacterRegexSource();

  [GeneratedRegex(@"[-\s]+")]
  private static partial Regex SlugifySpaceCharacterRegexSource();
}
