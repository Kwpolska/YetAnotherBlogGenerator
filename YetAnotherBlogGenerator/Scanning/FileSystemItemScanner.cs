// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Items;
using YetAnotherBlogGenerator.Meta;

namespace YetAnotherBlogGenerator.Scanning;

internal class FileSystemItemScanner(IConfiguration configuration, IMetaParser metaParser) : IItemScanner {
  public Task<SourceItem[]> ScanForItems()
    => Task.WhenAll(configuration.ScanPatterns.SelectMany(pattern => {
      var topPath = Path.Combine(configuration.SourceRoot, pattern.StartDirectory);
      return Directory
          .GetFiles(
              path: topPath,
              searchPattern: pattern.FileNamePattern,
              SearchOption.AllDirectories)
          .Select(async path => {
            var itemType = pattern.ItemType;
            var source = (await File.ReadAllTextAsync(path).ConfigureAwait(false)).ReplaceLineEndings("\n");
            var metaParseResults = metaParser.Parse(itemFullSource: source, itemPath: path, itemType: itemType);

            var sourcePathElements = path.AsSpan()[topPath.Length..].TrimStart(Path.DirectorySeparatorChar).ToString()
                .Split(Path.DirectorySeparatorChar);

            return new SourceItem(
                Type: itemType,
                ScanPattern: pattern,
                SourcePath: path,
                SourcePathElements: sourcePathElements,
                Meta: metaParseResults.Meta,
                Source: metaParseResults.ItemSource);
          });
    }));
}
