// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Scanning;
using YetAnotherBlogGenerator.StaticFiles;

namespace YetAnotherBlogGenerator.Config;

public interface IConfiguration {
  string SiteTitle { get; }
  Uri SiteUri { get; }
  string SiteAuthor { get; }
  string SiteDescription { get; }
  string SiteFooter { get; }

  string SourceRoot { get; }
  string OutputFolder { get; }

  MenuItem[] MenuItems { get; }
  ScanPattern[] ScanPatterns { get; }

  int IndexSize { get; }
  int FeedSize { get; }

  Dictionary<string, string> CategoryColors { get; }
  Dictionary<string, string> TagsAndCategoriesCustomSlugs { get; }
  AssetBundle[] AssetBundles { get; }

  string PygmentsAdapterPythonBinary { get; }
}
