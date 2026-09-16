// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Runtime;
using PSLoom.Warp;

namespace PSLoom.Reed.Cmdlets;

/// <summary>
///   Removes a completer from this session. Completion for its names then finds nothing and offers nothing; the native
///   registration itself stays, because PowerShell has no way to remove one from a live session.
/// </summary>
[Cmdlet(VerbsLifecycle.Unregister, "Completer")]
[OutputType(typeof(void))]
public sealed class UnregisterCompleterCmdlet : PSCmdlet {
  /// <summary>
  ///   Gets or sets the command names to remove; an alias removes the whole completer it belongs to.
  /// </summary>
  [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
  [ValidateNotNullOrEmpty]
  public string[] Command { get; set; } = null!;

  /// <inheritdoc />
  protected override void ProcessRecord() {
    try {
      var registry = ReedSession.ForCurrent().Completers;

      foreach (var name in Command) {
        if (registry.Remove(name) is not { } removed) {
          WriteWarning($"No completer is registered for '{name}' in this session.");
          continue;
        }

        WriteVerbose($"Removed the completer for '{removed.Command}' ({string.Join(", ", removed.Names)}).");
      }
    }
    catch (PowerShellException exception) {
      WriteError(exception.ToErrorRecord());
    }
  }
}
