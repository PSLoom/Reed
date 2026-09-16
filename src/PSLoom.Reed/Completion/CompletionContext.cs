// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace PSLoom.Reed.Completion;

/// <summary>
///   Where the cursor is, in the completer's terms: which command is active, what is already bound, and what the next word is.
/// </summary>
internal sealed class CompletionContext {
  public CompletionContext(CompiledCommand command, IReadOnlySet<string> boundOptions, CompiledOption? expectingValueFor, int positional) {
    Command = command;
    BoundOptions = boundOptions;
    ExpectingValueFor = expectingValueFor;
    Positional = positional;
  }

  /// <summary>
  ///   Gets the innermost command the typed tokens resolved to.
  /// </summary>
  public CompiledCommand Command { get; }

  /// <summary>
  ///   Gets the spellings of the options already on the line.
  /// </summary>
  public IReadOnlySet<string> BoundOptions { get; }

  /// <summary>
  ///   Gets the option whose value the next word is, when the previous token was an option expecting one.
  /// </summary>
  public CompiledOption? ExpectingValueFor { get; }

  /// <summary>
  ///   Gets how many positional arguments were already given to <see cref="Command" />.
  /// </summary>
  public int Positional { get; }
}
