// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

namespace YetAnotherBlogGenerator.Utilities;

internal class FallbackDictionary<TKey, TValue>(
    Dictionary<TKey, TValue> baseDictionary,
    Dictionary<TKey, TValue> fallbackDictionary)
    where TKey : notnull {
  public TValue this[TKey key] => GetValue(key);

  public TValue GetValue(TKey key) {
    var foundInBase = baseDictionary.TryGetValue(key, out var value);
    return foundInBase ? value! : fallbackDictionary[key];
  }

  public TValue? GetValueOrDefault(TKey key) {
    var foundInBase = baseDictionary.TryGetValue(key, out var value);
    return foundInBase ? value! : fallbackDictionary.GetValueOrDefault(key);
  }

  public TValue GetValueOrDefault(TKey key, TValue defaultValue) {
    var foundInBase = baseDictionary.TryGetValue(key, out var value);
    return foundInBase ? value! : fallbackDictionary.GetValueOrDefault(key, defaultValue);
  }

  internal string? GetString(TKey key) {
    var rawValue = GetValueOrDefault(key);
    return rawValue == null ? null : rawValue.ToString()!;
  }
}
