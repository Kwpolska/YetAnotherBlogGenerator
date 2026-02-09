// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Text;
using Microsoft.Extensions.Logging;
using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;
using YetAnotherBlogGenerator.Utilities;

namespace YetAnotherBlogGenerator.TemplateRendering;

internal class RenderDispatcher(ITemplateEngine engine, ILogger<RenderDispatcher> logger, IUrlHelper urlHelper)
    : IRenderDispatcher {
  public async Task RenderItems(IEnumerable<Item> items, NavigationGroup navigationGroup) {
    var renderAction = (Item post, TextWriter textWriter) => engine.RenderItem(post, navigationGroup, textWriter);
    await Parallel.ForEachAsync(items, async (i, _) => await RenderItem(i, renderAction).ConfigureAwait(false))
        .ConfigureAwait(false);
  }

  public async Task RenderGroups(IEnumerable<IHtmlGroup> groups) {
    await Parallel.ForEachAsync(groups, async (g, _) => await RenderGroup(g).ConfigureAwait(false))
        .ConfigureAwait(false);
  }

  private async Task RenderItem(Item item, Func<Item, TextWriter, Task> renderAction) {
    await RenderCore(item, renderAction).ConfigureAwait(false);
  }

  private async Task RenderGroup(IHtmlGroup group) {
    await RenderCore(group, engine.RenderGroup).ConfigureAwait(false);
  }

  private async Task RenderCore<T>(T renderable, Func<T, TextWriter, Task> renderAction) where T : IRenderable {
    var outputPath = urlHelper.UrlToOutputPath(renderable.Url);
    if (!outputPath.EndsWith(".html")) {
      throw new InvalidOperationException($"Output URL '{renderable.Url}' does not end with '.html'.");
    }

    var outputParentFolder = Path.GetDirectoryName(outputPath)!;
    Directory.CreateDirectory(outputParentFolder);
    var writer = new StreamWriter(outputPath, append: false, Encoding.UTF8NoBom);
    writer.NewLine = "\n";
    await using var _ = writer.ConfigureAwait(false);
    await renderAction(renderable, writer).ConfigureAwait(false);
    logger.LogDebug(Constants.RenderLog, "{Destination}", outputPath);
  }
}
