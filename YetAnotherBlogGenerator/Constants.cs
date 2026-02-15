// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace YetAnotherBlogGenerator;

internal static partial class Constants {
  public static readonly Regex TeaserRegex = TeaserRegexSource();

  public const string CacheFileName = ".yabg-cache.sqlite3";
  public const string SiteConfigFileName = "yabg-site.yml";
  public const string LocalConfigFileName = "yabg-local.yml";
  public const char UrlPathSeparator = '/';
  public const string IndexHtml = "index.html";
  public const string CoreCacheSource = "Core";

  public const string BundleLogEventType = nameof(BundleLogEventType);
  public const string CopyLogEventType = nameof(CopyLogEventType);
  public const string CoreLogEventType = nameof(CoreLogEventType);
  public const string WriteLogEventType = nameof(WriteLogEventType);
  public const string RenderLogEventType = nameof(RenderLogEventType);

  public static readonly EventId BundleLog = new(0, BundleLogEventType);
  public static readonly EventId CopyLog = new(0, CopyLogEventType);
  public static readonly EventId CoreLog = new(0, CoreLogEventType);
  public static readonly EventId RenderLog = new(0, RenderLogEventType);
  public static readonly EventId WriteLog = new(0, WriteLogEventType);

  public const uint MaxThumbnailSize = 300;

  public static readonly HashSet<string> SystemLogTypes =
      [CopyLogEventType, CoreLogEventType, RenderLogEventType, WriteLogEventType];

  public static readonly HashSet<string> OutputLogTypes =
      [CopyLogEventType, RenderLogEventType, WriteLogEventType];

  [GeneratedRegex(@"<!--\s*TEASER_END\s*-->")]
  private static partial Regex TeaserRegexSource();
}
