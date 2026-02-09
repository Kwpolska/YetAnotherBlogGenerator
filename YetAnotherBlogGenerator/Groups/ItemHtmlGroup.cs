// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Groups;

public record ItemHtmlGroup(
    Item[] Items,
    string Title,
    string Url,
    string TemplateName,
    string? Key = null,
    string? PreviousGroupUrl = null,
    string? NextGroupUrl = null) : IHtmlGroup {
  public DateTimeOffset LastUpdated => Items.Max(i => i.Updated ?? i.Published);
}
