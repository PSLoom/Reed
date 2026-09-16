// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Runtime;

namespace PSLoom.Reed.Completion;

/// <summary>
///   Turns a completion context and the word in progress into candidates. Filtering names is a binary search over the node's
///   pre-sorted arrays followed by a walk while the prefix still matches, so a Tab press allocates only the results it returns.
/// </summary>
internal static class CompletionEngine {
  /// <summary>
  ///   Produces the completions for a word.
  /// </summary>
  /// <param name="session">The runspace's Reed state, for sources that need the providers or the cache.</param>
  /// <param name="context">Where the cursor is.</param>
  /// <param name="wordToComplete">The word being completed; empty offers everything valid here.</param>
  public static IReadOnlyList<CompletionResult> Complete(ReedSession session, CompletionContext context, string? wordToComplete) {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(context);

    var word = wordToComplete ?? string.Empty;
    var results = new List<CompletionResult>();

    // The next word belongs to an option: only that option's source has anything valid to say here.
    if (context.ExpectingValueFor is not null) {
      FromSource(session, context, word, results);
      return results;
    }

    if (!IsOption(word)) {
      Collect(results, context.Command.CommandCandidates, word, CompletionResultType.Command, context.BoundOptions, false);
      FromSource(session, context, word, results);
    }

    Collect(results, context.Command.OptionCandidates, word, CompletionResultType.ParameterName, context.BoundOptions, true);

    return results;
  }

  private static void FromSource(ReedSession session, CompletionContext context, string word, List<CompletionResult> results) {
    if (context.ExpectedArgument?.Source is not { } source) {
      return;
    }

    foreach (var candidate in CompletionSourceInvoker.Invoke(session, source, context, word)) {
      // A source may ignore the word entirely, so the prefix is applied here rather than trusted.
      if (candidate.Text.StartsWith(word, StringComparison.OrdinalIgnoreCase)) {
        results.Add(new CompletionResult(Quote(candidate.Text), candidate.Text, candidate.Type, candidate.ToolTip));
      }
    }
  }

  private static void Collect(List<CompletionResult> results, CompiledCandidate[] candidates, string word, CompletionResultType type,
    IReadOnlySet<string> bound, bool skipBound) {
    for (var index = LowerBound(candidates, word); index < candidates.Length; index++) {
      var candidate = candidates[index];

      if (!candidate.Text.StartsWith(word, StringComparison.Ordinal)) {
        return;
      }

      // An empty word lists what exists; repeating every spelling of it would double the menu. Once the user types, the alias is
      // what they may be typing, so it is offered.
      if (candidate.IsAlias &&
          word.Length == 0) {
        continue;
      }

      if (skipBound &&
          bound.Contains(candidate.Text)) {
        continue;
      }

      results.Add(new CompletionResult(candidate.Text, candidate.Text, type, candidate.Description));
    }
  }

  private static int LowerBound(CompiledCandidate[] candidates, string word) {
    var low = 0;
    var high = candidates.Length;

    while (low < high) {
      var middle = (low + high) / 2;

      if (string.CompareOrdinal(candidates[middle].Text, word) < 0) {
        low = middle + 1;
        continue;
      }

      high = middle;
    }

    return low;
  }

  private static string Quote(string text)
    => text.Any(char.IsWhiteSpace) || text.Contains('\'', StringComparison.Ordinal)
      ? $"'{text.Replace("'", "''", StringComparison.Ordinal)}'"
      : text;

  private static bool IsOption(string word)
    => word.Length > 0 && word[0] == '-';
}
