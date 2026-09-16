// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace PSLoom.Reed.Completion;

/// <summary>
///   Walks the typed tokens against a compiled completer to find where the cursor is.
/// </summary>
internal static class ContextResolver {
  /// <summary>
  ///   Resolves the completion context.
  /// </summary>
  /// <param name="completer">The compiled completer.</param>
  /// <param name="tokens">The tokens already typed, without the command name and the word in progress.</param>
  public static CompletionContext Resolve(CompiledCompleter completer, IReadOnlyList<string> tokens) {
    ArgumentNullException.ThrowIfNull(completer);
    ArgumentNullException.ThrowIfNull(tokens);

    var command = completer.Root;
    var bound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    CompiledOption? expecting = null;
    var positional = 0;

    foreach (var token in tokens) {
      if (expecting is not null) {
        expecting = null;
        continue;
      }

      if (IsOption(token)) {
        // '--name=value' carries its own value, so it never leaves an option expecting one.
        var separator = token.IndexOf('=', StringComparison.Ordinal);
        var name = separator < 0 ? token : token[..separator];

        bound.Add(name);

        if (separator < 0 &&
            command.Options.TryGetValue(name, out var option) &&
            option.TakesValue) {
          expecting = option;
        }

        continue;
      }

      if (command.Commands.TryGetValue(token, out var child)) {
        command = child;
        positional = 0;
        continue;
      }

      positional++;
    }

    return new CompletionContext(command, bound, expecting, positional);
  }

  private static bool IsOption(string token)
    => token.Length > 1 && token[0] == '-';
}
