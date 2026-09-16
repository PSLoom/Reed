// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Model;

namespace PSLoom.Reed.Completion;

/// <summary>
///   A completer compiled for the tab-press path.
/// </summary>
internal sealed class CompiledCompleter {
  private CompiledCompleter(string command, IReadOnlyList<string> names, CompiledCommand root) {
    Command = command;
    Names = names;
    Root = root;
  }

  public string Command { get; }

  /// <summary>
  ///   Gets every name wired to this completer, the command first.
  /// </summary>
  public IReadOnlyList<string> Names { get; }

  public CompiledCommand Root { get; }

  public static CompiledCompleter Compile(CompleterDefinition definition) {
    ArgumentNullException.ThrowIfNull(definition);
    return new CompiledCompleter(definition.Command, [.. definition.Names], CompiledCommand.Compile(definition.Root));
  }
}
