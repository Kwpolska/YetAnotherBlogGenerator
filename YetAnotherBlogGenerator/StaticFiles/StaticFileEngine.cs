// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Items;
using YetAnotherBlogGenerator.Output;

namespace YetAnotherBlogGenerator.StaticFiles;

internal class StaticFileEngine(IConfiguration configuration) : IStaticFileEngine {
  public IEnumerable<CopyTask> CopyAllFiles() {
    var fileTasks = CopyFiles(CommonFolders.Files);
    var imageTasks = CopyFiles(CommonFolders.Images);

    return fileTasks.Concat(imageTasks);
  }

  private IEnumerable<CopyTask> CopyFiles(string sourceFolderName) {
    var sourceFolder = Path.Combine(configuration.SourceRoot, sourceFolderName);
    var files = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);

    return files.Select(sourcePath => new CopyTask(
        sourcePath,
        Path.Combine(configuration.OutputFolder, Path.GetRelativePath(sourceFolder, sourcePath))));
  }

  public IEnumerable<CopyTask> CopyItemFiles(Item[] items) {
    var listingFiles = items.Where(i => i.Type == ItemType.Listing).Select(CopyListingFile);
    var galleryFiles = items.Where(i => i.Type == ItemType.Gallery).SelectMany(CopyGalleryFiles);
    return listingFiles.Concat(galleryFiles);
  }

  private CopyTask CopyListingFile(Item listing) {
    var sourcePath = listing.SourcePath;
    var targetPath = Path.Combine([
        configuration.OutputFolder, listing.ScanPattern.TargetDirectory, ..listing.SourcePathElements
    ]);

    return new CopyTask(sourcePath, targetPath);
  }

  private IEnumerable<CopyTask> CopyGalleryFiles(Item gallery) {
    if (gallery.RichItemData is not GalleryData galleryData) {
      return [];
    }

    var sourcePath = Path.GetDirectoryName(gallery.SourcePath)!;
    var targetPath = Path.Combine([
        configuration.OutputFolder, gallery.ScanPattern.TargetDirectory, ..gallery.SourcePathElements.SkipLast(1)
    ]);

    return galleryData.Images.Select(i => new CopyTask(
        Source: Path.Combine(sourcePath, i.FileName),
        Destination: Path.Combine(targetPath, i.FileName)
    ));
  }
}
