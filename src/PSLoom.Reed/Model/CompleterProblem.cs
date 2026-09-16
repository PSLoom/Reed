// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace PSLoom.Reed.Model;

/// <summary>
///   Something validation found in a completer, at the path it was found.
/// </summary>
/// <param name="Path">The node path, for example <c>git/commit/--message</c>.</param>
/// <param name="Message">What is wrong.</param>
/// <param name="IsError">
///   <see langword="true" /> when the completer must not be registered; <see langword="false" /> for a warning.
/// </param>
public sealed record CompleterProblem(string Path, string Message, bool IsError) {
  /// <inheritdoc />
  public override string ToString()
    => $"{Path}: {Message}";
}
