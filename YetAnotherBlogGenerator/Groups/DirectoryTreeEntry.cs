// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

namespace YetAnotherBlogGenerator.Groups;

public abstract class DirectoryTreeEntry(string[] path, bool isItem) {
  public bool IsItem { get; } = isItem;
  public string[] Path { get; } = path;
  public string JoinedPath { get; } = string.Join('/', path);
  public string LastPart { get; } = path.Length == 0 ? "" : path[^1];

  public abstract string Url { get; }

  protected bool Equals(DirectoryTreeEntry other) {
    return JoinedPath == other.JoinedPath;
  }

  public override bool Equals(object? obj) {
    if (obj is null) {
      return false;
    }

    if (ReferenceEquals(this, obj)) {
      return true;
    }

    if (obj.GetType() != GetType()) {
      return false;
    }

    return Equals((DirectoryTreeEntry)obj);
  }

  public override int GetHashCode() {
    return JoinedPath.GetHashCode();
  }

  public override string ToString() => JoinedPath;
}
