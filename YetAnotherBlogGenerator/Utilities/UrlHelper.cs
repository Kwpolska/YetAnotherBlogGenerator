// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Text;
using YetAnotherBlogGenerator.Config;

namespace YetAnotherBlogGenerator.Utilities;

public class UrlHelper(IConfiguration configuration) : IUrlHelper {
  public string UrlToOutputPath(string url) {
    var stringBuilder = new StringBuilder();
    stringBuilder.Append(configuration.OutputFolder);
    if (configuration.OutputFolder.EndsWith(Path.DirectorySeparatorChar)) {
      stringBuilder.Remove(stringBuilder.Length - 1, 1);
    }

    if (!url.StartsWith(Constants.UrlPathSeparator)) {
      throw new InvalidOperationException($"URLs must start with '{Constants.UrlPathSeparator}'.");
    }

    stringBuilder.Append(url);
    if (url.EndsWith(Constants.UrlPathSeparator)) {
      stringBuilder.Append(Constants.IndexHtml);
    }

    stringBuilder.Replace(Constants.UrlPathSeparator, Path.DirectorySeparatorChar);

    return stringBuilder.ToString();
  }
}
