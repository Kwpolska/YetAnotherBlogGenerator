// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using YetAnotherBlogGenerator.Cache;
using YetAnotherBlogGenerator.Utilities;

namespace YetAnotherBlogGenerator.ItemRendering.External;

internal class CachingPygmentsRenderer(
    ICacheService cacheService,
    ILogger<CachingPygmentsRenderer> logger,
    PygmentsRenderer pygmentsRenderer
) : IListingRenderer {
  private const string CacheSource = "pygments:1";

  public async Task<string> RenderSingleListing(string code, string? path, string? language) {
    var cacheKey = GetCacheKey(code, path, language);
    var cachedResult = cacheService.Get(CacheSource, cacheKey);
    if (cachedResult != null) {
      logger.LogTrace("Reusing pre-rendered listing with key='{Key}', path='{Path}'", cacheKey, path);
      return cachedResult;
    }

    var freshResult = await pygmentsRenderer.RenderSingleListing(code, path, language).ConfigureAwait(false);
    cacheService.Set(CacheSource, cacheKey, freshResult);
    return freshResult;
  }

  public async Task<List<ExternalRenderResponse>> RenderMultipleListings(IEnumerable<ExternalRenderRequest> requests) {
    var missingRequests = new List<ExternalRenderRequest>();
    var cachedResults = new List<ExternalRenderResponse>();
    var cacheKeyPerGuid = new Dictionary<Guid, string>();
    foreach (var request in requests) {
      var cacheKey = GetCacheKey(request.Source, request.Path, request.Language);
      cacheKeyPerGuid[request.Guid] = cacheKey;
      var cachedResult = cacheService.Get(CacheSource, cacheKey);
      if (cachedResult == null) {
        missingRequests.Add(request);
      } else {
        logger.LogTrace("Reusing pre-rendered listing with key='{Key}', path='{Path}'", cacheKey, request.Path);
        cachedResults.Add(new ExternalRenderResponse(request.Guid, request.Path, true, cachedResult));
      }
    }

    if (missingRequests.Count == 0) {
      return cachedResults;
    }

    var newResults = await pygmentsRenderer.RenderMultipleListings(missingRequests).ConfigureAwait(false);
    foreach (var newResult in newResults) {
      if (newResult.Success) {
        var cacheKey = cacheKeyPerGuid[newResult.Guid];
        cacheService.Set(CacheSource, cacheKey, newResult.Html);
      }
    }

    cachedResults.AddRange(newResults);
    return cachedResults;
  }

  private static string GetCacheKey(string listing, string? path, string? language) {
    var rawListing = Encoding.UTF8NoBom.GetBytes(listing);
    var hash = SHA1.HashData(rawListing);
    return $"{Convert.ToHexString(hash).ToLower()}|{path}|{language}";
  }
}
