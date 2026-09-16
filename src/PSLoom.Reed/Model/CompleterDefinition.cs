// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace PSLoom.Reed.Model;

/// <summary>
///   A declared completer: the native command it completes, the names it answers to, and the tree of subcommands, options and
///   arguments underneath. This is the shape the DSL builds and the registry compiles; it is mutable only while it is being built.
/// </summary>
public sealed class CompleterDefinition {
  internal CompleterDefinition(string command, IReadOnlyList<string> aliases, string? description) {
    Command = command;
    Aliases = aliases;
    Description = description;
    Root = new CommandNode(command, aliases, description);
  }

  /// <summary>
  ///   Gets the native command this completer completes.
  /// </summary>
  public string Command { get; }

  /// <summary>
  ///   Gets the other names the command is known by; each one is wired to the same completer.
  /// </summary>
  public IReadOnlyList<string> Aliases { get; }

  /// <summary>
  ///   Gets the description shown when the command itself is offered.
  /// </summary>
  public string? Description { get; }

  /// <summary>
  ///   Gets the root node: the subcommands, options and arguments valid right after the command name.
  /// </summary>
  public CommandNode Root { get; }

  /// <summary>
  ///   Gets every name this completer answers to, the command first.
  /// </summary>
  public IEnumerable<string> Names => [Command, .. Aliases];

  /// <summary>
  ///   Gets the option groups declared at the root, by name. They are templates for <c>Use OptionGroup</c>, never completed
  ///   themselves.
  /// </summary>
  internal Dictionary<string, List<OptionNode>> OptionGroups { get; } = new(StringComparer.OrdinalIgnoreCase);

  /// <inheritdoc />
  public override string ToString()
    => Aliases.Count == 0 ? Command : $"{Command} ({string.Join(", ", Aliases)})";
}
