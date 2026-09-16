// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Dsl;
using PSLoom.Warp.Dsl;

namespace PSLoom.Reed.Verbs;

/// <summary>
///   <c>OptionGroup &lt;name&gt; { Option … }</c> — names a set of options for reuse. The group itself completes nothing; it is a
///   template <c>Use OptionGroup</c> copies into a command.
/// </summary>
[LoomVerb("OptionGroup", typeof(CompleterScope))]
public sealed class OptionGroupVerb : LoomVerb {
  /// <summary>
  ///   Gets or sets the group name.
  /// </summary>
  [Parameter(Mandatory = true, Position = 0)]
  public string Name { get; set; } = null!;

  /// <summary>
  ///   Gets or sets the options the group collects.
  /// </summary>
  [Parameter(Mandatory = true, Position = 1)]
  [OpensScope(typeof(OptionGroupScope))]
  public ScriptBlock Body { get; set; } = null!;

  /// <inheritdoc />
  protected override void Weave() {
    var completer = Declarations.Completer(Loom);

    if (completer.OptionGroups.ContainsKey(Name)) {
      throw ReedException.InvalidDeclaration($"Option group '{Name}' is declared more than once.", Name);
    }

    var frame = new OptionGroupFrame(Name);
    Loom.RunScoped(frame, Body);
    completer.OptionGroups[Name] = frame.Options;
  }
}
