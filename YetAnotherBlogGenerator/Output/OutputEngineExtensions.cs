// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

namespace YetAnotherBlogGenerator.Output;

public static class OutputEngineExtensions {
  public static async Task ExecuteMany(
      this IOutputEngine outputEngine,
      IEnumerable<IOutputTask> tasks) {
    await Parallel.ForEachAsync(
        tasks,
        new ParallelOptions() { MaxDegreeOfParallelism = 10 },
        async (task, _) => await outputEngine.Execute(task).ConfigureAwait(false)).ConfigureAwait(false);
  }
}
