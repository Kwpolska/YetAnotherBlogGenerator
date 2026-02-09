// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Config;

namespace YetAnotherBlogGenerator.TemplateRendering.Models;

public interface IModelBase {
  MenuItem[] MenuItems { get; }
  Dictionary<string, string> CategoryColors { get; }
  string SiteFooter { get; }
  string Title { get; }
  string Url { get; }
  Dictionary<string, string> CategoryTagSlugs { get; }
}
