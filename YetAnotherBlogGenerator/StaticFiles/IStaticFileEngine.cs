// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Items;
using YetAnotherBlogGenerator.Output;

namespace YetAnotherBlogGenerator.StaticFiles;

internal interface IStaticFileEngine {
  IEnumerable<CopyTask> CopyAllFiles();
  IEnumerable<CopyTask> CopyItemFiles(Item[] items);
}
