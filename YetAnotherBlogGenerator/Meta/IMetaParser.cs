// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Meta;

internal interface IMetaParser {
  MetaParseResult Parse(string itemFullSource, string itemPath, ItemType itemType);
}
