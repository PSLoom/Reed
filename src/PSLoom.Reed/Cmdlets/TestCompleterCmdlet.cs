// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Model;
using PSLoom.Warp;

namespace PSLoom.Reed.Cmdlets;

/// <summary>
///   Validates a completer without registering it, reporting every problem found.
/// </summary>
[Cmdlet(VerbsDiagnostic.Test, "Completer")]
[OutputType(typeof(CompleterProblem), typeof(bool))]
public sealed class TestCompleterCmdlet : PSCmdlet {
  /// <summary>
  ///   Gets or sets the completer to validate.
  /// </summary>
  [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
  [ValidateNotNull]
  public CompleterDefinition Completer { get; set; } = null!;

  /// <summary>
  ///   Gets or sets a value indicating whether to write a single boolean instead of the problems.
  /// </summary>
  [Parameter]
  public SwitchParameter Quiet { get; set; }

  /// <inheritdoc />
  protected override void ProcessRecord() {
    try {
      var problems = CompleterValidator.Validate(Completer);

      if (Quiet.IsPresent) {
        WriteObject(!problems.Any(problem => problem.IsError));
        return;
      }

      foreach (var problem in problems) {
        WriteObject(problem);
      }
    }
    catch (PowerShellException exception) {
      WriteError(exception.ToErrorRecord());
    }
  }
}
