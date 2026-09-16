// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Runtime;
using PSLoom.Warp;

namespace PSLoom.Reed.Cmdlets;

/// <summary>
///   Lists the completion providers registered in this session.
/// </summary>
[Cmdlet(VerbsCommon.Get, "CompletionProvider")]
[OutputType(typeof(CompletionProvider))]
public sealed class GetCompletionProviderCmdlet : PSCmdlet {
  /// <summary>
  ///   Gets or sets the names to list; wildcards are accepted.
  /// </summary>
  [Parameter(Position = 0)]
  [SupportsWildcards]
  public string[]? Name { get; set; }

  /// <inheritdoc />
  protected override void ProcessRecord() {
    try {
      var providers = ReedSession.ForCurrent().Providers;

      if (Name is not { Length: > 0 }) {
        foreach (var provider in providers.All.OrderBy(provider => provider.Name, StringComparer.OrdinalIgnoreCase)) {
          WriteObject(provider);
        }

        return;
      }

      foreach (var pattern in Name) {
        if (!WildcardPattern.ContainsWildcardCharacters(pattern)) {
          if (providers.TryGet(pattern, out var provider)) {
            WriteObject(provider);
          }

          continue;
        }

        var wildcard = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);

        foreach (var provider in providers.All.Where(provider => wildcard.IsMatch(provider.Name))
                   .OrderBy(provider => provider.Name, StringComparer.OrdinalIgnoreCase)) {
          WriteObject(provider);
        }
      }
    }
    catch (PowerShellException exception) {
      WriteError(exception.ToErrorRecord());
    }
  }
}
