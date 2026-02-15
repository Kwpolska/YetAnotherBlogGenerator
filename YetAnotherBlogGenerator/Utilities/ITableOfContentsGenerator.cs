// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Utilities;

public interface ITableOfContentsGenerator {
  TableOfContentsItem[] BuildTree(IEnumerable<Heading> headings);
  IReadOnlyCollection<TableOfContentsItem> ExtractTableOfContents(string html, bool isLegacy);
}
