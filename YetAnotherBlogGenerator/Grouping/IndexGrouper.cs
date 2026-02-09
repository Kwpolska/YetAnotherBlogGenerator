// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Grouping;

internal class IndexGrouper(IConfiguration configuration, IGroupFormatter groupFormatter) : IPostGrouper {
  public IEnumerable<IGroup> GroupPosts(Item[] posts) {
    var htmlGroups = groupFormatter.FormatHtmlIndexGroups(
        items: posts,
        title: "Blog",
        url: "/",
        template: CommonTemplates.Index);

    var rssGroups = groupFormatter.FormatRssGroup(
        items: posts.Take(configuration.FeedSize),
        title: configuration.SiteTitle,
        url: "/rss.xml");

    return htmlGroups.Cast<IGroup>().Append(rssGroups);
  }
}
