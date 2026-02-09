// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Grouping;

internal class GalleryIndexGrouper(IGroupFormatter groupFormatter) : IItemGrouper {
  public IEnumerable<IGroup> GroupItems(Item[] items) {
    var galleries = items
        .Where(i => i.Type == ItemType.Gallery)
        .OrderBy(p => p.Title);

    yield return groupFormatter.FormatHtmlListGroup(
        items: galleries,
        title: "Galleries",
        url: "/galleries/",
        template: CommonTemplates.ItemList);
  }
}
