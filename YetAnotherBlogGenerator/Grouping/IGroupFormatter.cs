// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Grouping;

internal interface IGroupFormatter {
  ItemGroup FormatHtmlListGroup(IEnumerable<Item> items, string title, string url, string template,
      string? key = null);

  ItemGroup FormatHtmlListGroup(Item[] items, string title, string url, string template, string? key = null);

  RssFeed FormatRssFeed(IEnumerable<Item> items, string title, string url, string? key = null);

  IEnumerable<ItemGroup> FormatHtmlIndexGroups(IEnumerable<Item> items, string title, string url, string template,
      string? key = null, string? rssUrl = null);

  IEnumerable<DirectoryTreeGroup> BuildDirectoryTreeGroups(IEnumerable<Item> items, string template,
      string titlePrefix = "", string titleSuffix = "");
}
