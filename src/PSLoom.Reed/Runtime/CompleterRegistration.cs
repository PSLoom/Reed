// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Completion;
using PSLoom.Reed.Model;

namespace PSLoom.Reed.Runtime;

/// <summary>
///   A completer registered in a session: the declaration, the model compiled from it, and every name it answers to.
/// </summary>
public sealed class CompleterRegistration {
  internal CompleterRegistration(CompleterDefinition definition) {
    Definition = definition;
    Names = [.. definition.Names];
    Compiled = CompiledCompleter.Compile(definition);
  }

  /// <summary>
  ///   Gets the identity of this registration, stable while it stays registered.
  /// </summary>
  public Guid Id { get; } = Guid.NewGuid();

  /// <summary>
  ///   Gets the command this completer completes.
  /// </summary>
  public string Command => Definition.Command;

  /// <summary>
  ///   Gets every name wired to this completer, the command first.
  /// </summary>
  public IReadOnlyList<string> Names { get; }

  /// <summary>
  ///   Gets the declaration.
  /// </summary>
  public CompleterDefinition Definition { get; }

  internal CompiledCompleter Compiled { get; }

  /// <inheritdoc />
  public override string ToString()
    => Definition.ToString();
}
