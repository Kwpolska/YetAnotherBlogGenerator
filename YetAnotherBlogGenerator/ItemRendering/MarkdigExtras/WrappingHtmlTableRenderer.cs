using Markdig.Extensions.Tables;
using Markdig.Renderers;

namespace YetAnotherBlogGenerator.ItemRendering.MarkdigExtras;

public class WrappingHtmlTableRenderer : HtmlTableRenderer {
  protected override void Write(HtmlRenderer renderer, Table table) {
    if (!renderer.EnableHtmlForBlock) {
      base.Write(renderer, table);
      return;
    }

    renderer.EnsureLine();
    renderer.Write("""<div class="table-wrapper">""");
    base.Write(renderer, table);
    renderer.Write("</div>");
  }
}
