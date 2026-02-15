// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Meta;

internal interface IMetaExtractor {
  int Priority { get; }
  string Name { get; }
  bool SupportsItemType(ItemType itemType);

  IReadOnlyDictionary<string, object>? ExtractMeta(string itemFullSource, string itemPath, ItemType itemType);
  string ExtractContentSource(string itemFullSource, string itemPath);
}
