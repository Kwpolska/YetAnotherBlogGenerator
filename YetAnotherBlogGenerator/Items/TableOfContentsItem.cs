// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

namespace YetAnotherBlogGenerator.Items;

public record TableOfContentsItem(string Anchor, string Title, TableOfContentsItem[] Children);
