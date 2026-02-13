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
      .AddScoped<IGroupEngine, GroupEngine>()
      .AddScoped<IGroupFormatter, GroupFormatter>()
      .AddScoped<IPostGrouper, ArchiveGrouper>()
      .AddScoped<IPostGrouper, GuideGrouper>()
      .AddScoped<IPostGrouper, IndexGrouper>()
      .AddScoped<IPostGrouper, NavigationGrouper>()
      .AddScoped<IPostGrouper, TagCategoryGrouper>()
      .AddScoped<IItemGrouper, GalleryIndexGrouper>()
      .AddScoped<IItemGrouper, ListingIndexGrouper>()
      .AddScoped<IItemGrouper, ProjectGrouper>()
      // Meta
      .AddScoped<IMetaExtractor, YamlMetaExtractor>()
      .AddScoped<IMetaExtractor, FileSystemMetaExtractor>()
      .AddScoped<IMetaParser, MetaParser>()
      // Listings
      .AddScoped<PygmentsRenderer>()
      .AddScoped<IListingRenderer, CachingPygmentsRenderer>()
      // Item Rendering
      .AddKeyedScoped<IItemRenderer, GalleryItemRenderer>(GalleryItemRenderer.Name)
      .AddKeyedScoped<IItemRenderer, HtmlItemRenderer>(HtmlItemRenderer.Name)
      .AddKeyedScoped<IItemRenderer, ListingItemRenderer>(ListingItemRenderer.Name)
      .AddKeyedScoped<IItemRenderer, MarkdownItemRenderer>(MarkdownItemRenderer.Name)
      .AddScoped<IItemRenderEngine, ItemRenderEngine>()
      .AddScoped<IRenderDispatcher, RenderDispatcher>()
      // Output
      .AddScoped<IOutputEngine, OutputEngine>()
      // Scanning
      .AddScoped<IItemScanner, FileSystemItemScanner>()
      // Static files
      .AddScoped<IAssetBundleEngine, AssetBundleEngine>()
      .AddScoped<ICacheBustingService, CacheBustingService>()
      .AddScoped<IStaticFileEngine, StaticFileEngine>()
      .AddScoped<IThumbnailEngine, ThumbnailEngine>()
      // Templating
      .AddScoped<ITemplateEngine, TemplateEngine>()
      // XML
      .AddScoped<IRssGenerator, RssGenerator>()
      .AddScoped<ISitemapGenerator, SitemapGenerator>()
      // General
      .AddScoped<ICacheService, CacheService>()
      .AddScoped<IConfigurationReader, ConfigurationReader>()
      .AddScoped<IConfiguration>(s => s.GetRequiredService<IConfigurationReader>().Read())
      .AddScoped<IUrlHelper, UrlHelper>()
      .AddScoped<MainEngine>()
      .AddScoped<StartTimeProvider>()
      .AddSingleton(TimeProvider.System);
}
