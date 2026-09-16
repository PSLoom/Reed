// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Dsl;
using PSLoom.Reed.Model;
using PSLoom.Warp.Dsl;

namespace PSLoom.Reed.Verbs;

/// <summary>
///   <c>Command &lt;name&gt; { … }</c> — a subcommand of the enclosing command. Nests to any depth.
/// </summary>
[LoomVerb("Command", typeof(CompleterScope), typeof(CommandScope))]
public sealed class CommandVerb : LoomVerb {
  /// <summary>
  ///   Gets or sets the subcommand name.
  /// </summary>
  [Parameter(Mandatory = true, Position = 0)]
  public string Name { get; set; } = null!;

  /// <summary>
  ///   Gets or sets the other names it answers to.
  /// </summary>
  [Parameter]
  public string[] Alias { get; set; } = [];

  /// <summary>
  ///   Gets or sets the description shown beside the candidate.
  /// </summary>
  [Parameter]
  public string? Description { get; set; }

  /// <summary>
  ///   Gets or sets what is valid under this subcommand.
  /// </summary>
  [Parameter(Position = 1)]
  [OpensScope(typeof(CommandScope))]
  public ScriptBlock? Body { get; set; }

  /// <inheritdoc />
  protected override void Weave() {
    var node = new CommandNode(Name, Alias, Description);
    Declarations.Command(Loom).Commands.Add(node);
    Loom.RunScoped(new CommandFrame(node), Body);
  }
}
