// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Model;
using PSLoom.Reed.Serialization;
using PSLoom.Warp;

namespace PSLoom.Reed.Cmdlets;

/// <summary>
///   Reads a completer from a JSON file and writes it to the pipeline. It registers nothing, so the usual line is
///   <c>Import-Completer … | Register-Completer</c>, the same separation <c>New-Completer</c> has.
/// </summary>
[Cmdlet(VerbsData.Import, "Completer")]
[OutputType(typeof(CompleterDefinition))]
public sealed class ImportCompleterCmdlet : PSCmdlet {
  /// <summary>
  ///   Gets or sets the file to read.
  /// </summary>
  [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
  [Alias("FullName")]
  [ValidateNotNullOrEmpty]
  public string Path { get; set; } = null!;

  /// <inheritdoc />
  protected override void ProcessRecord() {
    var file = GetUnresolvedProviderPathFromPSPath(Path);

    try {
      WriteObject(CompleterJson.Read(File.ReadAllText(file), file));
    }
    catch (PowerShellException exception) {
      WriteError(exception.ToErrorRecord());
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException) {
      WriteError(ReedException.CompleterNotReadable(file, exception.Message, exception).ToErrorRecord());
    }
  }
}
