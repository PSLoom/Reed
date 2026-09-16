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

  /// <summary>
  ///   Gets or sets the script block producing the candidates. It receives the word being completed, the command path, the bound
  ///   options and the positional arguments already typed.
  /// </summary>
  [Parameter]
  public ScriptBlock? Source { get; set; }

  /// <summary>
  ///   Gets or sets the registered provider producing the candidates. Unlike an inline source, a provider name survives an export.
  /// </summary>
  [Parameter]
  public string? Provider { get; set; }

  /// <summary>
  ///   Gets or sets the script block whose value decides when candidates may be reused instead of asking the source again.
  /// </summary>
  [Parameter]
  public ScriptBlock? CacheKey { get; set; }

  /// <inheritdoc />
  protected override void Weave() {
    var argument = new ArgumentNode(Name, Variadic.IsPresent) { Source = DeclaredSource() };

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

  private CompletionSource? DeclaredSource() {
    if (Source is not null &&
        !string.IsNullOrWhiteSpace(Provider)) {
      throw ReedException.InvalidDeclaration($"Argument '{Name}' has both -Source and -Provider; a slot is completed by one of them.",
        Name);
    }

    if (Source is { } scriptBlock) {
      return new CompletionSource(scriptBlock, CacheKey);
    }

    if (!string.IsNullOrWhiteSpace(Provider)) {
      return new CompletionSource(Provider, CacheKey);
    }

    return CacheKey is null
      ? null
      : throw ReedException.InvalidDeclaration($"Argument '{Name}' has a -CacheKey but no -Source or -Provider to cache.", Name);
  }
}
