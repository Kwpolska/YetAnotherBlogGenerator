// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;
using YetAnotherBlogGenerator.Utilities;

namespace YetAnotherBlogGenerator.Grouping;

internal class GroupFormatter(IConfiguration configuration) : IGroupFormatter {
  public ItemGroup FormatHtmlListGroup(IEnumerable<Item> items, string title, string url, string template,
      string? key = null)
    => FormatHtmlListGroup(items.ToArray(), title: title, url: url, template: template, key: key);

  public ItemGroup FormatHtmlListGroup(Item[] items, string title, string url, string template,
      string? key = null)
    => new(Items: items, Title: title, Url: url, TemplateName: template,
        Key: key);

  public RssFeed FormatRssFeed(IEnumerable<Item> items, string title, string url, string? key = null)
    => new(Items: items.ToArray(), Title: title, Url: url, Key: key);

  public IEnumerable<ItemGroup> FormatHtmlIndexGroups(
      IEnumerable<Item> items,
      string title,
      string outputFolderPath,
      string template, string? key = null, string? rssUrl = null) {
    var sortedItems = items.OrderByDescending(i => i.Published).ToArray();
    var groupCount = sortedItems.Length / configuration.IndexSize +
                     Math.Sign(sortedItems.Length % configuration.IndexSize > 0 ? 1 : 0);
    outputFolderPath = outputFolderPath.TrimEnd('/');
    for (int groupNumber = 0; groupNumber < groupCount; groupNumber++) {
      // ranges are start-inclusive, end-exclusive
      var rangeStart = groupNumber * configuration.IndexSize;
      var rangeEnd = Math.Min((groupNumber + 1) * configuration.IndexSize, sortedItems.Length);
      var itemsInGroup = sortedItems[rangeStart..rangeEnd];

      var thisGroupOutputUrl =
          groupNumber == 0 ? $"{outputFolderPath}/" : $"{outputFolderPath}/index-{groupNumber}.html";
      var groupTitle = groupNumber == 0 ? title : $"{title} (old posts, page {groupNumber})";
      var previousGroupUrl = groupNumber switch {
          0 => null,
          1 => $"{outputFolderPath}/",
          _ => $"{outputFolderPath}/index-{groupNumber - 1}.html"
      };
      var nextGroupUrl = groupNumber == (groupCount - 1) ? null : $"{outputFolderPath}/index-{groupNumber + 1}.html";

      yield return new ItemGroup(
          Items: itemsInGroup,
          Title: groupTitle,
          Url: thisGroupOutputUrl,
          TemplateName: template,
          Key: key ?? groupNumber.ToString(),
          PreviousGroupUrl: previousGroupUrl,
          NextGroupUrl: nextGroupUrl);
    }
  }

  public IEnumerable<DirectoryTreeGroup> BuildDirectoryTreeGroups(
      IEnumerable<Item> items,
      string template,
      string titlePrefix = "",
      string titleSuffix = "") {
    return items
        .GroupBy(i => i.ScanPattern.TargetDirectory)
        .SelectMany(itemsInTargetDirectory =>
            BuildDirectoryTreeGroup(itemsInTargetDirectory, template, titlePrefix, titleSuffix));
  }

  private static IEnumerable<DirectoryTreeGroup> BuildDirectoryTreeGroup(
      IEnumerable<Item> items,
      string template,
      string titlePrefix,
      string titleSuffix) {
    var entries = new HashSet<DirectoryTreeEntry>();
    string? basePath = null;
    var lastUpdated = DateTimeOffset.MinValue;

    foreach (var item in items) {
      basePath ??= $"/{item.ScanPattern.TargetDirectory}";
      entries.Add(new DirectoryTreeItem(item));
      for (var i = 0; i < item.SourcePathElements.Length; i++) {
        entries.Add(new DirectoryTreeDirectory(item.SourcePathElements.Take(i).ToArray(), basePath));
      }

      var d = item.Updated ?? item.Published;
      if (d > lastUpdated) lastUpdated = d;
    }

    if (basePath == null) {
      return [];
    }

    return entries
        .Where(e => e.JoinedPath != "")
        .GroupBy(e => {
          var url = e.Url.TrimEnd('/');
          var index = url.LastIndexOf('/');
          return (index == -1 ? url : e.Url[..index]) + "/";
        })
        .Select(g => new DirectoryTreeGroup(
            Entries: g.OrderBy(e => e.IsItem ? 1 : 0).ThenBy(e => e.Url).ToArray(),
            Title: titlePrefix + g.Key.TrimStart('/') + titleSuffix,
            Url: g.Key,
            Breadcrumbs: BreadcrumbsHelper.BuildBreadcrumbs(g.Key),
            TemplateName: template,
            LastUpdated: lastUpdated));
  }
}
