// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Runtime;
using PSLoom.Warp;

namespace PSLoom.Reed.Cmdlets;

/// <summary>
///   Removes completion providers. A completer still naming one keeps working; that slot simply offers nothing.
/// </summary>
[Cmdlet(VerbsLifecycle.Unregister, "CompletionProvider")]
[OutputType(typeof(void))]
public sealed class UnregisterCompletionProviderCmdlet : PSCmdlet {
  /// <summary>
  ///   Gets or sets the provider names to remove.
  /// </summary>
  [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
  [ValidateNotNullOrEmpty]
  public string[] Name { get; set; } = null!;

  /// <inheritdoc />
  protected override void ProcessRecord() {
    try {
      var providers = ReedSession.ForCurrent().Providers;

      foreach (var name in Name) {
        if (providers.Remove(name) is null) {
          WriteWarning($"No completion provider named '{name}' is registered in this session.");
          continue;
        }

        WriteVerbose($"Removed completion provider '{name}'.");
      }
    }
    catch (PowerShellException exception) {
      WriteError(exception.ToErrorRecord());
    }
  }
}
