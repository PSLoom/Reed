// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Model;
using PSLoom.Warp.Dsl;

namespace PSLoom.Reed.Dsl;

/// <summary>
///   The completer being declared.
/// </summary>
public sealed class CompleterFrame(CompleterDefinition definition) : IDslFrame<CompleterScope> {
  /// <summary>
  ///   Gets the completer.
  /// </summary>
  public CompleterDefinition Definition { get; } = definition;
}

/// <summary>
///   The subcommand being declared.
/// </summary>
public sealed class CommandFrame(CommandNode node) : IDslFrame<CommandScope> {
  /// <summary>
  ///   Gets the subcommand.
  /// </summary>
  public CommandNode Node { get; } = node;
}

/// <summary>
///   The option being declared.
/// </summary>
public sealed class OptionFrame(OptionNode option) : IDslFrame<OptionScope> {
  /// <summary>
  ///   Gets the option.
  /// </summary>
  public OptionNode Option { get; } = option;
}

/// <summary>
///   The option group being declared.
/// </summary>
public sealed class OptionGroupFrame(string name) : IDslFrame<OptionGroupScope> {
  /// <summary>
  ///   Gets the group name.
  /// </summary>
  public string Name { get; } = name;

  /// <summary>
  ///   Gets the options collected so far.
  /// </summary>
  public List<OptionNode> Options { get; } = [];
}
