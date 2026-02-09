// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Groups;

public class NavigationGroup(NavigationGroupItem[] items) : IGroup {
  private readonly Dictionary<string, NavigationGroupItem> _lookup
      = items.ToDictionary(i => i.Item.SourcePath, i => i);

  public NavigationGroupItem? ForItem(Item item) => _lookup.GetValueOrDefault(item.SourcePath);
}
