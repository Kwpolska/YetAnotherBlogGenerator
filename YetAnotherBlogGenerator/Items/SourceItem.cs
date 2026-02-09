// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Meta;
using YetAnotherBlogGenerator.Scanning;

namespace YetAnotherBlogGenerator.Items;

internal record SourceItem(
    ItemType Type,
    ScanPattern ScanPattern,
    string SourcePath,
    string[] SourcePathElements,
    ItemMeta Meta,
    string Source);
