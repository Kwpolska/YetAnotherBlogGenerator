// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

namespace YetAnotherBlogGenerator.ItemRendering.External;

internal record ExternalRenderResponse(Guid Guid, string? Path, bool Success, string Html);
