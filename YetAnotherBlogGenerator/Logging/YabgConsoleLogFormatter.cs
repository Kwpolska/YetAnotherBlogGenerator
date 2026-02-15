// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

// This code is adapted from Microsoft.Extensions.Logging.Console
// https://github.com/dotnet/runtime/blob/dee607911e68b92ea8fede4dd735d90c9132a3ed/src/libraries/Microsoft.Extensions.Logging.Console/src/SimpleConsoleFormatter.cs
// Original code copyright (c) .NET Foundation and Contributors, licensed under the MIT license

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace YetAnotherBlogGenerator.Logging;

internal sealed class YabgConsoleLogFormatter() : ConsoleFormatter(nameof(YabgConsoleLogFormatter)) {
  private static readonly string MultiLinePadding = new string(' ', 4);
  private static readonly string NewLineWithMessagePadding = Environment.NewLine + MultiLinePadding;

  private const int MaxLogLevelLength = 8; // CRITICAL
  private const int MaxScopeLength = 13; // [html groups]

  private const string TimestampFormat = "HH:mm:ss ";

  public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider,
      TextWriter textWriter) {
    if (logEntry.State is BufferedLogRecord bufferedRecord) {
      string message = bufferedRecord.FormattedMessage ?? string.Empty;
      WriteInternal(null, textWriter, message, bufferedRecord.LogLevel, bufferedRecord.EventId.Name,
          bufferedRecord.Exception, logEntry.Category, bufferedRecord.Timestamp);
    } else {
      string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
      if (logEntry.Exception == null && message == null) {
        return;
      }

      // We extract most of the work into a non-generic method to save code size. If this was left in the generic
      // method, we'd get generic specialization for all TState parameters, but that's unnecessary.
      WriteInternal(scopeProvider, textWriter, message, logEntry.LogLevel, logEntry.EventId.Name,
          logEntry.Exception?.ToString(), logEntry.Category, GetCurrentDateTime());
    }

    textWriter.Flush();
  }

  private void WriteInternal(IExternalScopeProvider? scopeProvider, TextWriter textWriter, string message,
      LogLevel logLevel, string? eventName, string? exception, string? category, DateTimeOffset stamp) {
    ConsoleColors logLevelColors = GetLogLevelConsoleColors(logLevel, eventName);
    string logLevelString = GetLogLevelString(logLevel, eventName);

    textWriter.Write(stamp.ToString(TimestampFormat));
    textWriter.WriteColoredMessage(logLevelString, logLevelColors.Background, logLevelColors.Foreground);
    textWriter.Write(new string(' ', MaxLogLevelLength - logLevelString.Length + 1));

    if (eventName == null || !Constants.SystemLogTypes.Contains(eventName)) {
      textWriter.Write(category);
    }

    // scope information
    WriteScopeInformation(textWriter, scopeProvider);
    WriteMessage(textWriter, message);

    // Example:
    // System.InvalidOperationException
    //    at Namespace.Class.Function() in File:line X
    if (exception != null) {
      // exception message
      textWriter.Write(MultiLinePadding);
      WriteMessage(textWriter, exception);
    }
  }

  private static void WriteMessage(TextWriter textWriter, string message) {
    if (string.IsNullOrEmpty(message)) {
      return;
    }

    string newMessage = message.Replace(Environment.NewLine, NewLineWithMessagePadding);
    textWriter.Write(newMessage);
    textWriter.Write(Environment.NewLine);
  }

  private DateTimeOffset GetCurrentDateTime() => DateTimeOffset.Now;

  private static string GetLogLevelString(LogLevel logLevel, string? eventName) => logLevel switch {
      LogLevel.Debug when eventName == Constants.CopyLogEventType => "COPY",
      LogLevel.Debug when eventName == Constants.WriteLogEventType => "WRITE",
      LogLevel.Debug when eventName == Constants.RenderLogEventType => "RENDER",
      LogLevel.Trace => "TRACE",
      LogLevel.Debug => "DEBUG",
      LogLevel.Information => "INFO",
      LogLevel.Warning => "WARNING",
      LogLevel.Error => "ERROR",
      LogLevel.Critical => "CRITICAL",
      LogLevel.None => "???",
      _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
  };

  private ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel, string? eventName) => logLevel switch {
      LogLevel.Debug when eventName != null && Constants.OutputLogTypes.Contains(eventName) => new ConsoleColors(
          ConsoleColor.DarkBlue, null),
      LogLevel.Trace or LogLevel.Debug => new ConsoleColors(ConsoleColor.Gray, null),
      LogLevel.Information => new ConsoleColors(ConsoleColor.DarkGreen, null),
      LogLevel.Warning => new ConsoleColors(ConsoleColor.Yellow, null),
      LogLevel.Error => new ConsoleColors(ConsoleColor.Red, null),
      LogLevel.Critical => new ConsoleColors(ConsoleColor.White, ConsoleColor.DarkRed),
      _ => new ConsoleColors(null, null)
  };

  private static void WriteScopeInformation(TextWriter textWriter, IExternalScopeProvider? scopeProvider) {
    var scopeNames = new List<string>(1);

    scopeProvider?.ForEachScope((scope, state) => {
      var s = scope?.ToString();
      if (s != null) {
        state.Add(s);
      }
    }, scopeNames);

    if (scopeNames.Count == 0) {
      textWriter.Write(new string(' ', MaxScopeLength + 1));
      return;
    }

    var scopeInfo = scopeNames.Count == 1 ? $"[{scopeNames[0]}]" : $"[{string.Join("|", scopeNames)}]";

    textWriter.WriteColoredMessage(scopeInfo, null, ConsoleColor.DarkCyan);
    textWriter.Write(new string(' ', Math.Max(1, MaxScopeLength - scopeInfo.Length + 1)));
  }

  private readonly struct ConsoleColors(ConsoleColor? foreground, ConsoleColor? background) {
    public ConsoleColor? Foreground { get; } = foreground;

    public ConsoleColor? Background { get; } = background;
  }
}
