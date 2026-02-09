// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Groups;

namespace YetAnotherBlogGenerator.TemplateRendering.Models;

public class LinkGroupModel(LinkGroup group, IConfiguration configuration)
    : ModelBase<LinkGroup>(group, configuration) {
  public LinkGroup Group => Renderable;
  public LinkGroupItem[] Links => Renderable.Links;
}
