// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Model;

namespace PSLoom.Reed.Completion;

/// <summary>
///   An option of the compiled model.
/// </summary>
internal sealed class CompiledOption(string name, ArgumentNode? value) {
  public string Name { get; } = name;

  /// <summary>
  ///   Gets the value slot, or <see langword="null" /> when the option is a plain switch.
  /// </summary>
  public ArgumentNode? Value { get; } = value;

  /// <summary>
  ///   Gets a value indicating whether the next token belongs to this option.
  /// </summary>
  public bool TakesValue => Value is not null;
}
