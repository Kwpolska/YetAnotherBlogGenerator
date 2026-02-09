// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Xml.Linq;
using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;
using YetAnotherBlogGenerator.Output;
using YetAnotherBlogGenerator.Utilities;

namespace YetAnotherBlogGenerator.XmlGenerators;

internal class SitemapGenerator(IConfiguration configuration, IUrlHelper urlHelper) : ISitemapGenerator {
  // <urlset xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
// xsi:schemaLocation="http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd"
// xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
  public WriteXmlTask GenerateSitemap(IReadOnlyCollection<Item> items, IReadOnlyCollection<IHtmlGroup> htmlGroups) {
    var sitemapItems = new List<SitemapItem>(items.Count + htmlGroups.Count);

    XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
    XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";

    sitemapItems.AddRange(items
        .Where(i => i.ScanPattern.IncludeInSitemap)
        .Select(i => new SitemapItem(
            uri: new Uri(configuration.SiteUri, i.Url),
            lastMod: i.Updated ?? i.Published
        )));

    sitemapItems.AddRange(htmlGroups.Select(g => new SitemapItem(
        uri: new Uri(configuration.SiteUri, g.Url),
        lastMod: g.LastUpdated,
        dateOnly: true
    )));

    var xmlItems = sitemapItems
        .OrderBy(i => i.Url)
        .Select(i => new XElement(
            xmlns + "url",
            new XElement(xmlns + "loc", i.Url),
            new XElement(xmlns + "lastmod",
                i.DateOnly
                    ? i.LastMod.ToString("yyyy-MM-dd")
                    : i.LastMod.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"))
        )).ToArray();


    var root = new XElement(xmlns + "urlset",
        new XAttribute("xmlns", xmlns),
        new XAttribute(XNamespace.Xmlns + nameof(xsi), xsi),
        new XAttribute(xsi + "schemaLocation",
            "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd"),
        xmlItems
    );

    var document = new XDocument(root);

    return new WriteXmlTask(document, urlHelper.UrlToOutputPath("/sitemap.xml"));
  }

  private class SitemapItem(Uri uri, DateTimeOffset lastMod, bool dateOnly = false) {
    public string Url { get; } = uri.ToString();
    public DateTimeOffset LastMod { get; } = lastMod;
    public bool DateOnly { get; } = dateOnly;
  }
}
