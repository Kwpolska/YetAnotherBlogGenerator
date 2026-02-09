// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Grouping;

public class NavigationGrouper : IPostGrouper {
  public IEnumerable<IGroup> GroupPosts(Item[] posts) {
    var groupItems = new NavigationGroupItem[posts.Length];
    for (int i = 0; i < posts.Length; i++) {
      // The list is sorted by date descending (the first post in the list is the newest)
      var previousPost = i == (posts.Length - 1) ? null : posts[i + 1];
      var nextPost = i == 0 ? null : posts[i - 1];
      groupItems[i] = new NavigationGroupItem(posts[i], previousPost, nextPost);
    }

    var group = new NavigationGroup(groupItems);
    return [group];
  }
}
