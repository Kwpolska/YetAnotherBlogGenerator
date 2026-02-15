using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Utilities;

public interface ITableOfContentsGenerator {
  TableOfContentsItem[] BuildTree(IEnumerable<Heading> headings);
  IReadOnlyCollection<TableOfContentsItem> ExtractTableOfContents(string html, bool isLegacy);
}
