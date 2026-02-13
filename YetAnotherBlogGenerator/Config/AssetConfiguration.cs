// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.StaticFiles;

namespace YetAnotherBlogGenerator.Config;

public class AssetConfiguration {
  public AssetBundle[] Bundles { get; init; } = [];
  public string[] Urls { get; init; } = [];
}
