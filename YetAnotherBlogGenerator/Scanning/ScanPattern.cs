// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Scanning;

public class ScanPattern {
  public required string StartDirectory { get; init; }
  public required string FileNamePattern { get; init; }
  public required ItemType ItemType { get; init; }
  public required string RendererName { get; init; }
  public required string TemplateName { get; init; }
  public required string TargetDirectory { get; init; }
  public bool SupportsTablesOfContents { get; init; } = true;
  public bool SupportsTeasers { get; init; } = true;
  public bool UsePrettyUrls { get; init; } = true;
  public bool IncludeInSitemap { get; init; } = true;
}
