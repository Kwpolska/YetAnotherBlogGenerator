// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Runtime.CompilerServices;
using System.Web;
using System.Xml.XPath;
using HtmlAgilityPack;
using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Utilities;

public class TableOfContentsGenerator : ITableOfContentsGenerator {
  private const string InnerHeadingXPath = "*[self::h1 or self::h2 or self::h3 or self::h4 or self::h5 or self::h6]";
  private const string GlobalHeadingXPath = $"//{InnerHeadingXPath}";

  public IReadOnlyCollection<TableOfContentsItem> ExtractTableOfContents(string html, bool isLegacy) {
    var doc = new HtmlDocument();
    doc.LoadHtml(html);

    IEnumerable<Heading> headings;

    if (isLegacy) {
      headings = doc.DocumentNode
          .SelectNodes2("//section")
          .Select(section => {
            var h = section.SelectSingleNode(InnerHeadingXPath);
            return new Heading(
                Anchor: section.Id,
                Title: HttpUtility.HtmlEncode(h.InnerText),
                Level: GetLevel(h.Name));
          });
    } else {
      headings = doc.DocumentNode
          .SelectNodes2(GlobalHeadingXPath)
          .Select(h => new Heading(
              Anchor: h.Id,
              Title: HttpUtility.HtmlEncode(h.InnerText),
              Level: GetLevel(h.Name)));
    }

    return BuildTree(headings);
  }

  public TableOfContentsItem[] BuildTree(IEnumerable<Heading> headings) {
    const int minLevel = 2; // h1 will never be children of other headers
    var groups = new LinkedList<HeadingWithChildren>();
    var nextLevel = -1;

    foreach (var h in headings) {
      groups.AddLast(new HeadingWithChildren(h));
      if (h.Level > nextLevel) nextLevel = h.Level;
    }

    // Is this algorithm the most optimal? Probably not, but it is O(n), visiting each heading up to 5 times
    // (but generally less, since that would require all h1-h6 heading levels to be in the document).

    while (nextLevel >= minLevel) {
      var level = nextLevel;
      nextLevel = -1;
      var node = groups.First;
      while (node != null) {
        var nodeLevel = node.Value.Level;
        if (nodeLevel > nextLevel && nodeLevel < level) {
          nextLevel = nodeLevel;
        }

        if (node.Value.Level != level) {
          node = node.Next;
          continue;
        }

        var previousNode = node.Previous;
        var nextNode = node.Next;
        if (previousNode == null || previousNode.Value.Level >= level) {
          node = node.Next;
          continue;
        }

        previousNode.Value.Children.AddLast(node.Value);
        groups.Remove(node);
        node = nextNode;
      }
    }

    return groups.Select(HeadingToItem).ToArray();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int GetLevel(string h) => (h[1] - 0x30);

  private static TableOfContentsItem HeadingToItem(HeadingWithChildren headingWithChildren) {
    return new TableOfContentsItem(
        Anchor: headingWithChildren.Heading.Anchor,
        Title: headingWithChildren.Heading.Title,
        Children: headingWithChildren.Children.Select(HeadingToItem).ToArray());
  }

  private class HeadingWithChildren(Heading heading) {
    public Heading Heading { get; } = heading;
    public int Level => Heading.Level;
    public LinkedList<HeadingWithChildren> Children { get; } = [];
  }
}
