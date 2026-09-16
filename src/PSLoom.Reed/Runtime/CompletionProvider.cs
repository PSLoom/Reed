// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace PSLoom.Reed.Runtime;

/// <summary>
///   A named source: what an <c>Argument -Provider &lt;name&gt;</c> resolves to at tab time. The indirection is what lets a
///   completer be exported and still complete values in the session that imports it.
/// </summary>
public sealed class CompletionProvider {
  internal CompletionProvider(string name, ScriptBlock scriptBlock, string? description) {
    Name = name;
    ScriptBlock = scriptBlock;
    Description = description;
  }

  /// <summary>
  ///   Gets the provider name.
  /// </summary>
  public string Name { get; }

  /// <summary>
  ///   Gets the script block producing the candidates.
  /// </summary>
  public ScriptBlock ScriptBlock { get; }

  /// <summary>
  ///   Gets what the provider completes, for <c>Get-CompletionProvider</c>.
  /// </summary>
  public string? Description { get; }

  /// <inheritdoc />
  public override string ToString()
    => Name;
}
