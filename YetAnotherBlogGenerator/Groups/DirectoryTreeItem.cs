// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Groups;

public class DirectoryTreeItem(Item item) : DirectoryTreeEntry(item.SourcePathElements, isItem: true) {
  public Item Item { get; } = item;
  public override string Url => Item.Url;
}
