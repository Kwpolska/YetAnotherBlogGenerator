// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using Markdig;
using YetAnotherBlogGenerator.ItemRendering.External;
using YetAnotherBlogGenerator.ItemRendering.MarkdigPygments;
using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.ItemRendering;

internal class MarkdownItemRenderer(IListingRenderer listingRenderer) : ISingleItemRenderer {
  public const string Name = RendererNames.Markdown;

  private readonly MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder()
      .UseAdvancedExtensions()
      .Use(new PygmentsMarkdownExtension(listingRenderer))
      .Build();

  public Task<string> RenderFullHtml(SourceItem item)
    => Task.Run(() => Markdown.ToHtml(item.Source, _markdownPipeline));
}
