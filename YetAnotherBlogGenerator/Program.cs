// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using YetAnotherBlogGenerator.Config;
using YetAnotherBlogGenerator.Logging;

namespace YetAnotherBlogGenerator;

internal class Program {
  static async Task<int> Main(string[] args) {
    CultureInfo.CurrentCulture = new CultureInfo("en-GB");

    var verbose = args.Contains("-v") || args.Contains("--verbose");

    var services = new ServiceCollection()
        .AddYabgServices()
        .AddSingleton<ISourceRootProvider>(new SourceRootProvider(Environment.CurrentDirectory))
        .AddLogging(loggingBuilder =>
            loggingBuilder
                .SetMinimumLevel(verbose ? LogLevel.Trace : LogLevel.Information)
                .AddConsole(options => {
                  options.FormatterName = nameof(YabgConsoleLogFormatter);
                })
                .AddConsoleFormatter<YabgConsoleLogFormatter, ConsoleFormatterOptions>())
        .BuildServiceProvider();

    await using var _ = services.ConfigureAwait(false);
    var logger = services.GetRequiredService<ILogger<Program>>();

    try {
      var mainEngine = services.GetRequiredService<MainEngine>();
      await mainEngine.Run().ConfigureAwait(false);
      return 0;
    } catch (Exception exc) {
      logger.LogCritical(Constants.CoreLog, exc, "An unhandled exception occurred");
      return 1;
    }
  }
}
