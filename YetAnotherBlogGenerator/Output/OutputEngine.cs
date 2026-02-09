// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using YetAnotherBlogGenerator.Utilities;

namespace YetAnotherBlogGenerator.Output;

public class OutputEngine(ILogger<OutputEngine> logger) : IOutputEngine {
  public ValueTask Execute(IOutputTask task) => task switch {
      CopyTask copyTask => ExecuteCore(copyTask),
      WriteBinaryTask writeBinaryTask => ExecuteCore(writeBinaryTask),
      WriteXmlTask writeXmlTask => ExecuteCore(writeXmlTask),
      _ => throw new ArgumentOutOfRangeException(nameof(task), task, null)
  };

  private ValueTask ExecuteCore(CopyTask copyTask) {
    var directory = Path.GetDirectoryName(copyTask.Destination)!;
    Directory.CreateDirectory(directory);
    logger.LogDebug(Constants.CopyLog, "{Source} -> {Destination}", copyTask.Source, copyTask.Destination);
    File.Copy(copyTask.Source, copyTask.Destination, overwrite: true);
    return ValueTask.CompletedTask;
  }

  private async ValueTask ExecuteCore(WriteBinaryTask writeTask) {
    var directory = Path.GetDirectoryName(writeTask.Destination)!;
    Directory.CreateDirectory(directory);
    logger.LogDebug(Constants.WriteLog, "{Destination}", writeTask.Destination);
    await File.WriteAllBytesAsync(writeTask.Destination, writeTask.Content).ConfigureAwait(false);
  }

  private async ValueTask ExecuteCore(WriteXmlTask writeTask) {
    var directory = Path.GetDirectoryName(writeTask.Destination)!;
    Directory.CreateDirectory(directory);
    logger.LogDebug(Constants.WriteLog, "{Destination}", writeTask.Destination);
    var fs = new FileStream(writeTask.Destination, FileMode.Create);
    await using var _ = fs.ConfigureAwait(false);

    var writerSettings = new XmlWriterSettings {
        Async = true,
        Encoding = Encoding.UTF8NoBom,
        NewLineChars = "\n",
#if DEBUG
        Indent = true
#else
        Indent = false
#endif
    };

    var writer = XmlWriter.Create(fs, writerSettings);
    await using (writer.ConfigureAwait(false)) {
      await writeTask.Document.WriteToAsync(writer, CancellationToken.None).ConfigureAwait(false);
      await writer.FlushAsync().ConfigureAwait(false);
    }
  }
}
