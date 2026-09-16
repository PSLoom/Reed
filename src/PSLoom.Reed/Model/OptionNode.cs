// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace PSLoom.Reed.Model;

/// <summary>
///   An option: a flag, which takes a value when it declares one.
/// </summary>
public sealed class OptionNode {
  internal OptionNode(string name, IReadOnlyList<string> aliases, string? description) {
    Name = name;
    Aliases = aliases;
    Description = description;
  }

  /// <summary>
  ///   Gets the option name, as typed (<c>--message</c>).
  /// </summary>
  public string Name { get; }

  /// <summary>
  ///   Gets the alternative spellings (<c>-m</c>).
  /// </summary>
  public IReadOnlyList<string> Aliases { get; }

  /// <summary>
  ///   Gets the description shown beside the candidate.
  /// </summary>
  public string? Description { get; }

  /// <summary>
  ///   Gets or sets the value the option takes; <see langword="null" /> for a plain switch.
  /// </summary>
  public ArgumentNode? Value { get; internal set; }

  /// <summary>
  ///   Gets every spelling of this option, the name first.
  /// </summary>
  public IEnumerable<string> Names => [Name, .. Aliases];

  /// <summary>
  ///   Copies this option, so an option group's template is never shared with the nodes that use it.
  /// </summary>
  internal OptionNode Clone()
    => new(Name, [.. Aliases], Description) {
      Value = Value is { } value ? new ArgumentNode(value.Name, value.Variadic) { Source = value.Source } : null
    };

  /// <inheritdoc />
  public override string ToString()
    => Name;
}
