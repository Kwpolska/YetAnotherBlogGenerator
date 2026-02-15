// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

namespace YetAnotherBlogGenerator.ItemRendering.External;

internal interface IListingRenderer {
  public Task<string> RenderSingleListing(string code, string? path, string? language);
  public Task<IReadOnlyCollection<ExternalRenderResponse>> RenderMultipleListings(IEnumerable<ExternalRenderRequest> requests);
}
