// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Dsl;
using PSLoom.Warp.Dsl;

namespace PSLoom.Reed.Verbs;

/// <summary>
///   <c>Use OptionGroup &lt;name&gt;</c> — copies a group's options into the enclosing command. The group must already be
///   declared: the copy is taken now, so options added to the group afterwards do not appear here.
/// </summary>
[LoomVerb("Use", typeof(CompleterScope), typeof(CommandScope))]
public sealed class UseVerb : LoomVerb {
  private const string OPTION_GROUP = "OptionGroup";

  /// <summary>
  ///   Gets or sets what is being used. Only <c>OptionGroup</c> exists today; the word keeps the statement readable.
  /// </summary>
  [Parameter(Mandatory = true, Position = 0)]
  [ValidateSet(OPTION_GROUP)]
  public string Kind { get; set; } = null!;

  /// <summary>
  ///   Gets or sets the group name.
  /// </summary>
  [Parameter(Mandatory = true, Position = 1)]
  public string Name { get; set; } = null!;

  /// <inheritdoc />
  protected override void Weave() {
    if (!Declarations.Completer(Loom).OptionGroups.TryGetValue(Name, out var options)) {
      throw ReedException.OptionGroupNotFound(Name);
    }

    var target = Declarations.Command(Loom);

    foreach (var option in options) {
      target.Options.Add(option.Clone());
    }
  }
}
