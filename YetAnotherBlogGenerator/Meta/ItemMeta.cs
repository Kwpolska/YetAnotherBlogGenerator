// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

namespace YetAnotherBlogGenerator.Meta;

public record ItemMeta(
    string Title,
    DateTimeOffset Published,
    DateTimeOffset? Updated,
    string[] Tags,
    string? Category,
    string? Description,
    string? Thumbnail,
    string? Template,
    bool Comments,
    string? ShortLink,
    GuideMeta? Guide,
    ProjectMeta? Project,
    bool PublishedDateInSource,
    Dictionary<string, object> CustomFields) {
  public DateTimeOffset UpdatedOrPublished => Updated ?? Published;
}
