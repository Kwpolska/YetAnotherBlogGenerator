// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Text;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.ItemRendering;

internal class ItemRenderEngine(IConfiguration configuration, IServiceProvider serviceProvider) : IItemRenderEngine {
  private readonly IKeyedServiceProvider _keyedServiceProvider = (IKeyedServiceProvider)serviceProvider;

  public async Task<IEnumerable<Item>> Render(IEnumerable<SourceItem> sourceItems) {
    var renderTasks = sourceItems
        .GroupBy(i => i.ScanPattern.RendererName)
        .Select(group => {
          var renderer = _keyedServiceProvider.GetRequiredKeyedService<IItemRenderer>(group.Key);
          switch (renderer) {
            case IBulkItemRenderer bulkRenderer:
              return bulkRenderer.RenderFullHtml(group);
            case ISingleItemRenderer singleRenderer:
              return Task.WhenAll(group.Select(item => RenderWithSingleRenderer(singleRenderer, item)));
            default:
              throw new InvalidOperationException("Unexpected renderer type");
          }
        });

    return (await Task.WhenAll(renderTasks).ConfigureAwait(false))
        .SelectMany(x => x)
        .Select(result => {
          var sourceItem = result.Item;
          var html = result.Html;
          var richItemData = result.RichItemData;

          var teaser = string.Empty;
          var content = html;

          if (sourceItem.ScanPattern.SupportsTeasers) {
            var splitResult = Constants.TeaserRegex.Split(html, 2);
            if (splitResult.Length == 2) {
              teaser = splitResult[0];
              content = splitResult[1];
            }
          }

          var urlBuilder = new StringBuilder(sourceItem.SourcePath.Length - configuration.SourceRoot.Length);
          if (!string.IsNullOrEmpty(sourceItem.ScanPattern.TargetDirectory)) {
            urlBuilder.Append('/');
            urlBuilder.Append(sourceItem.ScanPattern.TargetDirectory);
          }

          var sourcePathElementsSpan = sourceItem.SourcePathElements.AsSpan();
          foreach (var e in sourcePathElementsSpan[..^1]) {
            urlBuilder.Append('/');
            urlBuilder.Append(e);
          }

          urlBuilder.Append('/');

          urlBuilder.Append(Path.GetFileNameWithoutExtension(sourcePathElementsSpan[^1]));

          if (sourceItem.ScanPattern.UsePrettyUrls) {
            urlBuilder.Append('/');
          } else {
            if (sourceItem.ScanPattern.ItemType == ItemType.Listing) {
              var ext = Path.GetExtension(sourcePathElementsSpan[^1]);
              urlBuilder.Append(ext);
            }

            urlBuilder.Append(".html");
          }

          return new Item(
              Type: sourceItem.Type,
              ScanPattern: sourceItem.ScanPattern,
              SourcePath: sourceItem.SourcePath,
              SourcePathElements: sourceItem.SourcePathElements,
              Url: urlBuilder.ToString(),
              Meta: sourceItem.Meta,
              Content: content,
              Teaser: teaser,
              RichItemData: richItemData
          );
        });
  }

  private static async Task<BulkRenderResult> RenderWithSingleRenderer(ISingleItemRenderer singleRenderer,
      SourceItem item) {
    var html = await singleRenderer.RenderFullHtml(item).ConfigureAwait(false);
    var richItemData = await singleRenderer.GenerateRichItemData(item).ConfigureAwait(false);
    return new BulkRenderResult(item, html, richItemData);
  }
}
