// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;
using YetAnotherBlogGenerator.Output;

namespace YetAnotherBlogGenerator.XmlGenerators;

public interface ISitemapGenerator {
  WriteXmlTask GenerateSitemap(IReadOnlyCollection<Item> items, IReadOnlyCollection<IHtmlGroup> htmlGroups);
}
