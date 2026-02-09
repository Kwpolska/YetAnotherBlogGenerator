// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using YetAnotherBlogGenerator.ItemRendering.External;

namespace YetAnotherBlogGenerator.ItemRendering.MarkdigPygments;

internal class PygmentsCodeBlockRenderer(IListingRenderer listingRenderer) : HtmlObjectRenderer<CodeBlock> {
  protected override void Write(HtmlRenderer renderer, CodeBlock obj) {
    string? language = (obj as FencedCodeBlock)?.Info;

    var code = obj.Lines.ToString();
    var task = listingRenderer.RenderSingleListing(code: code, path: null, language: language);
    string renderResult;

    try {
      renderResult = task.Result;
    } catch (AggregateException exc) {
      var flatExc = exc.Flatten();
      if (flatExc.InnerExceptions.Count == 1) throw flatExc.InnerExceptions[0];
      throw flatExc;
    }

    renderer.WriteLine(renderResult);
  }
}
