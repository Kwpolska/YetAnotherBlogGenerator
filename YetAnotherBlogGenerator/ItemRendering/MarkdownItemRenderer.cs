// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using YetAnotherBlogGenerator.ItemRendering.External;
using YetAnotherBlogGenerator.ItemRendering.MarkdigPygments;
using YetAnotherBlogGenerator.Items;
using YetAnotherBlogGenerator.Utilities;

namespace YetAnotherBlogGenerator.ItemRendering;

internal class MarkdownItemRenderer(
    IListingRenderer listingRenderer,
    ITableOfContentsGenerator tableOfContentsGenerator) : ISingleItemRenderer {
  public const string Name = RendererNames.Markdown;

  private readonly MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder()
      .UseAdvancedExtensions()
      .Use(new PygmentsMarkdownExtension(listingRenderer))
      .Build();

  public Task<RenderResult> RenderItem(SourceItem item) {
    return Task.Run(() => {
      var markdown = Markdown.Parse(item.Source, _markdownPipeline);
      var html = markdown.ToHtml(_markdownPipeline);
      var tableOfContents = tableOfContentsGenerator.BuildTree(BlocksToHeadings(markdown));
      return new RenderResult(item, html, TableOfContents: tableOfContents);
    });
  }

  private static IEnumerable<Heading> BlocksToHeadings(MarkdownDocument markdown) {
    using var writer = new StringWriter();
    var htmlRenderer = new HtmlRenderer(writer);
    foreach (var block in markdown) {
      if (block is not HeadingBlock headingBlock) continue;
      var anchor = headingBlock.GetAttributes().Id;
      if (anchor is null) continue;

      htmlRenderer.WriteLeafInline(headingBlock);
      writer.Flush();
      var headingText = writer.ToString();
      writer.GetStringBuilder().Clear();

      yield return new Heading(Anchor: anchor, Title: headingText, Level: headingBlock.Level);
    }
  }
}
