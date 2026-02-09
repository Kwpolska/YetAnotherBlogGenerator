// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

namespace YetAnotherBlogGenerator.Config;

public class TagsAndCategoriesConfiguration {
  public Dictionary<string, string> CategoryColors { get; init; } = new();
  public Dictionary<string, string> CustomSlugs { get; init; } = new();
}
