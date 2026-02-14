// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Globalization;
using Microsoft.Extensions.Logging;
using YetAnotherBlogGenerator.Items;
using YetAnotherBlogGenerator.Utilities;

namespace YetAnotherBlogGenerator.Meta;

internal class MetaParser : IMetaParser {
  private readonly ILogger<MetaParser> _logger;
  private readonly IMetaExtractor[] _extractors;
  private readonly IMetaExtractor _fallbackExtractor;

  public MetaParser(ILogger<MetaParser> logger, IEnumerable<IMetaExtractor> extractors) {
    _logger = logger;

    var sortedExtractors = extractors
        .OrderBy(e => e.Priority)
        .ToArray();
    _fallbackExtractor = sortedExtractors.Last();
    _extractors = sortedExtractors.SkipLast(1).ToArray();
  }

  public MetaParseResult Parse(string itemFullSource, string itemPath, ItemType itemType) {
    Dictionary<string, object> sourceMeta = [];
    Dictionary<string, object> fallbackMeta =
        UseCaseInsensitiveComparer(
            _fallbackExtractor.ExtractMeta(itemFullSource, itemPath, itemType)
        )
        ?? throw new Exception("Fallback extractor did not return any metadata."
        );
    string contentSource = string.Empty;

    foreach (var extractor in _extractors) {
      if (!extractor.SupportsItemType(itemType)) continue;
      Dictionary<string, object>? metaCandidate;
      try {
        metaCandidate = UseCaseInsensitiveComparer(extractor.ExtractMeta(itemFullSource, itemPath, itemType));
      } catch (Exception exc) {
        _logger.LogError(exc, "Extractor {Name} failed to parse {Path}: {Error}", extractor.Name, itemPath, exc);
        continue;
      }

      if (metaCandidate != null) {
        sourceMeta = metaCandidate;
        contentSource = extractor.ExtractContentSource(itemFullSource, itemPath);
        break;
      }
    }

    if (sourceMeta.Count == 0 || string.IsNullOrWhiteSpace(contentSource)) {
      contentSource = _fallbackExtractor.ExtractContentSource(itemFullSource, itemPath);
    }

    var metaReader = new FallbackDictionary<string, object>(sourceMeta, fallbackMeta);

    var itemMeta = new ItemMeta(
        Title: metaReader.GetString(MetaFields.Title) ?? throw new Exception($"Title is empty for {itemPath}."),
        Published: ParseDate(metaReader.GetValue(MetaFields.Published)) ??
                   throw new Exception($"Published date is empty for {itemPath}."),
        Updated: ParseDate(metaReader.GetValueOrDefault(MetaFields.Updated)),
        Tags: ParseTags(metaReader.GetValueOrDefault(MetaFields.Tags)),
        Category: metaReader.GetString(MetaFields.Category),
        Description: metaReader.GetString(MetaFields.Description),
        Thumbnail: metaReader.GetString(MetaFields.Thumbnail),
        Template: metaReader.GetString(MetaFields.Template),
        Comments: ParseBoolean(metaReader.GetValueOrDefault(MetaFields.Comments)) ?? true,
        ShortLink: metaReader.GetString(MetaFields.ShortLink),
        Guide: ParseBoolean(metaReader.GetValueOrDefault(MetaFields.Guide)) == true ? ParseGuideMeta(metaReader) : null,
        Project: itemType == ItemType.Project ? ParseProjectMeta(metaReader) : null,
        PublishedDateInSource: sourceMeta.ContainsKey(MetaFields.Published),
        Legacy: ParseBoolean(metaReader.GetValueOrDefault(MetaFields.Legacy)) ?? false,
        HtmlTitle: metaReader.GetString(MetaFields.HtmlTitle),
        CustomFields: sourceMeta
    );

    return new MetaParseResult(itemMeta, contentSource);
  }

  private static int? ParseNumber(object? rawValue) => rawValue switch {
      null => null,
      int i => i,
      string s => int.Parse(s, CultureInfo.InvariantCulture),
      _ => throw new ArgumentOutOfRangeException($"Number {rawValue} is of an unknown format.")
  };

  private static bool? ParseBoolean(object? rawValue) => rawValue switch {
      null => null,
      bool b => b,
      "true" => true,
      "false" => false,
      _ => throw new ArgumentOutOfRangeException($"Boolean {rawValue} is of an unknown format.")
  };

  private static DateTimeOffset? ParseDate(object? rawDate) => rawDate switch {
      null => null,
      DateTimeOffset dto => dto,
      DateTime dt => dt,
      string s when string.IsNullOrWhiteSpace(s) => null,
      string s => DateTimeOffset.Parse(s),
      _ => throw new ArgumentOutOfRangeException($"Date {rawDate} is of an unknown format.")
  };

  private static string[] ParseTags(object? rawValue) {
    var value = rawValue switch {
        null => [],
        IEnumerable<string> es => es.ToArray(),
        IEnumerable<object> eo => eo.Select(o => o is string s ? s : o.ToString()!).ToArray(),
        string s => [s],
        _ => throw new ArgumentOutOfRangeException($"Unexpected tags value: {rawValue}")
    };

    value.Sort(StringComparer.CurrentCultureIgnoreCase);
    return value;
  }

  private static GuideMeta ParseGuideMeta(FallbackDictionary<string, object> metaReader) => new(
      Effect: metaReader.GetString(MetaFields.GuideEffect)!,
      Platform: metaReader.GetString(MetaFields.GuidePlatform)!,
      Topic: metaReader.GetString(MetaFields.GuideTopic)!
  );

  private static int ParseRequiredNumber(FallbackDictionary<string, object> metaReader, string key)
    => ParseNumber(metaReader.GetValueOrDefault(key)) ?? throw new Exception($"Missing {key}");

  private static ProjectMeta ParseProjectMeta(FallbackDictionary<string, object> metaReader) => new(
      Sort: ParseRequiredNumber(metaReader, "sort"),
      DevStatus: ParseRequiredNumber(metaReader, "devstatus"),
      Download: metaReader.GetString("download"),
      GitHub: metaReader.GetString("github"),
      BugTracker: metaReader.GetString("bugtracker"),
      Role: metaReader.GetString("role"),
      License: metaReader.GetString("license"),
      Language: metaReader.GetString("language"),
      Logo: metaReader.GetString("logo"),
      Featured: ParseBoolean(metaReader.GetValueOrDefault("featured")) ?? false
  );

  private static Dictionary<string, object>? UseCaseInsensitiveComparer(Dictionary<string, object>? d) {
    return d == null ? null : new Dictionary<string, object>(d, StringComparer.InvariantCultureIgnoreCase);
  }
}
