// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Meta;

internal partial class YamlMetaExtractor(ILogger<YamlMetaExtractor> logger) : IMetaExtractor {
  private static readonly Regex SeparatorRegex = SeparatorRegexSource();
  private readonly IDeserializer _deserializer = new DeserializerBuilder().Build();

  public int Priority => 0;
  public string Name => "YAML";

  public bool SupportsItemType(ItemType itemType) => itemType != ItemType.Listing;

  public string ExtractContentSource(string itemFullSource, string itemPath) {
    var items = SeparatorRegex.Split(itemFullSource, 3);
    return items.Last();
  }

  public IReadOnlyDictionary<string, object>? ExtractMeta(string itemFullSource, string itemPath, ItemType itemType) {
    var items = SeparatorRegex.Split(itemFullSource, 3);
    var yaml = items.Skip(1).FirstOrDefault();
    if (yaml == null) {
      logger.LogDebug("YAML metadata not found in {Path}", itemPath);
      return null;
    }

    return _deserializer.Deserialize<Dictionary<string, object>>(yaml);
  }

  [GeneratedRegex(@"^---$", RegexOptions.Multiline)]
  private static partial Regex SeparatorRegexSource();
}
