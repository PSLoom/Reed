// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace PSLoom.Reed.Completion;

/// <summary>
///   One candidate as a source produced it, before Reed filters by the word being completed. Cached in this shape, so a cache hit
///   still filters against the current word.
/// </summary>
/// <param name="Text">The word to insert.</param>
/// <param name="ToolTip">The tooltip.</param>
/// <param name="Type">How PowerShell should present it.</param>
internal readonly record struct SourceCandidate(string Text, string ToolTip, CompletionResultType Type);
