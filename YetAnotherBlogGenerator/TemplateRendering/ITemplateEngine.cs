// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.TemplateRendering;

internal interface ITemplateEngine {
  Task RenderItem(Item item, NavigationGroup navigationGroup, TextWriter outputStream);
  Task RenderGroup(IHtmlGroup group, TextWriter writer);
}
