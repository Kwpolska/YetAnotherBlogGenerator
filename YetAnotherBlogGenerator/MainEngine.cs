// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using YetAnotherBlogGenerator.Cache;
using YetAnotherBlogGenerator.Grouping;
using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.ItemRendering;
using YetAnotherBlogGenerator.Output;
using YetAnotherBlogGenerator.Scanning;
using YetAnotherBlogGenerator.StaticFiles;
using YetAnotherBlogGenerator.TemplateRendering;
using YetAnotherBlogGenerator.XmlGenerators;

namespace YetAnotherBlogGenerator;

internal class MainEngine(
    IAssetBundleEngine assetBundleEngine,
    ICacheBustingService cacheBustingService,
    ICacheService cacheService,
    IGroupEngine groupEngine,
    IItemRenderEngine itemRenderEngine,
    IEnumerable<IItemScanner> itemScanners,
    ILogger<MainEngine> logger,
    IOutputEngine outputEngine,
    IRenderDispatcher renderDispatcher,
    IRssGenerator rssGenerator,
    ISitemapGenerator sitemapGenerator,
    IStaticFileEngine staticFileEngine,
    TimeProvider timeProvider,
    IThumbnailEngine thumbnailEngine) {
  private readonly Stopwatch _actionStopwatch = new();
  private IDisposable? _actionScope;

  public async Task Run() {
    await Task.Yield();
    var startTime = timeProvider.GetUtcNow();
    logger.LogInformation(Constants.CoreLog, "Starting build");

    try {
      await RunCore().ConfigureAwait(false);
      cacheService.Set(Constants.CoreCacheSource, nameof(LastRenderStatus),
          new LastRenderStatus(startTime, true));
    } catch {
      cacheService.Set(Constants.CoreCacheSource, nameof(LastRenderStatus),
          new LastRenderStatus(startTime, false));
      throw;
    }
  }

  private async Task RunCore() {
    var topStopwatch = Stopwatch.StartNew();

    StartAction("scan", "Scanning for items");
    var sourceItems =
        (await Task.WhenAll(itemScanners.Select(scanner => scanner.ScanForItems())).ConfigureAwait(false))
        .SelectMany(i => i).ToArray();
    FinishAction("Found", sourceItems.Length, "items");

    StartAction("render", "Rendering items to HTML");
    var items = (await itemRenderEngine.Render(sourceItems).ConfigureAwait(false)).ToArray();
    FinishAction("Rendered", items.Length, "items");

    StartAction("group", "Grouping items");
    var groups = groupEngine.GenerateGroups(items);
    FinishAction("Generated", groups.Length, "groups");

    StartAction("static", "Copying static files");
    var copyTasks = staticFileEngine.CopyAllFiles().Concat(staticFileEngine.CopyItemFiles(items)).ToArray();
    await outputEngine.ExecuteMany(copyTasks).ConfigureAwait(false);
    FinishAction("Copied", copyTasks.Length, "files");

    StartAction("assets", "Processing assets");
    var assetBundleCount = await assetBundleEngine.BundleAssets().ConfigureAwait(false);
    await cacheBustingService.PreCacheAssetUrls().ConfigureAwait(false); // trivial, no separate log message needed
    FinishAction("Generated", assetBundleCount, "asset bundles");

    StartAction("thumbnails", "Generating thumbnails");
    var thumbnailTasks = thumbnailEngine.GenerateThumbnailsForImagesFolder()
        .Concat(thumbnailEngine.GenerateThumbnailsForItems(items)).ToArray();
    await outputEngine.ExecuteMany(thumbnailTasks).ConfigureAwait(false);
    FinishAction("Generated", thumbnailTasks.Length, "thumbnails");

    var htmlGroups = new List<IHtmlGroup>(groups.Length);
    var rssFeeds = new List<RssFeed>(groups.Length);
    NavigationGroup? foundNavigationGroup = null;

    foreach (var group in groups) {
      switch (group) {
        case RssFeed rg:
          rssFeeds.Add(rg);
          break;
        case IHtmlGroup hg:
          htmlGroups.Add(hg);
          break;
        case NavigationGroup ng:
          if (foundNavigationGroup != null) {
            throw new InvalidOperationException("Multiple navigation groups are not supported");
          }

          foundNavigationGroup = ng;
          break;
        default:
          throw new InvalidOperationException($"Unsupported group type {group.GetType().Name} in {group}");
      }
    }

    var navigationGroup = foundNavigationGroup ?? throw new InvalidOperationException("Navigation group not found");

    StartAction("html items", "Rendering items to HTML pages");
    await renderDispatcher.RenderItems(items, navigationGroup).ConfigureAwait(false);
    FinishAction("Rendered", items.Length, "items");

    StartAction("html groups", "Rendering groups to HTML pages");
    await renderDispatcher.RenderGroups(htmlGroups).ConfigureAwait(false);
    FinishAction("Rendered", htmlGroups.Count, "groups");

    StartAction("rss", "Rendering RSS feeds");
    var rssTasks = rssFeeds.Select(rssGenerator.GenerateRss).ToArray();
    await outputEngine.ExecuteMany(rssTasks).ConfigureAwait(false);
    FinishAction("Rendered", rssFeeds.Count, "feeds");

    StartAction("sitemap", "Rendering sitemap");
    var sitemapTask = sitemapGenerator.GenerateSitemap(items, htmlGroups);
    await outputEngine.Execute(sitemapTask).ConfigureAwait(false);
    FinishAction("Rendered", 1, "sitemap");

    logger.LogInformation(Constants.CoreLog, "Build finished in {Time} seconds", topStopwatch.Elapsed.TotalSeconds);
  }

  private void StartAction(string scopeName, string introMessage) {
    _actionScope = logger.BeginScope(scopeName);
    logger.LogInformation(Constants.CoreLog, "{Action}", introMessage);
    _actionStopwatch.Restart();
  }

  private void FinishAction(string action, int count, string itemType) {
    var s = _actionStopwatch.Elapsed.TotalSeconds;
    logger.LogInformation(Constants.CoreLog, "=> {Action} {Count} {ItemType} in {Time} seconds",
        action, count, itemType, s);
    _actionScope?.Dispose();
    _actionScope = null;
  }
}
