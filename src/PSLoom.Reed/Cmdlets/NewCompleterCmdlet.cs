// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Dsl;
using PSLoom.Reed.Model;
using PSLoom.Warp;
using PSLoom.Warp.Hosting;

namespace PSLoom.Reed.Cmdlets;

/// <summary>
///   Builds a completer from the same declaration body <c>Sley</c> takes, validates it, and writes it out for
///   <c>Register-Completer</c>. This is the path a module author uses, with no draft involved.
/// </summary>
[Cmdlet(VerbsCommon.New, "Completer")]
[OutputType(typeof(CompleterDefinition))]
public sealed class NewCompleterCmdlet : PSCmdlet {
  /// <summary>
  ///   Gets or sets the native command to complete.
  /// </summary>
  [Parameter(Mandatory = true, Position = 0)]
  [ValidateNotNullOrEmpty]
  public string Command { get; set; } = null!;

  /// <summary>
  ///   Gets or sets the declaration body.
  /// </summary>
  [Parameter(Mandatory = true, Position = 1)]
  [ValidateNotNull]
  public ScriptBlock ScriptBlock { get; set; } = null!;

  /// <summary>
  ///   Gets or sets the other names the command is known by.
  /// </summary>
  [Parameter]
  public string[] Alias { get; set; } = [];

  /// <summary>
  ///   Gets or sets the description shown for the command itself.
  /// </summary>
  [Parameter]
  public string? Description { get; set; }

  /// <inheritdoc />
  protected override void ProcessRecord() {
    try {
      var definition = new CompleterDefinition(Command, Alias, Description);
      var errors = HarnessHost.Current<ReedHarness>().Dsl.Run(new CompleterFrame(definition), ScriptBlock);

      if (errors.Count > 0) {
        foreach (var error in errors) {
          WriteError(error);
        }

        return;
      }

      // Building and checking are separate steps on this path: Test-Completer reports, Register-Completer refuses. Sley, which
      // has no pipeline to hand the completer to, does all three at once.
      WriteObject(definition);
    }
    catch (PowerShellException exception) {
      WriteError(exception.ToErrorRecord());
    }
  }
}
