// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;
using YetAnotherBlogGenerator.Utilities;

namespace YetAnotherBlogGenerator.Grouping;

internal class TagCategoryGrouper(IConfiguration configuration, IGroupFormatter groupFormatter) : IPostGrouper {
  private const string UrlRoot = "/blog/tags/";
  private const string HtmlUrlTemplate = UrlRoot + "{0}/";
  private const string RssUrlTemplate = UrlRoot + "{0}.xml";

  public IEnumerable<IGroup> GroupPosts(Item[] posts) {
    var postsByCategory = posts
        .Where(p => p.Meta.Category != null)
        .GroupBy(p => {
          var category = p.Meta.Category!;
          return new TagKey(category, "cat_" + category.Slugify(configuration.TagsAndCategoriesCustomSlugs), true);
        });

    var postsByTag = posts
        .SelectMany(p => p.Meta.Tags.Select(t => {
          var key = new TagKey(t, t.Slugify(configuration.TagsAndCategoriesCustomSlugs), false);
          return new KeyValuePair<TagKey, Item>(key, p);
        }))
        .GroupBy(keySelector: tp => tp.Key, elementSelector: tp => tp.Value);

    var postsByAnything = postsByCategory.Concat(postsByTag).ToArray();

    // Verify slugs are unique
    var allSlugs = postsByAnything.Select(g => g.Key.Slug);
    var duplicateSlugs = allSlugs.GroupBy(s => s).Where(g => g.Count() > 1).Select(g => $"'{g.Key}'").ToArray();
    if (duplicateSlugs.Length > 0) {
      throw new Exception("Some tags or categories have duplicate slugs: " + string.Join(", ", duplicateSlugs));
    }

    var htmlGroups = postsByAnything
        .SelectMany(g => groupFormatter.FormatHtmlIndexGroups(
            items: g,
            title: $"Posts about {g.Key.Name}",
            url: g.Key.HtmlUrl,
            template: CommonTemplates.TagIndex,
            key: g.Key.Slug,
            rssUrl: g.Key.RssUrl));

    var rssFeeds = postsByAnything
        .Select(g => groupFormatter.FormatRssFeed(
            items: g.Take(configuration.FeedSize),
            title: $"Posts about {g.Key.Name}",
            url: g.Key.RssUrl,
            key: g.Key.Slug));

    var linkGroupItems = postsByAnything
        .OrderBy(g => g.Key.Name)
        .Select(g => new LinkGroupItem(g.Key.IsCategory ? "category" : "tag", g.Key.Name, g.Key.HtmlUrl, g.Count()))
        .ToArray();

    var linkGroup = new LinkGroup(
        Links: linkGroupItems,
        Url: UrlRoot,
        TemplateName: CommonTemplates.TagList,
        Title: "Tags and Categories",
        LastUpdated: posts.Max(p => p.Published));

    return htmlGroups.Cast<IGroup>().Concat(rssFeeds).Append(linkGroup);
  }

  private record struct TagKey(string Name, string Slug, bool IsCategory) {
    public readonly string HtmlUrl = string.Format(HtmlUrlTemplate, Slug);
    public readonly string RssUrl = string.Format(RssUrlTemplate, Slug);
  }
}
