// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Scanning;
using YetAnotherBlogGenerator.StaticFiles;

namespace YetAnotherBlogGenerator.Config;

public class SiteConfiguration {
  public required SitePropertiesConfiguration Site { get; init; }
  public required GroupingConfiguration Grouping { get; init; }
  public MenuItem[] Menu { get; init; } = [];
  public ScanPattern[] ScanPatterns { get; init; } = [];
  public AssetBundle[] AssetBundles { get; init; } = [];
  public required TagsAndCategoriesConfiguration TagsAndCategories { get; init; }
}
