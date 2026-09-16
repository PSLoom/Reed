// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace PSLoom.Reed.Model;

/// <summary>
///   A command or subcommand: what may follow it on the command line.
/// </summary>
public sealed class CommandNode {
  internal CommandNode(string name, IReadOnlyList<string> aliases, string? description) {
    Name = name;
    Aliases = aliases;
    Description = description;
  }

  /// <summary>
  ///   Gets the subcommand name.
  /// </summary>
  public string Name { get; }

  /// <summary>
  ///   Gets the alternative names it answers to.
  /// </summary>
  public IReadOnlyList<string> Aliases { get; }

  /// <summary>
  ///   Gets the description shown beside the candidate.
  /// </summary>
  public string? Description { get; }

  /// <summary>
  ///   Gets the subcommands declared under this one.
  /// </summary>
  public List<CommandNode> Commands { get; } = [];

  /// <summary>
  ///   Gets the options valid at this level.
  /// </summary>
  public List<OptionNode> Options { get; } = [];

  /// <summary>
  ///   Gets the positional arguments of this command, in order.
  /// </summary>
  public List<ArgumentNode> Arguments { get; } = [];

  /// <inheritdoc />
  public override string ToString()
    => Name;
}
