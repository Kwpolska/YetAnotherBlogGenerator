// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Text;

namespace YetAnotherBlogGenerator.Utilities;

internal static class ImageHelper {
  public static (uint Width, uint Height) ScaleThumbnail(uint width, uint height) {
    return width > height
        ? (Constants.MaxThumbnailSize, (uint)((double)height / width * Constants.MaxThumbnailSize))
        : ((uint)((double)width / height * Constants.MaxThumbnailSize), Constants.MaxThumbnailSize);
  }

  public static string GetThumbnailPath(string path) {
    const string thumbnailExtension = ".thumbnail";
    var stringBuilder = new StringBuilder(path.Length + thumbnailExtension.Length);
    var extensionStart = path.LastIndexOf('.');
    var pathSpan = path.AsSpan();

    stringBuilder.Append(pathSpan[..extensionStart]);
    stringBuilder.Append(thumbnailExtension);
    stringBuilder.Append(pathSpan[extensionStart..]);

    return stringBuilder.ToString();
  }
}
