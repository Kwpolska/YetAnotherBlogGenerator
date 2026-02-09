// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Grouping;

internal class ListingIndexGrouper(IGroupFormatter groupFormatter) : IItemGrouper {
  public IEnumerable<IGroup> GroupItems(Item[] items) {
    var listings = items
        .Where(i => i.Type == ItemType.Listing);

    return groupFormatter.BuildDirectoryTreeGroups(listings, "listing_list.liquid", "Listings (/", ")");
  }
}
