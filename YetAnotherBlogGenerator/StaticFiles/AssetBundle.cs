// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

namespace YetAnotherBlogGenerator.StaticFiles;

public class AssetBundle {
  public required string OutputUrl { get; init; }
  public required string BaseSourceDirectory { get; init; }
  public required string[] Files { get; init; }
}
