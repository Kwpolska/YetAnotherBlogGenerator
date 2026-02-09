// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using YetAnotherBlogGenerator.Groups;
using YetAnotherBlogGenerator.Items;

namespace YetAnotherBlogGenerator.Grouping;

internal class ProjectGrouper(IGroupFormatter groupFormatter) : IItemGrouper {
  public IEnumerable<IGroup> GroupItems(Item[] items) {
    var projects = items
        .Where(i => i.Type == ItemType.Project)
        .OrderBy(p => p.Meta.Project!.Sort)
        .ThenBy(p => p.Title);

    yield return groupFormatter.FormatHtmlListGroup(
        items: projects,
        title: "Projects",
        url: "/projects/",
        template: CommonTemplates.ProjectList);
  }
}
