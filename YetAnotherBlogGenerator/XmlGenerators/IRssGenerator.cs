// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Output;

namespace YetAnotherBlogGenerator.XmlGenerators;

internal interface IRssGenerator {
  WriteXmlTask GenerateRss(ItemRssGroup group);
}
