// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using Microsoft.Extensions.Logging;
using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Utilities;

namespace YetAnotherBlogGenerator.StaticFiles;

public class AssetBundleEngine(IConfiguration configuration, ILogger<AssetBundleEngine> logger, IUrlHelper urlHelper)
    : IAssetBundleEngine {
  public async Task<int> BundleAssets() {
    var tasks = configuration.AssetBundles.Select(BundleAssetsCore);
    await Task.WhenAll(tasks).ConfigureAwait(false);
    return configuration.AssetBundles.Length;
  }

  private async Task BundleAssetsCore(AssetBundle bundle) {
    var outputPath = urlHelper.UrlToOutputPath(bundle.OutputUrl);
    logger.LogDebug(Constants.BundleLog, "{Destination}", outputPath);
    var outputStream = new FileStream(outputPath, FileMode.Create);
    await using var _ = outputStream.ConfigureAwait(false);
    foreach (var inputFile in bundle.Files) {
      var inputPath = Path.Combine(configuration.SourceRoot, bundle.BaseSourceDirectory, inputFile);
      var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
      await using (inputStream.ConfigureAwait(false)) {
        await inputStream.CopyToAsync(outputStream).ConfigureAwait(false);
      }
    }
  }
}
