// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Model;
using PSLoom.Reed.Runtime;
using PSLoom.Warp;

namespace PSLoom.Reed.Cmdlets;

/// <summary>
///   Registers a completer in this session and wires every name it answers to into tab completion.
/// </summary>
[Cmdlet(VerbsLifecycle.Register, "Completer")]
[OutputType(typeof(CompleterRegistration))]
public sealed class RegisterCompleterCmdlet : PSCmdlet {
  /// <summary>
  ///   Gets or sets the completer, usually from <c>New-Completer</c>.
  /// </summary>
  [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
  [ValidateNotNull]
  public CompleterDefinition Completer { get; set; } = null!;

  /// <summary>
  ///   Gets or sets a value indicating whether to replace whatever completer already holds one of these names.
  /// </summary>
  [Parameter]
  public SwitchParameter Force { get; set; }

  /// <summary>
  ///   Gets or sets a value indicating whether to write the registration to the pipeline.
  /// </summary>
  [Parameter]
  public SwitchParameter PassThru { get; set; }

  /// <inheritdoc />
  protected override void ProcessRecord() {
    try {
      if (!CompleterGate.Approve(this, Completer)) {
        return;
      }

      var registration = ReedEngine.Register(ReedEngine.Of(this), Completer, Force.IsPresent);
      WriteVerbose($"Registered the completer for '{registration.Command}' ({string.Join(", ", registration.Names)}).");

      if (PassThru.IsPresent) {
        WriteObject(registration);
      }
    }
    catch (PowerShellException exception) {
      WriteError(exception.ToErrorRecord());
    }
  }
}
