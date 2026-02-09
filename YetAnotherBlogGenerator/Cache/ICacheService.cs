// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

namespace YetAnotherBlogGenerator.Cache;

internal interface ICacheService {
  string? Get(string source, string key);
  void Set(string source, string key, string? value);
}
