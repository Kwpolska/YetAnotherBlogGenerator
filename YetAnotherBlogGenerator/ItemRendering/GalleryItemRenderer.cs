// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using ImageMagick;
using YetAnotherBlogGenerator.Cache;
using YetAnotherBlogGenerator.Items;
using YetAnotherBlogGenerator.Utilities;

namespace YetAnotherBlogGenerator.ItemRendering;

internal class GalleryItemRenderer(ICacheService cacheService) : ISingleItemRenderer {
  public const string Name = RendererNames.Gallery;

  public Task<RenderResult> RenderItem(SourceItem item) {
    var html = (string)item.Meta.CustomFields.GetValueOrDefault(MetaFields.GalleryIntroHtml, "");

    using var reader = new StringReader(item.Source);
    var sourceDirectory = Path.GetDirectoryName(item.SourcePath)!;
    var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "\t", Encoding = Encoding.UTF8 };
    using var csv = new CsvReader(reader, config);
    var csvImages = csv.GetRecords<CsvGalleryImage>().ToArray();
    var images = csvImages.Select(i => GetImageDetails(sourceDirectory, i)).ToArray();

    var result = new RenderResult(Item: item, Html: html, RichItemData: new GalleryData(images));
    return Task.FromResult(result);
  }

  private GalleryImage GetImageDetails(string sourceDirectory, CsvGalleryImage csvGalleryImage) {
    const string cacheSource = "GalleryItemRenderer.GetImageDetails";
    const int cacheVersion = 2;

    var path = Path.Join(sourceDirectory, csvGalleryImage.Name);
    var lastWriteTime = new DateTimeOffset(File.GetLastWriteTimeUtc(path), TimeSpan.Zero);
    var cacheDetails = cacheService.Get<CacheDetails>(cacheSource, path);
    if (cacheDetails is { Version: cacheVersion } && cacheDetails.LastWriteTime == lastWriteTime) {
      return Image(csvGalleryImage, cacheDetails.Width, cacheDetails.Height);
    }

    using var image = new MagickImage(path);
    var width = image.Width;
    var height = image.Height;
    cacheDetails = new CacheDetails(Version: cacheVersion, LastWriteTime: lastWriteTime, Width: width, Height: height);
    cacheService.Set(cacheSource, path, cacheDetails);

    return Image(csvGalleryImage, width, height);
  }

  private GalleryImage Image(CsvGalleryImage csvGalleryImage, uint width, uint height) {
    var thumbnailName = ImageHelper.GetThumbnailPath(csvGalleryImage.Name);
    var (thumbnailWidth, thumbnailHeight) = ImageHelper.ScaleThumbnail(width, height);

    return new GalleryImage(
        FileName: csvGalleryImage.Name,
        ThumbnailName: thumbnailName,
        Description: csvGalleryImage.Description,
        Width: thumbnailWidth,
        Height: thumbnailHeight);
  }

  private record CacheDetails(int Version, DateTimeOffset LastWriteTime, uint Width, uint Height);
}
