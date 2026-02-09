// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Text;

namespace YetAnotherBlogGenerator.Groups;

public class DirectoryTreeDirectory : DirectoryTreeEntry {
  public override string Url { get; }

  public DirectoryTreeDirectory(string[] path, string basePath)
      : base(path, isItem: false) {
    var builder = new StringBuilder(basePath);
    if (JoinedPath != "") {
      builder.Append('/');
      builder.Append(JoinedPath);
    }

    if (!IsItem) {
      builder.Append('/');
    }

    Url = builder.ToString();
  }
}
