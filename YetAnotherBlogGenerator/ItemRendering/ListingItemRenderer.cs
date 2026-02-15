// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.ItemRendering.External;
using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.ItemRendering;

internal class ListingItemRenderer(IListingRenderer listingRenderer) : IBulkItemRenderer {
  public const string Name = RendererNames.Listing;

  public async Task<RenderResult[]> RenderItems(IEnumerable<SourceItem> items) {
    var sourceItems = items
        .Select(item => new ExternalRenderSource(Guid.NewGuid(), item, item.Source))
        .ToArray();
    var requests = sourceItems.Select(sourceItem => new ExternalRenderRequest(
        Guid: sourceItem.Guid, Path: Path.GetFileName(sourceItem.Item.SourcePath), Source: sourceItem.Source));
    var responses = await listingRenderer.RenderMultipleListings(requests).ConfigureAwait(false);
    var responsesDict = responses.ToDictionary(r => r.Guid, r => r.Html);
    return sourceItems.Select(i => {
      var htmlFound = responsesDict.TryGetValue(i.Guid, out var html);
      return htmlFound
          ? new RenderResult(i.Item, html!)
          : throw new Exception($"Request for item {i.Item} was not returned by Pygments");
    }).ToArray();
  }
}
