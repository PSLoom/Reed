// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Dsl;
using PSLoom.Reed.Model;
using PSLoom.Reed.Runtime;
using PSLoom.Warp.Dsl;

namespace PSLoom.Reed.Verbs;

/// <summary>
///   <c>Sley &lt;Name&gt; { … }</c> — declares, validates and registers a completer for a native command in one step, the draft
///   equivalent of <c>New-Completer | Register-Completer</c>. A declaration that does not validate is reported and not registered;
///   the rest of the draft carries on.
/// </summary>
[LoomVerb("Sley", typeof(DraftScope), Reweave = ReweaveBehavior.Replay)]
public sealed class SleyVerb : LoomVerb, IRevertibleVerb {
  /// <summary>
  ///   Gets or sets the native command to complete.
  /// </summary>
  [Parameter(Mandatory = true, Position = 0)]
  [ReweaveKey]
  public string Name { get; set; } = null!;

  /// <summary>
  ///   Gets or sets the other names the command is known by; each is wired to this completer.
  /// </summary>
  [Parameter]
  public string[] Alias { get; set; } = [];

  /// <summary>
  ///   Gets or sets the description shown for the command itself.
  /// </summary>
  [Parameter]
  public string? Description { get; set; }

  /// <summary>
  ///   Gets or sets the declaration body.
  /// </summary>
  [Parameter(Mandatory = true, Position = 1)]
  [OpensScope(typeof(CompleterScope))]
  public ScriptBlock Body { get; set; } = null!;

  /// <summary>
  ///   Unregisters the completer a previous draft registered.
  /// </summary>
  public void Revert(ReweaveEntry previous) {
    ArgumentNullException.ThrowIfNull(previous);

    if (previous.BoundParameters.GetValueOrDefault(nameof(Name)) is string name) {
      ReedSession.ForCurrent().Completers.Remove(name);
    }
  }

  /// <inheritdoc />
  protected override void Weave() {
    var definition = new CompleterDefinition(Name, Alias, Description);
    Loom.RunScoped(new CompleterFrame(definition), Body);

    if (!CompleterGate.Approve(this, definition, ReportError)) {
      return;
    }

    // A draft owns the completers it declares, so re-running it replaces them instead of colliding with itself.
    ReedEngine.Register(ReedEngine.Of(this), definition, true);
  }
}
