// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Model;
using PSLoom.Reed.Runtime;
using PSLoom.Reed.Serialization;
using PSLoom.Warp;

namespace PSLoom.Reed.Cmdlets;

/// <summary>
///   Writes a completer to a JSON file another session can import. Inline sources cannot travel: by default the export is
///   refused, naming every slot that holds one, and <c>-Force</c> writes the file without them.
/// </summary>
[Cmdlet(VerbsData.Export, "Completer", DefaultParameterSetName = COMMAND_SET)]
[OutputType(typeof(void), typeof(FileInfo))]
public sealed class ExportCompleterCmdlet : PSCmdlet {
  private const string COMMAND_SET = "Command";
  private const string COMPLETER_SET = "Completer";

  /// <summary>
  ///   Gets or sets the registered command to export.
  /// </summary>
  [Parameter(Mandatory = true, Position = 0, ParameterSetName = COMMAND_SET)]
  [ValidateNotNullOrEmpty]
  public string Command { get; set; } = null!;

  /// <summary>
  ///   Gets or sets the completer to export, usually from <c>New-Completer</c>.
  /// </summary>
  [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = COMPLETER_SET)]
  [ValidateNotNull]
  public CompleterDefinition Completer { get; set; } = null!;

  /// <summary>
  ///   Gets or sets the file to write.
  /// </summary>
  [Parameter(Mandatory = true, Position = 1)]
  [ValidateNotNullOrEmpty]
  public string Path { get; set; } = null!;

  /// <summary>
  ///   Gets or sets a value indicating whether to export without the sources that cannot travel, and to overwrite the file.
  /// </summary>
  [Parameter]
  public SwitchParameter Force { get; set; }

  /// <summary>
  ///   Gets or sets a value indicating whether to write the file to the pipeline.
  /// </summary>
  [Parameter]
  public SwitchParameter PassThru { get; set; }

  /// <inheritdoc />
  protected override void ProcessRecord() {
    try {
      var definition = Resolve();
      var inline = ModelSources.WithPaths(definition)
        .Where(entry => entry.Source.Kind == CompletionSourceKind.ScriptBlock || entry.Source.CacheKey is not null)
        .Select(entry => entry.Path)
        .ToArray();

      if (inline.Length > 0 &&
          !Force.IsPresent) {
        WriteError(ReedException.CompleterNotPortable(definition.Command, inline).ToErrorRecord());
        return;
      }

      foreach (var path in inline) {
        WriteWarning($"{definition.Command}: the inline source at '{path}' was dropped; the exported slot completes nothing.");
      }

      var file = GetUnresolvedProviderPathFromPSPath(Path);
      File.WriteAllText(file, CompleterJson.Write(definition));
      WriteVerbose($"Exported the completer for '{definition.Command}' to '{file}'.");

      if (PassThru.IsPresent) {
        WriteObject(new FileInfo(file));
      }
    }
    catch (PowerShellException exception) {
      WriteError(exception.ToErrorRecord());
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException) {
      WriteError(new ErrorRecord(exception, ReedException.COMPLETER_NOT_PORTABLE, ErrorCategory.WriteError, Path));
    }
  }

  private CompleterDefinition Resolve() {
    if (ParameterSetName == COMPLETER_SET) {
      return Completer;
    }

    return ReedSession.ForCurrent().Completers.TryGet(Command, out var registration)
      ? registration.Definition
      : throw ReedException.CompleterNotFound(Command);
  }
}
