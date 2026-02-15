// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Grouping;

internal class GroupEngine(IEnumerable<IItemGrouper> itemGroupers, IEnumerable<IPostGrouper> postGroupers)
    : IGroupEngine {
  public IGroup[] GenerateGroups(Item[] items) {
    var sortedItems = items
        .OrderByDescending(i => i.Published)
        .ThenBy(i => i.SourcePath)
        .ToArray();

    var sortedPosts = sortedItems
        .Where(item => item.Type == ItemType.Post)
        .ToArray();

    var itemGroups = itemGroupers.SelectMany(g => g.GroupItems(sortedItems));
    var postGroups = postGroupers.SelectMany(g => g.GroupPosts(sortedPosts));
    return itemGroups.Concat(postGroups).ToArray();
  }
}
