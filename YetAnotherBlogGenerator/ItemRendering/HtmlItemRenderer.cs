// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.ItemRendering;

internal class HtmlItemRenderer : ISingleItemRenderer {
  public const string Name = RendererNames.Html;

  public Task<string> RenderFullHtml(SourceItem item)
    => Task.FromResult(item.Source);
}
