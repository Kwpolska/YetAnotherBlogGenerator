// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Utilities;

namespace YetAnotherBlogGenerator.TemplateRendering.Models;

public class ModelBase<T>(T renderable, IConfiguration configuration) : IModelBase
    where T : IRenderable {
  public MenuItem[] MenuItems => configuration.MenuItems;

  public Dictionary<string, string> CategoryColors => configuration.CategoryColors;

  public Dictionary<string, string> CategoryTagSlugs => configuration.TagsAndCategoriesCustomSlugs;

  public string SiteFooter => configuration.SiteFooter;

  protected readonly T Renderable = renderable;

  public string Title => Renderable.Title;

  public string Url => Renderable.Url;

  public string AbsoluteUrl => new Uri(configuration.SiteUri, Url).ToString();
}
