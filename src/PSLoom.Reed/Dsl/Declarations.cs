// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Model;
using PSLoom.Warp.Dsl;

namespace PSLoom.Reed.Dsl;

/// <summary>
///   Where a verb's declaration belongs. The scope table already guarantees a verb only runs where it is valid, so finding the
///   nearest frame of a kind is enough to know what it is declaring into.
/// </summary>
internal static class Declarations {
  /// <summary>
  ///   The command a declaration attaches to: the enclosing subcommand, or the completer's root.
  /// </summary>
  public static CommandNode Command(ILoomContext loom)
    => loom.Frame<CommandFrame>()?.Node ?? loom.RequireFrame<CompleterFrame>().Definition.Root;

  /// <summary>
  ///   The completer being declared.
  /// </summary>
  public static CompleterDefinition Completer(ILoomContext loom)
    => loom.RequireFrame<CompleterFrame>().Definition;

  /// <summary>
  ///   Adds an option to the enclosing group, or to the enclosing command when there is none.
  /// </summary>
  public static void AddOption(ILoomContext loom, OptionNode option) {
    if (loom.Frame<OptionGroupFrame>() is { } group) {
      group.Options.Add(option);
      return;
    }

    Command(loom).Options.Add(option);
  }
}
