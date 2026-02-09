// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using Microsoft.Extensions.DependencyInjection;
using YetAnotherBlogGenerator.Cache;
using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Grouping;
using YetAnotherBlogGenerator.ItemRendering;
using YetAnotherBlogGenerator.ItemRendering.External;
using YetAnotherBlogGenerator.Meta;
using YetAnotherBlogGenerator.Output;
using YetAnotherBlogGenerator.Scanning;
using YetAnotherBlogGenerator.StaticFiles;
using YetAnotherBlogGenerator.TemplateRendering;
using YetAnotherBlogGenerator.Utilities;
using YetAnotherBlogGenerator.XmlGenerators;

namespace YetAnotherBlogGenerator;

internal static class ServiceConfiguration {
  public static IServiceCollection AddYabgServices(this IServiceCollection services) => services
      // Grouping
      .AddSingleton<IGroupEngine, GroupEngine>()
      .AddSingleton<IGroupFormatter, GroupFormatter>()
      .AddSingleton<IPostGrouper, ArchiveGrouper>()
      .AddSingleton<IPostGrouper, GuideGrouper>()
      .AddSingleton<IPostGrouper, IndexGrouper>()
      .AddSingleton<IPostGrouper, NavigationGrouper>()
      .AddSingleton<IPostGrouper, TagCategoryGrouper>()
      .AddSingleton<IItemGrouper, GalleryIndexGrouper>()
      .AddSingleton<IItemGrouper, ListingIndexGrouper>()
      .AddSingleton<IItemGrouper, ProjectGrouper>()
      // Meta
      .AddSingleton<IMetaExtractor, YamlMetaExtractor>()
      .AddSingleton<IMetaExtractor, FileSystemMetaExtractor>()
      .AddSingleton<IMetaParser, MetaParser>()
      // Listings
      .AddSingleton<PygmentsRenderer>()
      .AddSingleton<IListingRenderer, CachingPygmentsRenderer>()
      // Item Rendering
      .AddKeyedSingleton<IItemRenderer, GalleryItemRenderer>(GalleryItemRenderer.Name)
      .AddKeyedSingleton<IItemRenderer, HtmlItemRenderer>(HtmlItemRenderer.Name)
      .AddKeyedSingleton<IItemRenderer, ListingItemRenderer>(ListingItemRenderer.Name)
      .AddKeyedSingleton<IItemRenderer, MarkdownItemRenderer>(MarkdownItemRenderer.Name)
      .AddSingleton<IItemRenderEngine, ItemRenderEngine>()
      .AddSingleton<IRenderDispatcher, RenderDispatcher>()
      // Output
      .AddSingleton<IOutputEngine, OutputEngine>()
      // Scanning
      .AddSingleton<IItemScanner, FileSystemItemScanner>()
      // Static files
      .AddSingleton<IAssetBundleEngine, AssetBundleEngine>()
      .AddSingleton<IStaticFileEngine, StaticFileEngine>()
      .AddSingleton<IThumbnailEngine, ThumbnailEngine>()
      // Templating
      .AddSingleton<ITemplateEngine, TemplateEngine>()
      // XML
      .AddSingleton<IRssGenerator, RssGenerator>()
      .AddSingleton<ISitemapGenerator, SitemapGenerator>()
      // General
      .AddSingleton<ICacheService, CacheService>()
      .AddSingleton<IConfigurationReader, ConfigurationReader>()
      .AddSingleton<IConfiguration>(s => s.GetRequiredService<IConfigurationReader>().Read())
      .AddSingleton<IUrlHelper, UrlHelper>()
      .AddSingleton<MainEngine>()
      .AddSingleton<StartTimeProvider>()
      .AddSingleton(TimeProvider.System);
}
