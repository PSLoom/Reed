// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Runtime;
using PSLoom.Warp;

namespace PSLoom.Reed.Cmdlets;

/// <summary>
///   Registers a named source, which any <c>Argument -Provider &lt;name&gt;</c> resolves at tab time. The script block receives
///   the word being completed, the command path, the bound options and the positional arguments already typed.
/// </summary>
[Cmdlet(VerbsLifecycle.Register, "CompletionProvider")]
[OutputType(typeof(CompletionProvider))]
public sealed class RegisterCompletionProviderCmdlet : PSCmdlet {
  /// <summary>
  ///   Gets or sets the provider name.
  /// </summary>
  [Parameter(Mandatory = true, Position = 0)]
  [ValidateNotNullOrEmpty]
  public string Name { get; set; } = null!;

  /// <summary>
  ///   Gets or sets the script block producing the candidates.
  /// </summary>
  [Parameter(Mandatory = true, Position = 1)]
  [ValidateNotNull]
  public ScriptBlock ScriptBlock { get; set; } = null!;

  /// <summary>
  ///   Gets or sets what the provider completes, shown by <c>Get-CompletionProvider</c>.
  /// </summary>
  [Parameter]
  public string? Description { get; set; }

  /// <summary>
  ///   Gets or sets a value indicating whether to replace a provider already registered under this name.
  /// </summary>
  [Parameter]
  public SwitchParameter Force { get; set; }

  /// <summary>
  ///   Gets or sets a value indicating whether to write the provider to the pipeline.
  /// </summary>
  [Parameter]
  public SwitchParameter PassThru { get; set; }

  /// <inheritdoc />
  protected override void ProcessRecord() {
    try {
      var provider = ReedSession.ForCurrent().Providers.Add(Name, ScriptBlock, Description, Force.IsPresent);
      WriteVerbose($"Registered completion provider '{provider.Name}'.");

      if (PassThru.IsPresent) {
        WriteObject(provider);
      }
    }
    catch (PowerShellException exception) {
      WriteError(exception.ToErrorRecord());
    }
  }
}
