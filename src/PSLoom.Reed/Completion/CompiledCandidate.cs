// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace PSLoom.Reed.Completion;

/// <summary>
///   One completable word of a node, ready to be offered: no work happens at Tab time beyond filtering.
/// </summary>
/// <param name="Text">The word.</param>
/// <param name="Description">The tooltip, never empty (it falls back to the word).</param>
/// <param name="IsAlias">Whether this spelling is an alias of another candidate.</param>
internal readonly record struct CompiledCandidate(string Text, string Description, bool IsAlias);
