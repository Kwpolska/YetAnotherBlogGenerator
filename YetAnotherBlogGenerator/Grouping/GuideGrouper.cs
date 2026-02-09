// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Grouping;

internal class GuideGrouper(IGroupFormatter groupFormatter) : IPostGrouper {
  public IEnumerable<IGroup> GroupPosts(Item[] posts) {
    var guides = posts
        .Where(p => p.Meta.Guide != null)
        .OrderByDescending(p => p.Meta.UpdatedOrPublished)
        .ToArray();

    yield return groupFormatter.FormatHtmlListGroup(
        items: guides,
        title: "Guides",
        url: "/guides/",
        template: CommonTemplates.GuideIndex);
  }
}
