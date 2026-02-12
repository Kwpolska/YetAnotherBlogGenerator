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
  private static readonly ManualResetEvent ResetEvent = new ManualResetEvent(false);
  private static bool _watching = true;
  private static ILogger<Program>? _logger;
  private static string? _outputFolder;
  static async Task<int> Main(string[] args) {
    CultureInfo.CurrentCulture = new CultureInfo("en-GB");

    var verbose = args.Contains("-v") || args.Contains("--verbose");
    var watch = args.Contains("--watch");
    var sourceRoot = Environment.CurrentDirectory;

    var services = new ServiceCollection()
        .AddYabgServices()
        .AddSingleton<ISourceRootProvider>(new SourceRootProvider(sourceRoot))
        .AddLogging(loggingBuilder =>
            loggingBuilder
                .SetMinimumLevel(verbose ? LogLevel.Trace : LogLevel.Information)
                .AddConsole(options => {
                  options.FormatterName = nameof(YabgConsoleLogFormatter);
                })
                .AddConsoleFormatter<YabgConsoleLogFormatter, ConsoleFormatterOptions>())
        .BuildServiceProvider();

    await using var _ = services.ConfigureAwait(false);
    _logger = services.GetRequiredService<ILogger<Program>>();
    _logger.LogInformation(Constants.CoreLog, "Welcome to YABG");

    var runResult = await RunOnce(services).ConfigureAwait(false);

    if (!watch || runResult > 0) {
      return runResult;
    }

    SetOutputFolder(services);


    var watchers = new List<FileSystemWatcher>();
    
    var rootWatcher = new FileSystemWatcher(sourceRoot);
    rootWatcher.IncludeSubdirectories = false;
    watchers.Add(rootWatcher);

    foreach (var dir in Directory.GetDirectories(sourceRoot)) {
      if (dir == _outputFolder) {
        continue;
      }

      var watcher = new FileSystemWatcher(dir);
      watcher.IncludeSubdirectories = true;
      watchers.Add(watcher);
    }

    foreach (var watcher in watchers) {
      watcher.NotifyFilter = NotifyFilters.Attributes
                             | NotifyFilters.CreationTime
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
                             | NotifyFilters.LastAccess
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Security
                             | NotifyFilters.Size;

      watcher.Changed += OnChanged;
      watcher.Created += OnChanged;
      watcher.Deleted += OnChanged;
      watcher.Renamed += OnChanged;
      watcher.Error += OnError;

      watcher.EnableRaisingEvents = true;
    }

    _logger.LogInformation(Constants.CoreLog, "Watching for file changes");
    while (_watching) {
      ResetEvent.WaitOne();
      if (!_watching) return 2;
      await Task.Delay(50).ConfigureAwait(false); // avoid multiple rebuilds for changes happening one after another
      ResetEvent.Reset();
      runResult = await RunOnce(services).ConfigureAwait(false);
    }
    
    return runResult;
  }


  private static void SetOutputFolder(ServiceProvider services) {
    using var scope = services.CreateScope();
    _outputFolder = scope.ServiceProvider.GetRequiredService<IConfiguration>().OutputFolder;
  }

  private static void OnChanged(object sender, FileSystemEventArgs e) {
    if (e.FullPath.Contains(Constants.CacheFileName)) {
      return;
    }
    
    _logger?.LogTrace(Constants.CoreLog, "Detected change in file {FileName}", e.FullPath);
    ResetEvent.Set();
  }
  
  private static void OnError(object sender, ErrorEventArgs e) {
    _logger!.LogCritical(Constants.CoreLog, e.GetException(), "File watcher failed");
    _watching = false;
    ResetEvent.Set();
  }

  private static async Task<int> RunOnce(ServiceProvider services) {
    var scope = services.CreateAsyncScope();
    await using var _ = scope.ConfigureAwait(false);
    try {
      var mainEngine = scope.ServiceProvider.GetRequiredService<MainEngine>();
      await mainEngine.Run().ConfigureAwait(false);
      return 0;
    } catch (Exception exc) {
      _logger!.LogCritical(Constants.CoreLog, exc, "An unhandled exception occurred");
      return 1;
    }
  }
}
