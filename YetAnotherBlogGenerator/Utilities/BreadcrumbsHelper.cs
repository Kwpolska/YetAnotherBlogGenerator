// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Text;
using YetAnotherBlogGenerator.Groups;

namespace YetAnotherBlogGenerator.Utilities;

public static class BreadcrumbsHelper {
  public static Link[] BuildBreadcrumbs(string url) {
    var parts = url.Trim('/').Split('/');
    var breadcrumbs = new Link[parts.Length];

    for (var i = 0; i < parts.Length; i++) {
      var partUrlBuilder = new StringBuilder();
      partUrlBuilder.Append('/');
      partUrlBuilder.AppendJoin('/', parts.AsSpan()[..(i + 1)]);
      if (i != parts.Length - 1 || url.EndsWith('/')) partUrlBuilder.Append('/');
      const string htmlSuffix = ".html";
      var part = parts[i].EndsWith(htmlSuffix) ? parts[i][..^htmlSuffix.Length] : parts[i];
      breadcrumbs[i] = new Link(partUrlBuilder.ToString(), part);
    }

    return breadcrumbs;
  }
}
