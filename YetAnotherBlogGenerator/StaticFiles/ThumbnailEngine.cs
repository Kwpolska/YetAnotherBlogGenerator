// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using ImageMagick;
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
    using var image = new MagickImage(source);

    if (image.Width > Constants.MaxThumbnailSize || image.Height > Constants.MaxThumbnailSize) {
      var (thumbnailWidth, thumbnailHeight) = ImageHelper.ScaleThumbnail(image.Width, image.Height);
      var size = new MagickGeometry(thumbnailWidth, thumbnailHeight) { IgnoreAspectRatio = true };
      image.Resize(size);
    }

    var content = image.ToByteArray(image.Format);

    return new WriteBinaryTask(content, destination);
  }
}
