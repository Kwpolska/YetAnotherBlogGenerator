// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Buffers.Text;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Utilities;

namespace YetAnotherBlogGenerator.StaticFiles;

internal class CacheBustingService(IConfiguration configuration, IUrlHelper urlHelper) : ICacheBustingService {
  private readonly ConcurrentDictionary<string, string> _urlsCache = new();

  public string Get(string url) => _urlsCache.GetOrAdd(url, ComputeUrl);

  public async Task PreCacheAssetUrls()
    => await Task.WhenAll(configuration.AssetUrls.Select(ComputeAndSaveUrlAsync)).ConfigureAwait(false);

  private string ComputeUrl(string url) {
    var filePath = urlHelper.UrlToOutputPath(url);
    using var stream = File.OpenRead(filePath);
    using var sha1 = SHA1.Create();
    var hashBytes = sha1.ComputeHash(stream);
    return GetUrl(url, hashBytes);
  }

  private async Task ComputeAndSaveUrlAsync(string url) {
    var filePath = urlHelper.UrlToOutputPath(url);
    using var stream = File.OpenRead(filePath);
    using var sha1 = SHA1.Create();
    var hashBytes = await sha1.ComputeHashAsync(stream).ConfigureAwait(false);
    var result = GetUrl(url, hashBytes);
    _urlsCache.TryAdd(url, result);
  }

  private static string GetUrl(string url, ReadOnlySpan<byte> hashBytes)
    => $"{url}?v={Base64Url.EncodeToString(hashBytes)}";
}
