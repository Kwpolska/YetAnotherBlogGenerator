// YetAnotherBlogGenerator
// Copyright © 2025-2026, Chris Warrick. All rights reserved.
// Licensed under the 3-clause BSD license.

using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using YetAnotherBlogGenerator.Config;

namespace YetAnotherBlogGenerator.ItemRendering.External;

internal class PygmentsRenderer(IConfiguration configuration) : IListingRenderer {
  private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web) {
      Encoder = JavaScriptEncoder.Default, DefaultIgnoreCondition = JsonIgnoreCondition.Never
  };

  public async Task<string> RenderSingleListing(string code, string? path, string? language) {
    var request = new ExternalRenderRequest(Guid: Guid.NewGuid(), Path: path, Source: code, Language: language);
    var responses = await RenderMultipleListings([request]).ConfigureAwait(false);
    return responses.Single().Html;
  }

  public async Task<List<ExternalRenderResponse>> RenderMultipleListings(IEnumerable<ExternalRenderRequest> requests) {
    var requestsJson = JsonSerializer.Serialize(requests, JsonSerializerOptions);
    var requestsJsonMemory = new ReadOnlyMemory<char>(requestsJson.ToCharArray());

    var startInfo =
        new ProcessStartInfo(configuration.PygmentsAdapterPythonBinary, ["-m", "yabg_pygments_adapter"]) {
            RedirectStandardInput = true, RedirectStandardOutput = true
        };

    var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start Pygments renderer");
    string stdout;

    try {
      var cancellationTokenSource = new CancellationTokenSource();
      cancellationTokenSource.CancelAfter(7500);
      var cancellationToken = cancellationTokenSource.Token;
      await process.StandardInput.WriteAsync(requestsJsonMemory, cancellationToken).ConfigureAwait(false);
      await process.StandardInput.FlushAsync(cancellationToken).ConfigureAwait(false);
      process.StandardInput.Close();
      stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
      await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
    } catch (Exception exc) {
      process.Kill();
      throw new Exception($"Pygments render failed to interact: {exc}", exc);
    }

    if (process.ExitCode != 0) {
      throw new Exception(
          $"Rendering listings with Pygments failed with exit code {process.ExitCode}. Output: {stdout}");
    }

    var responses = JsonSerializer.Deserialize<List<ExternalRenderResponse>>(stdout, JsonSerializerOptions)
                    ?? throw new Exception($"Failed to parse Pygments response: {stdout}");
    var errors = responses.Where(r => !r.Success).Select(r => $"[{r.Path}] {r.Html}").ToList();

    if (errors.Count != 0) {
      throw new Exception("Failed to render listings with Pygments.\n" + string.Join('\n', errors));
    }

    return responses;
  }
}
