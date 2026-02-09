// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Scanning;
using YetAnotherBlogGenerator.StaticFiles;

namespace YetAnotherBlogGenerator.Config;

internal class Configuration(
    string sourceRoot,
    SiteConfiguration siteConfiguration,
    LocalConfiguration localConfiguration)
    : IConfiguration {
  public string SiteTitle { get; } = siteConfiguration.Site.Title;
  public Uri SiteUri { get; } = new(siteConfiguration.Site.Uri);
  public string SiteAuthor { get; } = siteConfiguration.Site.Author;
  public string SiteDescription { get; } = siteConfiguration.Site.Description;
  public string SiteFooter { get; } = siteConfiguration.Site.Footer.Trim();

  public MenuItem[] MenuItems { get; } = siteConfiguration.Menu;
  public ScanPattern[] ScanPatterns { get; } = siteConfiguration.ScanPatterns;
  public AssetBundle[] AssetBundles { get; } = siteConfiguration.AssetBundles;

  public int IndexSize { get; } = siteConfiguration.Grouping.IndexSize;
  public int FeedSize { get; } = siteConfiguration.Grouping.FeedSize;

  public Dictionary<string, string> CategoryColors { get; } = siteConfiguration.TagsAndCategories.CategoryColors;
  public Dictionary<string, string> TagsAndCategoriesCustomSlugs { get; } = siteConfiguration.TagsAndCategories.CustomSlugs;

  public string SourceRoot { get; } = sourceRoot;

  public string OutputFolder { get; } = Path.IsPathRooted(localConfiguration.OutputFolder)
      ? localConfiguration.OutputFolder
      : Path.Combine(sourceRoot, localConfiguration.OutputFolder);

  public string PygmentsAdapterPythonBinary { get; } = localConfiguration.PygmentsAdapterPythonBinary;
}
