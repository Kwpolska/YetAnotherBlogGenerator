using Markdig;
using Markdig.Renderers;

namespace YetAnotherBlogGenerator.ItemRendering.MarkdigExtras;

internal class WrappingHtmlTableRendererExtension : IMarkdownExtension {
  public void Setup(MarkdownPipelineBuilder pipeline) {
  }

  public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) {
    if (renderer is HtmlRenderer htmlRenderer && !htmlRenderer.ObjectRenderers.Contains<WrappingHtmlTableRenderer>()) {
      htmlRenderer.ObjectRenderers.Add(new WrappingHtmlTableRenderer());
    }
  }
}
