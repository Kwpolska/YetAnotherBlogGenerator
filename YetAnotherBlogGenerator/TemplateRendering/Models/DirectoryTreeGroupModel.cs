// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Groups;

namespace YetAnotherBlogGenerator.TemplateRendering.Models;

public class DirectoryTreeGroupModel(DirectoryTreeGroup group, IConfiguration configuration)
    : ModelBase<DirectoryTreeGroup>(group, configuration) {
  public DirectoryTreeGroup Group => Renderable;
  public DirectoryTreeEntry[] Entries => Renderable.Entries;
  public Link[] Breadcrumbs => Renderable.Breadcrumbs;
}
