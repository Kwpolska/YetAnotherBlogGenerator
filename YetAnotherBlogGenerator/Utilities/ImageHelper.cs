// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Text;

namespace YetAnotherBlogGenerator.Utilities;

internal static class ImageHelper {
  private const int MaxThumbnailSize = 300;

  public static (int Width, int Height) ScaleThumbnail(int width, int height) {
    return width > height
        ? (MaxThumbnailSize, (int)((double)height / width * MaxThumbnailSize))
        : ((int)((double)width / height * MaxThumbnailSize), MaxThumbnailSize);
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
