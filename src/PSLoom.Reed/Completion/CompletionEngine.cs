// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace PSLoom.Reed.Completion;

/// <summary>
///   Turns a completion context and the word in progress into candidates. Filtering is a binary search over the node's pre-sorted
///   arrays followed by a walk while the prefix still matches, so a Tab press allocates only the results it returns.
/// </summary>
internal static class CompletionEngine {
  /// <summary>
  ///   Produces the completions for a word.
  /// </summary>
  /// <param name="context">Where the cursor is.</param>
  /// <param name="wordToComplete">The word being completed; empty offers everything valid here.</param>
  public static IReadOnlyList<CompletionResult> Complete(CompletionContext context, string? wordToComplete) {
    ArgumentNullException.ThrowIfNull(context);

    // An option's value is completed by its source, which this cycle does not have: offering the node's own words here would be
    // worse than offering nothing, because they are not valid in that position.
    if (context.ExpectingValueFor is not null) {
      return [];
    }

    var word = wordToComplete ?? string.Empty;
    var results = new List<CompletionResult>();

    if (!IsOption(word)) {
      Collect(results, context.Command.CommandCandidates, word, CompletionResultType.Command, context.BoundOptions, false);
    }

    Collect(results, context.Command.OptionCandidates, word, CompletionResultType.ParameterName, context.BoundOptions, true);

    return results;
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

  private static bool IsOption(string word)
    => word.Length > 0 && word[0] == '-';
}
