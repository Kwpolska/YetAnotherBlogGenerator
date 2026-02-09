// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

namespace YetAnotherBlogGenerator.Meta;

public record ProjectMeta(
    int Sort,
    int DevStatus,
    bool Featured,
    string? Download,
    string? GitHub,
    string? BugTracker,
    string? Role,
    string? License,
    string? Language,
    string? Logo);
