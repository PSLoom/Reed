// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Dsl;
using PSLoom.Reed.Model;
using PSLoom.Warp.Dsl;

namespace PSLoom.Reed.Verbs;

/// <summary>
///   <c>Option &lt;name&gt; [{ Argument … }]</c> — a flag of the enclosing command or option group. With a body it takes a value;
///   without one it is a plain switch.
/// </summary>
[LoomVerb("Option", typeof(CompleterScope), typeof(CommandScope), typeof(OptionGroupScope))]
public sealed class OptionVerb : LoomVerb {
  /// <summary>
  ///   Gets or sets the option as typed, for example <c>--message</c>.
  /// </summary>
  [Parameter(Mandatory = true, Position = 0)]
  public string Name { get; set; } = null!;

  /// <summary>
  ///   Gets or sets the other spellings, for example <c>-m</c>.
  /// </summary>
  [Parameter]
  public string[] Alias { get; set; } = [];

  /// <summary>
  ///   Gets or sets the description shown beside the candidate.
  /// </summary>
  [Parameter]
  public string? Description { get; set; }

  /// <summary>
  ///   Gets or sets the value this option takes.
  /// </summary>
  [Parameter(Position = 1)]
  [OpensScope(typeof(OptionScope))]
  public ScriptBlock? Body { get; set; }

  /// <inheritdoc />
  protected override void Weave() {
    var option = new OptionNode(Name, Alias, Description);
    Declarations.AddOption(Loom, option);
    Loom.RunScoped(new OptionFrame(option), Body);
  }
}
