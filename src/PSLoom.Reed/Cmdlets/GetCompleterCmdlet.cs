// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Runtime;
using PSLoom.Warp;

namespace PSLoom.Reed.Cmdlets;

/// <summary>
///   Lists the completers registered in this session, or those matching the names given.
/// </summary>
[Cmdlet(VerbsCommon.Get, "Completer")]
[OutputType(typeof(CompleterRegistration))]
public sealed class GetCompleterCmdlet : PSCmdlet {
  /// <summary>
  ///   Gets or sets the command names to list; wildcards are accepted, and an alias finds its completer.
  /// </summary>
  [Parameter(Position = 0)]
  [SupportsWildcards]
  public string[]? Command { get; set; }

  /// <inheritdoc />
  protected override void ProcessRecord() {
    try {
      var registry = ReedSession.ForCurrent().Completers;

      if (Command is not { Length: > 0 }) {
        foreach (var registration in registry.All) {
          WriteObject(registration);
        }

        return;
      }

      foreach (var pattern in Command) {
        if (!WildcardPattern.ContainsWildcardCharacters(pattern)) {
          if (registry.TryGet(pattern, out var registration)) {
            WriteObject(registration);
          }

          continue;
        }

        var wildcard = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);

        foreach (var registration in registry.All.Where(registration => registration.Names.Any(wildcard.IsMatch))) {
          WriteObject(registration);
        }
      }
    }
    catch (PowerShellException exception) {
      WriteError(exception.ToErrorRecord());
    }
  }
}
