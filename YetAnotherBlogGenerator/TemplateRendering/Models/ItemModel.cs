// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;
using YetAnotherBlogGenerator.Utilities;

namespace YetAnotherBlogGenerator.TemplateRendering.Models;

public class ItemModel(Item item, NavigationGroup navigationGroup, IConfiguration configuration)
    : ModelBase<Item>(item, configuration) {
  public Item Item => Renderable;
  public NavigationGroupItem? PostNavigation { get; } = navigationGroup.ForItem(item);
  public Link[] Breadcrumbs => BreadcrumbsHelper.BuildBreadcrumbs(Item.Url);
  public string? PreviousUrl => PostNavigation?.Previous?.Url;
  public string? NextUrl => PostNavigation?.Next?.Url;
}
