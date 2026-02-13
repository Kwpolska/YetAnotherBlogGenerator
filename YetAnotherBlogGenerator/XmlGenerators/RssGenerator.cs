// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Xml.Linq;
using HtmlAgilityPack;
using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Output;
using YetAnotherBlogGenerator.Utilities;

namespace YetAnotherBlogGenerator.XmlGenerators;

internal class RssGenerator(IConfiguration configuration, IUrlHelper urlHelper)
    : IRssGenerator {
  private readonly Dictionary<string, string> _teaserCache = new();
  private readonly Dictionary<string, string> _contentCache = new();

  public WriteXmlTask GenerateRss(ItemRssGroup group) {
    XNamespace atom = "http://www.w3.org/2005/Atom";
    XNamespace content = "http://purl.org/rss/1.0/modules/content/";
    XNamespace dc = "http://purl.org/dc/elements/1.1/";

    var atomLink = new XElement(atom + "link");
    atomLink.SetAttributeValue("href", AbsoluteUrl(group.Url));
    atomLink.SetAttributeValue("rel", "self");
    atomLink.SetAttributeValue("type", "application/rss+xml");

    var channel = new XElement("channel",
        new XElement("title", group.Title),
        new XElement("link", configuration.SiteUri),
        atomLink,
        new XElement("description", configuration.SiteDescription),
        new XElement("lastBuildDate", FormatDate(group.Items.Max(i => i.Updated ?? i.Published))),
        new XElement("generator", "https://github.com/Kwpolska/YetAnotherBlogGenerator")
    );

    foreach (var item in group.Items) {
      var rssItem = new XElement("item",
          new XElement("title", item.Title),
          new XElement(dc + "creator", configuration.SiteAuthor),
          new XElement("link", AbsoluteUrl(item.Url)),
          new XElement("pubDate", FormatDate(item.Published)),
          new XElement("guid", AbsoluteUrl(item.Url)),
          new XElement("description", StripHtmlWithCache(item.Teaser, item.Url)),
          new XElement(content + "encoded",
              new XCData(SanitizeUrlsWithCache(item.Teaser + "\n\n" + item.Content, item.Url)))
      );

      if (item.Meta.Category != null) {
        rssItem.Add(new XElement("category", item.Meta.Category));
      }

      foreach (var tag in item.Meta.Tags) {
        rssItem.Add(new XElement("category", tag));
      }

      channel.Add(rssItem);
    }

    var root = new XElement("rss",
        new XAttribute("version", "2.0"),
        new XAttribute(XNamespace.Xmlns + nameof(atom), atom),
        new XAttribute(XNamespace.Xmlns + nameof(content), content),
        new XAttribute(XNamespace.Xmlns + nameof(dc), dc),
        channel
    );

    var document = new XDocument(root);
    return new WriteXmlTask(document, urlHelper.UrlToOutputPath(group.Url));
  }

  private string AbsoluteUrl(string url) {
    return new Uri(configuration.SiteUri, url).ToString();
  }

  private Uri AbsoluteUri(string url) {
    return new Uri(configuration.SiteUri, url);
  }

  private static string FormatDate(DateTimeOffset date) => date.ToString("r");

  private string StripHtmlWithCache(string html, string pageUrl) {
    if (_teaserCache.TryGetValue(pageUrl, out var cachedResult)) {
      return cachedResult;
    }

    var result = StripHtml(html);
    _teaserCache.Add(pageUrl, result);
    return result;
  }

  private static string StripHtml(string html) {
    var doc = new HtmlDocument();
    doc.LoadHtml(html);
    return doc.DocumentNode.InnerText;
  }

  private string SanitizeUrlsWithCache(string html, string pageUrl) {
    if (_contentCache.TryGetValue(pageUrl, out var cachedResult)) {
      return cachedResult;
    }

    var result = SanitizeUrls(html, pageUrl);
    _contentCache.Add(pageUrl, result);
    return result;
  }

  private string SanitizeUrls(string html, string pageUrl) {
    var doc = new HtmlDocument();
    var pageUri = AbsoluteUri(pageUrl);
    doc.LoadHtml(html);

    SanitizeUrlsCore(pageUri, doc, "//a", "href");
    SanitizeUrlsCore(pageUri, doc, "//img", "src");

    return doc.DocumentNode.InnerHtml;
  }

  private static void SanitizeUrlsCore(Uri pageUri, HtmlDocument doc, string xpath, string attribute) {
    var links = doc.DocumentNode.SelectNodes(xpath);

    // This can actually be null, HtmlAgilityPack has nullable reference types enabled, but not actually specified in the codebase
    if (links == null) return;

    foreach (var link in links) {
      var href = link.GetAttributeValue(attribute, "");
      if (href == "") {
        continue;
      }

      var linkUri = new Uri(pageUri, href);
      link.SetAttributeValue(attribute, linkUri.ToString());
    }
  }
}
