// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Text.Json.Serialization;

namespace YetAnotherBlogGenerator.Items;

public record GalleryImage(
    [property: JsonPropertyName("fileName")]
    string FileName,
    [property: JsonPropertyName("thumbnailName")]
    string ThumbnailName,
    [property: JsonPropertyName("description")]
    string Description,
    [property: JsonPropertyName("width")]
    int Width,
    [property: JsonPropertyName("height")]
    int Height);
