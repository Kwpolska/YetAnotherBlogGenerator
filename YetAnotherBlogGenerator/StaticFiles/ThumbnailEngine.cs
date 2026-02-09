// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using SkiaSharp;
using YetAnotherBlogGenerator.Cache;
using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Items;
using YetAnotherBlogGenerator.Output;
using YetAnotherBlogGenerator.Utilities;

namespace YetAnotherBlogGenerator.StaticFiles;

internal class ThumbnailEngine(ICacheService cacheService, IConfiguration configuration) : IThumbnailEngine {
  public IEnumerable<WriteBinaryTask> GenerateThumbnailsForImagesFolder() {
    var cacheBase = GetCacheBase();
    var filesRoot = Path.Combine(configuration.SourceRoot, CommonFolders.Images);
    var files = Directory.GetFiles(filesRoot, "*", SearchOption.AllDirectories);

    return files
        .Select(sourcePath => GenerateThumbnailIfMissing(
            source: sourcePath,
            destination: ImageHelper.GetThumbnailPath(Path.Combine(configuration.OutputFolder,
                Path.GetRelativePath(filesRoot, sourcePath))),
            cacheBase: cacheBase))
        .WhereNotNull();
  }

  public IEnumerable<WriteBinaryTask> GenerateThumbnailsForItems(Item[] items) {
    var cacheBase = GetCacheBase();
    var galleryFiles = items.Where(i => i.Type == ItemType.Gallery)
        .SelectMany(gallery => GenerateThumbnailsForGallery(gallery, cacheBase));
    return galleryFiles;
  }

  private IEnumerable<WriteBinaryTask> GenerateThumbnailsForGallery(Item gallery, DateTimeOffset? cacheBase) {
    if (gallery.RichItemData is not GalleryData galleryData) {
      return [];
    }

    var sourcePath = Path.GetDirectoryName(gallery.SourcePath)!;
    var targetPath = Path.Combine([
        configuration.OutputFolder, gallery.ScanPattern.TargetDirectory, ..gallery.SourcePathElements.SkipLast(1)
    ]);

    return galleryData.Images
        .Select(i => GenerateThumbnailIfMissing(
            source: Path.Combine(sourcePath, i.FileName),
            destination: ImageHelper.GetThumbnailPath(Path.Combine(targetPath, i.FileName)),
            cacheBase: cacheBase
        ))
        .WhereNotNull();
  }

  private DateTimeOffset? GetCacheBase() {
    var lastRenderStatus = cacheService.Get<LastRenderStatus>(Constants.CoreCacheSource, nameof(LastRenderStatus));
    return lastRenderStatus is { Successful: true } ? lastRenderStatus.StartTime : null;
  }

  private WriteBinaryTask? GenerateThumbnailIfMissing(string source, string destination, DateTimeOffset? cacheBase) {
    if (cacheBase == null || !Path.Exists(destination)) {
      return GenerateThumbnail(source, destination);
    }

    var lastWriteTime = new DateTimeOffset(File.GetLastWriteTimeUtc(source), TimeSpan.Zero);
    return lastWriteTime >= cacheBase ? GenerateThumbnail(source, destination) : null;
  }

  private static WriteBinaryTask GenerateThumbnail(string source, string destination) {
    // Code based on:
    // https://stackoverflow.com/a/79543965 
    // https://stackoverflow.com/a/50344496
    using var codec = SKCodec.Create(source);
    using var originalBitmap = SKBitmap.Decode(codec);
    using var sourceImage = SKImage.FromBitmap(originalBitmap);

    (int resizedWidth, int resizedHeight) = ImageHelper.ScaleThumbnail(sourceImage.Width, sourceImage.Height);

    using var surface = SKSurface.Create(new SKImageInfo {
        Width = resizedWidth,
        Height = resizedHeight,
        ColorType = SKImageInfo.PlatformColorType,
        AlphaType = SKAlphaType.Premul
    });
    using var paint = new SKPaint();
    // high quality with antialiasing
    paint.IsAntialias = true;

    // draw the bitmap to fill the surface
    surface.Canvas.DrawImage(sourceImage, new SKRectI(0, 0, resizedWidth, resizedHeight),
        new SKSamplingOptions(new SKCubicResampler()),
        paint);
    surface.Canvas.Flush();

    using var newImage = surface.Snapshot();
    using var newImageData = newImage.Encode(codec.EncodedFormat, 95);
    var content = newImageData.ToArray();
    return new WriteBinaryTask(content, destination);
  }
}
