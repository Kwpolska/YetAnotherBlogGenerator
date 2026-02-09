// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

namespace YetAnotherBlogGenerator;

public interface IRenderable {
  string Title { get; }

  string Url { get; }

  string TemplateName { get; }
}
