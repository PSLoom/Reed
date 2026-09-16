// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Dsl;
using PSLoom.Reed.Model;
using PSLoom.Warp.Dsl;

namespace PSLoom.Reed.Verbs;

/// <summary>
///   <c>Argument &lt;name&gt; [-Variadic]</c> — a value slot: the enclosing option's value, or the next positional argument of the
///   enclosing command.
/// </summary>
[LoomVerb("Argument", typeof(CompleterScope), typeof(CommandScope), typeof(OptionScope))]
public sealed class ArgumentVerb : LoomVerb {
  /// <summary>
  ///   Gets or sets the slot name, used in messages and tooltips.
  /// </summary>
  [Parameter(Mandatory = true, Position = 0)]
  public string Name { get; set; } = null!;

  /// <summary>
  ///   Gets or sets a value indicating whether the slot takes every remaining positional argument.
  /// </summary>
  [Parameter]
  public SwitchParameter Variadic { get; set; }

  /// <inheritdoc />
  protected override void Weave() {
    var argument = new ArgumentNode(Name, Variadic.IsPresent);

    if (Loom.Frame<OptionFrame>() is not { } frame) {
      Declarations.Command(Loom).Arguments.Add(argument);
      return;
    }

    if (frame.Option.Value is not null) {
      throw ReedException.InvalidDeclaration($"Option '{frame.Option.Name}' already takes a value; an option takes at most one.",
        frame.Option.Name);
    }

    frame.Option.Value = argument;
  }
}
