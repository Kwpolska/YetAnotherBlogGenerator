// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using Fluid;
using Fluid.Values;
using Fluid.ViewEngine;
using Microsoft.Extensions.FileProviders;
using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;
using YetAnotherBlogGenerator.Meta;
using YetAnotherBlogGenerator.StaticFiles;
using YetAnotherBlogGenerator.TemplateRendering.Models;
using YetAnotherBlogGenerator.Utilities;

namespace YetAnotherBlogGenerator.TemplateRendering;

internal class TemplateEngine : ITemplateEngine {
  private readonly FluidViewRenderer _renderer;
  private readonly TemplateOptions _templateOptions;
  private readonly ICacheBustingService _cacheBustingService;
  private readonly IConfiguration _configuration;

  public TemplateEngine(ICacheBustingService cacheBustingService, IConfiguration configuration) {
    var options = new FluidViewEngineOptions();
    options.Parser = new FluidViewParser(new FluidParserOptions() { AllowFunctions = true });
    var templateFolder = Path.Join(configuration.SourceRoot, CommonFolders.Templates);
    options.ViewsFileProvider = new PhysicalFileProvider(templateFolder);
    options.PartialsFileProvider = new PhysicalFileProvider(templateFolder);
    options.TemplateOptions.FileProvider = new PhysicalFileProvider(templateFolder);
    options.TemplateOptions.MemberAccessStrategy = new UnsafeMemberAccessStrategy();
    options.TemplateOptions.MemberAccessStrategy.Register<DirectoryTreeDirectory>();
    options.TemplateOptions.MemberAccessStrategy.Register<DirectoryTreeEntry>();
    options.TemplateOptions.MemberAccessStrategy.Register<DirectoryTreeGroup>();
    options.TemplateOptions.MemberAccessStrategy.Register<DirectoryTreeGroupModel>();
    options.TemplateOptions.MemberAccessStrategy.Register<DirectoryTreeItem>();
    options.TemplateOptions.MemberAccessStrategy.Register<GalleryData>();
    options.TemplateOptions.MemberAccessStrategy.Register<GalleryImage>();
    options.TemplateOptions.MemberAccessStrategy.Register<GuideMeta>();
    options.TemplateOptions.MemberAccessStrategy.Register<Item>();
    options.TemplateOptions.MemberAccessStrategy.Register<ItemGroupModel>();
    options.TemplateOptions.MemberAccessStrategy.Register<ItemHtmlGroup>();
    options.TemplateOptions.MemberAccessStrategy.Register<ItemMeta>();
    options.TemplateOptions.MemberAccessStrategy.Register<ItemModel>();
    options.TemplateOptions.MemberAccessStrategy.Register<Link>();
    options.TemplateOptions.MemberAccessStrategy.Register<LinkGroup>();
    options.TemplateOptions.MemberAccessStrategy.Register<LinkGroupItem>();
    options.TemplateOptions.MemberAccessStrategy.Register<LinkGroupModel>();
    options.TemplateOptions.MemberAccessStrategy.Register<MenuItem>();
    options.TemplateOptions.MemberAccessStrategy.Register<NavigationGroup>();
    options.TemplateOptions.MemberAccessStrategy.Register<NavigationGroupItem>();
    options.TemplateOptions.MemberAccessStrategy.Register<ProjectMeta>();
    options.TemplateOptions.MemberAccessStrategy.IgnoreCasing = true;
    options.TemplateOptions.Filters.AddFilter("link", GetLink);
    options.TemplateOptions.Filters.AddFilter("categoryColor", GetCategoryColor);
    options.TemplateOptions.Filters.AddFilter("listingSourceUrl", GetListingSourceUrl);
    options.TemplateOptions.Filters.AddFilter("projectDevStatus", GetDevStatus);
    options.TemplateOptions.Filters.AddFilter("cacheBusting", GetCacheBustedUrl);
    _renderer = new FluidViewRenderer(options);
    _templateOptions = options.TemplateOptions;
    _cacheBustingService = cacheBustingService;
    _configuration = configuration;
  }

  private static ValueTask<FluidValue> GetCategoryColor(FluidValue input, FilterArguments arguments,
      TemplateContext context) =>
      new StringValue(
          (context.Model.ToObjectValue() as IModelBase)
          ?.CategoryColors.GetValueOrDefault(input.ToStringValue())
          ?? "red");

  private static ValueTask<FluidValue> GetDevStatus(FluidValue input, FilterArguments arguments,
      TemplateContext context) => input.ToNumberValue() switch {
      1 => DevStatusBadge("default", "Planning"),
      2 => DevStatusBadge("danger", "Pre-Alpha"),
      3 => DevStatusBadge("warning", "Alpha"),
      4 => DevStatusBadge("info", "Beta"),
      5 => DevStatusBadge("success", "Production/Stable"),
      6 => DevStatusBadge("success", "Mature"),
      7 => DevStatusBadge("danger", "Inactive"),
      _ => DevStatusBadge("default", "Unknown")
  };

  private ValueTask<FluidValue> GetCacheBustedUrl(FluidValue input, FilterArguments arguments,
      TemplateContext context) {
    var url = input.ToStringValue();
    return new StringValue(_cacheBustingService.Get(url));
  }

  private static ValueTask<FluidValue> DevStatusBadge(string className, string label)
    => new StringValue($"""<span class="badge rounded-pill text-bg-{className}">{label}</span>""", encode: false);

  private static ValueTask<FluidValue> GetListingSourceUrl(FluidValue input, FilterArguments arguments,
      TemplateContext context) {
    var item = (Item)(input.ToObjectValue());
    var basePath = item.Url[..item.Url.LastIndexOf('.')];
    return new StringValue(basePath);
  }

  private static ValueTask<FluidValue> GetLink(FluidValue input, FilterArguments arguments, TemplateContext context) {
    var model = context.Model.ToObjectValue() as IModelBase;

    var value = input.ToStringValue();
    var result = (arguments.At(0).ToStringValue()) switch {
        "tag" => $"/blog/tags/{value.Slugify(model?.CategoryTagSlugs)}",
        "category" => $"/blog/tags/cat_{value.Slugify(model?.CategoryTagSlugs)}",
        var unknown => throw new Exception($"Unknown link type {unknown}")
    };
    return new StringValue(result);
  }

  public async Task RenderItem(Item item, NavigationGroup navigationGroup, TextWriter outputStream) {
    var templateName =
        (string)item.Meta.CustomFields.GetValueOrDefault(MetaFields.Template, item.ScanPattern.TemplateName);

    var templateContext = new TemplateContext(
        new ItemModel(item, navigationGroup, _configuration),
        _templateOptions
    );

    await _renderer.RenderViewAsync(outputStream, templateName, templateContext).ConfigureAwait(false);
  }

  public async Task RenderGroup(IHtmlGroup group, TextWriter outputStream) {
    object model = group switch {
        ItemHtmlGroup itemHtmlGroup => new ItemGroupModel(itemHtmlGroup, _configuration),
        LinkGroup linkGroup => new LinkGroupModel(linkGroup, _configuration),
        DirectoryTreeGroup directoryTreeGroup => new DirectoryTreeGroupModel(directoryTreeGroup, _configuration),
        _ => throw new ArgumentOutOfRangeException(nameof(group))
    };

    var templateContext = new TemplateContext(model, _templateOptions);

    await _renderer.RenderViewAsync(outputStream, group.TemplateName, templateContext).ConfigureAwait(false);
  }
}
