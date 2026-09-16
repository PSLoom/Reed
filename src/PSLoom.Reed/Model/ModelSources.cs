// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace PSLoom.Reed.Model;

/// <summary>
///   Walks a completer for the sources it declares, wherever they sit.
/// </summary>
internal static class ModelSources {
  /// <summary>
  ///   Every source a completer declares, in declaration order.
  /// </summary>
  public static IEnumerable<CompletionSource> Of(CompleterDefinition definition) {
    ArgumentNullException.ThrowIfNull(definition);
    return Of(definition.Root);
  }

  /// <summary>
  ///   Every source a command declares, with its path, for messages that must name where a source sits.
  /// </summary>
  public static IEnumerable<(string Path, CompletionSource Source)> WithPaths(CompleterDefinition definition) {
    ArgumentNullException.ThrowIfNull(definition);
    return WithPaths(definition.Root, definition.Command);
  }

  private static IEnumerable<CompletionSource> Of(CommandNode node)
    => WithPaths(node, node.Name).Select(entry => entry.Source);

  private static IEnumerable<(string Path, CompletionSource Source)> WithPaths(CommandNode node, string path) {
    foreach (var argument in node.Arguments) {
      if (argument.Source is { } source) {
        yield return ($"{path}/{argument.Name}", source);
      }
    }

    foreach (var option in node.Options) {
      if (option.Value?.Source is { } source) {
        yield return ($"{path}/{option.Name}", source);
      }
    }

    foreach (var command in node.Commands) {
      foreach (var entry in WithPaths(command, $"{path}/{command.Name}")) {
        yield return entry;
      }
    }
  }
}
