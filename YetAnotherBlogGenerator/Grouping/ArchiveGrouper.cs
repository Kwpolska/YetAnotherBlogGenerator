// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Grouping;

internal class ArchiveGrouper(IGroupFormatter groupFormatter) : IPostGrouper {
  public IEnumerable<IGroup> GroupPosts(Item[] posts) {
    var groups = posts
        .GroupBy(item => item.Meta.Published.Year)
        .Select(group => groupFormatter.FormatHtmlListGroup(
            items: group,
            title: $"Posts for the year {group.Key}",
            url: $"/blog/{group.Key}/",
            template: CommonTemplates.ItemList,
            key: group.Key.ToString()))
        .ToList();

    var linkGroupItems = groups
        .Select(g => new LinkGroupItem(
            SubGroup: null,
            Title: g.Key!,
            Url: $"/blog/{g.Key}/",
            Count: g.Items.Length))
        .OrderByDescending(g => g.Title)
        .ToArray();

    var linkGroup = new LinkGroup(
        Links: linkGroupItems,
        Url: "/blog/",
        TemplateName: CommonTemplates.LinkList,
        Title: "Archives",
        LastUpdated: posts.Max(p => p.Published));

    return groups.Cast<IGroup>().Append(linkGroup);
  }
}
