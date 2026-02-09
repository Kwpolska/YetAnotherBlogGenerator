// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

namespace YetAnotherBlogGenerator.Utilities;

public class StartTimeProvider {
  public DateTimeOffset StartTime = _StartTime;

  public static DateTimeOffset _StartTime { get; private set; }

  public StartTimeProvider(TimeProvider provider) {
    _StartTime = provider.GetUtcNow();
  }
}
