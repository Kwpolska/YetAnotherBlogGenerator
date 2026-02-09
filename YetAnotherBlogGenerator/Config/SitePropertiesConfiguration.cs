// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

namespace YetAnotherBlogGenerator.Config;

public class SitePropertiesConfiguration {
  public required string Title { get; init; }
  public required string Uri { get; init; }
  public required string Author { get; init; }
  public required string Description { get; init; }
  public required string Footer { get; init; }
}
