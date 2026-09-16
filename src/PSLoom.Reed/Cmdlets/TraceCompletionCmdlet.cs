// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Runtime;
using PSLoom.Warp;

namespace PSLoom.Reed.Cmdlets;

/// <summary>
///   Shows the tab presses Reed served in this session, oldest first: every one, not just the failures.
/// </summary>
[Cmdlet(VerbsDiagnostic.Trace, "Completion")]
[OutputType(typeof(CompletionTrace))]
public sealed class TraceCompletionCmdlet : PSCmdlet {
  /// <summary>
  ///   Gets or sets how many of the most recent entries to show.
  /// </summary>
  [Parameter(Position = 0)]
  [ValidateRange(1, int.MaxValue)]
  public int? Last { get; set; }

  /// <summary>
  ///   Gets or sets a value indicating whether to forget every entry instead of showing them.
  /// </summary>
  [Parameter]
  public SwitchParameter Clear { get; set; }

  /// <inheritdoc />
  protected override void ProcessRecord() {
    try {
      if (ReedSession.ForCurrent().Traces is not { } traces) {
        WriteWarning("Reed has no trace log in this session; thread the harness before completing.");
        return;
      }

      if (Clear.IsPresent) {
        traces.Clear();
        return;
      }

      foreach (var entry in traces.Snapshot(Last)) {
        WriteObject(entry);
      }
    }
    catch (PowerShellException exception) {
      WriteError(exception.ToErrorRecord());
    }
  }
}
