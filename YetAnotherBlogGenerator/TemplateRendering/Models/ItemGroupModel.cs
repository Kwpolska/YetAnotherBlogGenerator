// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.TemplateRendering.Models;

public class ItemGroupModel(ItemGroup group, IConfiguration configuration)
    : ModelBase<ItemGroup>(group, configuration) {
  public ItemGroup Group => Renderable;
  public Item[] Items => Renderable.Items;
  public Item[] FeaturedProjects => Renderable.Items.Where(i => i.Meta.Project!.Featured).ToArray();
  public string? PreviousUrl => Renderable.PreviousGroupUrl;
  public string? NextUrl => Renderable.NextGroupUrl;
}
