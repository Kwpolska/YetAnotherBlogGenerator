// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Meta;
using YetAnotherBlogGenerator.Scanning;

namespace YetAnotherBlogGenerator.Items;

public record Item(
    ItemType Type,
    ScanPattern ScanPattern,
    string SourcePath,
    string[] SourcePathElements,
    string Url,
    ItemMeta Meta,
    string Source,
    string Content,
    string Teaser,
    IRichItemData? RichItemData
) : IRenderable {
  public string Title => Meta.Title;
  public DateTimeOffset Published => Meta.Published;
  public DateTimeOffset? Updated => Meta.Updated;
  public string TemplateName => Meta.Template ?? ScanPattern.TemplateName;
  public string ContentTag => Meta.Legacy ? "content-legacy" : $"content-{ScanPattern.RendererName}";
}
