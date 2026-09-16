// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Model;

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
    var path = new List<string> { completer.Command };
    var bound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var arguments = new List<string>();
    CompiledOption? expecting = null;

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
        path.Add(child.Name);
        arguments.Clear();
        continue;
      }

      arguments.Add(token);
    }

    return new CompletionContext(command, path, bound, arguments, expecting, Expected(command, expecting, arguments.Count));
  }

  private static ArgumentNode? Expected(CompiledCommand command, CompiledOption? expecting, int positional) {
    if (expecting is not null) {
      return expecting.Value;
    }

    if (positional < command.Arguments.Count) {
      return command.Arguments[positional];
    }

    // Past the declared slots only a variadic last one keeps accepting values.
    return command.Arguments is [.., { Variadic: true } last] ? last : null;
  }

  private static bool IsOption(string token)
    => token.Length > 1 && token[0] == '-';
}
