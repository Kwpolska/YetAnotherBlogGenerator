// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Meta;

internal class FileSystemMetaExtractor : IMetaExtractor {
  public int Priority => 100;
  public string Name => "FileSystem";

  public bool SupportsItemType(ItemType itemType) => true;

  public string ExtractContentSource(string itemFullSource, string itemPath)
    => itemFullSource;

  public Dictionary<string, object> ExtractMeta(string itemFullSource, string itemPath, ItemType itemType) => new() {
      { MetaFields.Title, itemType == ItemType.Listing ? Path.GetFileName(itemPath) : Path.GetFileNameWithoutExtension(itemPath) },
      { MetaFields.Published, new DateTimeOffset(File.GetCreationTimeUtc(itemPath), TimeSpan.Zero) }
  };
}
