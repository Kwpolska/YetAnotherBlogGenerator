// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

// This code is adapted from Microsoft.Extensions.Logging.Console
// https://github.com/dotnet/runtime/blob/dee607911e68b92ea8fede4dd735d90c9132a3ed/src/libraries/Microsoft.Extensions.Logging.Console/src/AnsiParser.cs
// https://github.com/dotnet/runtime/blob/dee607911e68b92ea8fede4dd735d90c9132a3ed/src/libraries/Microsoft.Extensions.Logging.Console/src/TextWriterExtensions.cs
// Original code copyright (c) .NET Foundation and Contributors, licensed under the MIT license

namespace YetAnotherBlogGenerator.Logging;

internal static class TextWriterExtensions {
  public static void WriteColoredMessage(this TextWriter textWriter, string message, ConsoleColor? background,
      ConsoleColor? foreground) {
    // Order: backgroundcolor, foregroundcolor, Message, reset foregroundcolor, reset backgroundcolor
    if (background.HasValue) {
      textWriter.Write(GetBackgroundColorEscapeCode(background.Value));
    }

    if (foreground.HasValue) {
      textWriter.Write(GetForegroundColorEscapeCode(foreground.Value));
    }

    textWriter.Write(message);

    if (foreground.HasValue) {
      textWriter.Write(DefaultForegroundColor); // reset to default foreground color
    }

    if (background.HasValue) {
      textWriter.Write(DefaultBackgroundColor); // reset to the background color
    }
  }

  private const string DefaultForegroundColor = "\e[39m\e[22m"; // reset to default foreground color
  private const string DefaultBackgroundColor = "\e[49m"; // reset to the background color

  private static string GetForegroundColorEscapeCode(ConsoleColor color) => color switch {
      ConsoleColor.Black => "\e[30m",
      ConsoleColor.DarkRed => "\e[31m",
      ConsoleColor.DarkGreen => "\e[32m",
      ConsoleColor.DarkYellow => "\e[33m",
      ConsoleColor.DarkBlue => "\e[34m",
      ConsoleColor.DarkMagenta => "\e[35m",
      ConsoleColor.DarkCyan => "\e[36m",
      ConsoleColor.Gray => "\e[37m",
      ConsoleColor.Red => "\e[1m\e[31m",
      ConsoleColor.Green => "\e[1m\e[32m",
      ConsoleColor.Yellow => "\e[1m\e[33m",
      ConsoleColor.Blue => "\e[1m\e[34m",
      ConsoleColor.Magenta => "\e[1m\e[35m",
      ConsoleColor.Cyan => "\e[1m\e[36m",
      ConsoleColor.White => "\e[1m\e[37m",
      _ => DefaultForegroundColor // default foreground color
  };

  private static string GetBackgroundColorEscapeCode(ConsoleColor color) => color switch {
      ConsoleColor.Black => "\e[40m",
      ConsoleColor.DarkRed => "\e[41m",
      ConsoleColor.DarkGreen => "\e[42m",
      ConsoleColor.DarkYellow => "\e[43m",
      ConsoleColor.DarkBlue => "\e[44m",
      ConsoleColor.DarkMagenta => "\e[45m",
      ConsoleColor.DarkCyan => "\e[46m",
      ConsoleColor.Gray => "\e[47m",
      _ => DefaultBackgroundColor // Use default background color
  };
}
