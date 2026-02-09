// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.ItemRendering.External;

internal record ExternalRenderSource(Guid Guid, SourceItem Item, string Source);
