// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Text.Json;

namespace YetAnotherBlogGenerator.Cache;

internal static class CacheServiceExtensions {
  extension(ICacheService cacheService) {
    public T? Get<T>(string source, string key) {
      var json = cacheService.Get(source, key);
      return json == null ? default : JsonSerializer.Deserialize<T>(json);
    }

    public void Set<T>(string source, string key, T value) {
      var json = JsonSerializer.Serialize(value);
      cacheService.Set(source, key, json);
    }
  }
}
