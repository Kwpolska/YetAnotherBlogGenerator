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
using YetAnotherBlogGenerator.Utilities;
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
    IThumbnailEngine thumbnailEngine,
    IUrlHelper urlHelper) : IMainEngine {
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
    var htmlGroups = new List<IHtmlGroup>();
    var rssFeeds = new List<RssFeed>();
    NavigationGroup? foundNavigationGroup = null;

    foreach (var group in groupEngine.GenerateGroups(items)) {
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
    FinishAction("Generated", htmlGroups.Count + rssFeeds.Count + 1, "groups");

    StartAction("static", "Copying static files");
    var copyTasks = staticFileEngine.CopyAllFiles().Concat(staticFileEngine.CopyItemFiles(items)).ToArray();
    await outputEngine.ExecuteMany(copyTasks).ConfigureAwait(false);
    FinishAction("Copied", copyTasks.Length, "files");

    StartAction("assets", "Processing assets");
    var assetBundleOutputPaths = await assetBundleEngine.BundleAssets().ConfigureAwait(false);
    await cacheBustingService.PreCacheAssetUrls().ConfigureAwait(false); // trivial, no separate log message needed
    FinishAction("Generated", assetBundleOutputPaths.Length, "asset bundles");

    StartAction("thumbnails", "Generating thumbnails");
    var thumbnailTasks = thumbnailEngine.GenerateThumbnailsForImagesFolder()
        .Concat(thumbnailEngine.GenerateThumbnailsForItems(items)).ToArray();
    await outputEngine.ExecuteMany(thumbnailTasks).ConfigureAwait(false);
    FinishAction("Generated", thumbnailTasks.Length, "thumbnails");

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

    var allOutputTasks = copyTasks.Cast<IOutputTask>().Concat(thumbnailTasks).Concat(rssTasks).Append(sitemapTask)
        .Select(t => new WrittenOutputTask(t.Destination, t));
    var allRenderables = items.Cast<IRenderable>().Concat(htmlGroups)
        .Select(r => new WrittenRenderable(urlHelper.UrlToOutputPath(r.Url), r));

    var duplicates = allOutputTasks.Cast<IWritten>().Concat(allRenderables)
        .Concat(assetBundleOutputPaths.Select(b => new WrittenBundle(b)))
        .GroupBy(w => w.Destination)
        .Where(g => g.Count() > 1)
        .ToArray();

    if (duplicates.Length == 0) {
      logger.LogTrace(Constants.CoreLog, "No duplicate files detected");
    } else {
      logger.LogError(Constants.CoreLog, "Found files generated by multiple sources");
      foreach (var group in duplicates) {
        logger.LogError(Constants.CoreLog, "{File}", group.Key);
        foreach (var written in group) {
          logger.LogError(Constants.CoreLog, " -> {Source}", written);
        }
      }
    }

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

  private interface IWritten {
    string Destination { get; }
  }

  private record WrittenOutputTask(string Destination, IOutputTask Task) : IWritten;
  private record WrittenRenderable(string Destination, IRenderable Renderable) : IWritten;
  private record WrittenBundle(string Destination) : IWritten;
}
