// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NikolaPostConverter;

internal static partial class Program {
  private static readonly Regex MetaLineRegex = MetaLineRegexSource();
  private static readonly Regex SlugLineRegex = SlugLineRegexSource();
  private static readonly HashSet<string> AllMetaFields = [];
  private static readonly JsonSerializerOptions JsonOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

  private static string? ConvertMetaLine(string originalLine) {
    var trimmedLine = originalLine.Trim();
    var match = MetaLineRegex.Match(trimmedLine);
    if (!match.Success) {
      if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine == "<!--" || trimmedLine == "-->") {
        return null;
      }

      Console.WriteLine($"WARN: unrecognized meta line '{originalLine}'");
      return null;
    }

    var key = match.Groups[1].Value;
    var value = match.Groups[2].Value;
    var valueJson = JsonSerializer.Serialize(value, JsonOptions);

    if (string.IsNullOrWhiteSpace(value)) return null;
    AllMetaFields.Add(key);

    switch (key) {
      case "type":
        // skip - pointless
        return null;
      case "slug":
        // skip - pointless
        return null;
      case "date":
        // rename to published
        key = "published";
        break;
      case "previewimage":
        // rename to thumbnail
        key = "thumbnail";
        break;
      case "tags":
        // convert to YAML list
        valueJson = JsonSerializer.Serialize(value.Split(',').Select(v => v.Trim()).ToList(), JsonOptions)
            .Replace("\",\"", "\", \"");
        break;
      case "devstatus":
      case "sort":
        // numbers
        valueJson = value;
        break;
      case "featured":
        if (value.Equals("false", StringComparison.InvariantCultureIgnoreCase)) return null;
        valueJson = "true";
        break;
      case "status":
        if (value.Equals("featured", StringComparison.InvariantCultureIgnoreCase)) return "featured: true";
        break;
      case "nocommments":
        key = "comments";
        valueJson = "false";
        break;
    }

    return $"{key}: {valueJson}";
  }

  private static string? ExtractSlug(string originalMeta) {
    var match = SlugLineRegex.Match(originalMeta);
    if (match.Success == false) return null;
    return match.Groups[1].Value;
  }

  static void Main() {
    var sourceDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "website");
    var cacheDirectory = Path.Combine(sourceDirectory, "cache");
    var targetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "website2");
    string[] rawDirectories = ["pages", "projects"];
    string[] cachedDirectories = ["posts"];
    string[] allDirectories = [.. rawDirectories, .. cachedDirectories];

    foreach (var directory in allDirectories) {
      var useCacheForDirectory = cachedDirectories.Contains(directory);
      var fullDirectory = Path.Combine(sourceDirectory, directory);
      var files = Directory.GetFiles(fullDirectory, "*.*", SearchOption.AllDirectories);
      foreach (var file in files) {
        if (file.EndsWith(".pl.rst")) {
          // skip translations
          continue;
        }

        var useCache = useCacheForDirectory && !file.EndsWith(".md");
        var relativePath = Path.GetRelativePath(sourceDirectory, file);
        var relativeDirectory = Path.GetDirectoryName(relativePath)!;
        Console.WriteLine($"Processing {relativePath} {useCache}");
        var metaSource = File.ReadAllText(file).ReplaceLineEndings("\n");
        var metaSplit = metaSource.Split("\n\n", 2,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var oldMeta = metaSplit[0];
        string content;

        if (useCache) {
          var contentPath = Path.Combine(cacheDirectory, relativePath);
          contentPath = Path.Combine(
              Path.GetDirectoryName(contentPath)!,
              Path.GetFileNameWithoutExtension(contentPath) + ".html");
          content = File.ReadAllText(contentPath).ReplaceLineEndings("\n");
        } else {
          content = metaSplit[1];
        }

        content = content.Trim('\n');

        var newFileLines = oldMeta.Split("\n")
            .Select(ConvertMetaLine)
            .Prepend("---")
            .Append(useCache ? "legacy: true" : null)
            .Append("---")
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Append(content)
            .Append("") // trailing new line
            .ToArray();

        var newFile = string.Join('\n', newFileLines);

        var slug = ExtractSlug(oldMeta) ?? Path.GetFileNameWithoutExtension(relativePath);
        var extension =
            (!useCache && (Path.GetExtension(relativePath) == ".rst" || Path.GetExtension(relativePath) == ".md"))
                ? ".md"
                : ".html";

        var fileTargetDirectory = Path.Combine(targetDirectory, relativeDirectory);
        Directory.CreateDirectory(fileTargetDirectory);
        var targetPath = Path.Combine(fileTargetDirectory, slug + extension);
        File.WriteAllText(targetPath, newFile, Encoding.UTF8);
      }
    }

    Console.WriteLine($"All meta fields: {string.Join(", ", AllMetaFields)}");
  }

  [GeneratedRegex(@"\s*\.\. ?([A-Za-z0-9_]+):\s*(.*)$")]
  private static partial Regex MetaLineRegexSource();

  [GeneratedRegex(@"\s*\.\. ?slug:\s*(.*)$")]
  private static partial Regex SlugLineRegexSource();
}
