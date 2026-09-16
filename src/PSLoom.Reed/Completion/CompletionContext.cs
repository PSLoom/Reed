// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Model;

namespace PSLoom.Reed.Completion;

/// <summary>
///   Where the cursor is, in the completer's terms: which command is active, what is already bound, and what the next word is.
/// </summary>
internal sealed class CompletionContext {
  public CompletionContext(CompiledCommand command, IReadOnlyList<string> commandPath, IReadOnlySet<string> boundOptions,
    IReadOnlyList<string> boundArguments, CompiledOption? expectingValueFor, ArgumentNode? expectedArgument) {
    Command = command;
    CommandPath = commandPath;
    BoundOptions = boundOptions;
    BoundArguments = boundArguments;
    ExpectingValueFor = expectingValueFor;
    ExpectedArgument = expectedArgument;
  }

  /// <summary>
  ///   Gets the innermost command the typed tokens resolved to.
  /// </summary>
  public CompiledCommand Command { get; }

  /// <summary>
  ///   Gets the command and the subcommands walked to reach it, as typed by the declaration.
  /// </summary>
  public IReadOnlyList<string> CommandPath { get; }

  /// <summary>
  ///   Gets the spellings of the options already on the line.
  /// </summary>
  public IReadOnlySet<string> BoundOptions { get; }

  /// <summary>
  ///   Gets the positional values already given to <see cref="Command" />.
  /// </summary>
  public IReadOnlyList<string> BoundArguments { get; }

  /// <summary>
  ///   Gets the option whose value the next word is, when the previous token was an option expecting one.
  /// </summary>
  public CompiledOption? ExpectingValueFor { get; }

  /// <summary>
  ///   Gets the slot the next word fills: an option's value, or the command's positional at this point. It is
  ///   <see langword="null" /> when the declaration has nothing for this position.
  /// </summary>
  public ArgumentNode? ExpectedArgument { get; }

  /// <summary>
  ///   Gets how many positional arguments were already given to <see cref="Command" />.
  /// </summary>
  public int Positional => BoundArguments.Count;
}
