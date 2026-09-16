// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace PSLoom.Reed.Model;

/// <summary>
///   A value slot: a command's positional argument, or the value an option takes. What completes it comes from a source, which
///   the next cycle adds; declaring the slot already tells the resolver that the next token is a value and not an option.
/// </summary>
public sealed class ArgumentNode {
  internal ArgumentNode(string name, bool variadic) {
    Name = name;
    Variadic = variadic;
  }

  /// <summary>
  ///   Gets the slot name, used in messages and tooltips.
  /// </summary>
  public string Name { get; }

  /// <summary>
  ///   Gets a value indicating whether the slot accepts every remaining positional argument.
  /// </summary>
  public bool Variadic { get; }

  /// <summary>
  ///   Gets or sets what completes this slot; <see langword="null" /> declares the slot without completing it.
  /// </summary>
  public CompletionSource? Source { get; internal set; }

  /// <inheritdoc />
  public override string ToString()
    => Variadic ? $"{Name}…" : Name;
}
