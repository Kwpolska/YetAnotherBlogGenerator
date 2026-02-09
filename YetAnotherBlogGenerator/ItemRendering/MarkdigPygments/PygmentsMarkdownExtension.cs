// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

// This code is largely taken from Markding.SyntaxHighlighting, which is licensed under the Apache-2.0 license.
// https://github.com/RichardSlater/Markdig.SyntaxHighlighting/blob/master/src/Markdig.SyntaxHighlighting/SyntaxHighlightingExtension.cs

using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using YetAnotherBlogGenerator.ItemRendering.External;

namespace YetAnotherBlogGenerator.ItemRendering.MarkdigPygments;

internal class PygmentsMarkdownExtension(IListingRenderer listingRenderer) : IMarkdownExtension {
  public void Setup(MarkdownPipelineBuilder pipeline) {
  }

  public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) {
    ArgumentNullException.ThrowIfNull(renderer);

    if (renderer is not TextRendererBase<HtmlRenderer> htmlRenderer) {
      return;
    }

    var originalCodeBlockRenderer = htmlRenderer.ObjectRenderers.FindExact<CodeBlockRenderer>();
    if (originalCodeBlockRenderer != null) {
      htmlRenderer.ObjectRenderers.Remove(originalCodeBlockRenderer);
    }

    htmlRenderer.ObjectRenderers.AddIfNotAlready(
        new PygmentsCodeBlockRenderer(listingRenderer));
  }
}
