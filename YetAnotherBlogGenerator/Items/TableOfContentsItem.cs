namespace YetAnotherBlogGenerator.Items;

public record TableOfContentsItem(string Anchor, string Title, TableOfContentsItem[] Children);
