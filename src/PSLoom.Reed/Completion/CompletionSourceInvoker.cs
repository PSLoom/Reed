// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Model;
using PSLoom.Reed.Runtime;

namespace PSLoom.Reed.Completion;

/// <summary>
///   Runs a value slot's source. The script block receives, positionally, the word being completed, the command path, the options
///   already bound and the positional arguments already typed — the four things a source can reasonably decide on.
/// </summary>
internal static class CompletionSourceInvoker {
  /// <summary>
  ///   Produces a source's candidates, from the cache when its key says the previous answer still holds.
  /// </summary>
  /// <param name="session">The runspace's Reed state, for the provider registry and the cache.</param>
  /// <param name="source">The source to run.</param>
  /// <param name="context">Where the cursor is.</param>
  /// <param name="word">The word being completed.</param>
  /// <returns>The candidates, unfiltered; empty when the source is a provider nobody registered.</returns>
  public static IReadOnlyList<SourceCandidate> Invoke(ReedSession session, CompletionSource source, CompletionContext context,
    string word) {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(context);

    if (Resolve(session, source) is not { } scriptBlock) {
      // An unregistered provider is not an error: an imported completer must survive a session that never registered it.
      return [];
    }

    var arguments = Arguments(context, word);
    var key = CacheKey(source, arguments);

    if (key is not null &&
        session.Cache.TryGet(source.Id, key, out var cached)) {
      return cached;
    }

    var candidates = Candidates(scriptBlock.Invoke(arguments));

    if (key is not null) {
      session.Cache.Set(source.Id, key, candidates);
    }

    return candidates;
  }

  private static ScriptBlock? Resolve(ReedSession session, CompletionSource source)
    => source.Kind == CompletionSourceKind.Provider
      ? session.Providers.TryGet(source.ProviderName!, out var provider) ? provider.ScriptBlock : null
      : source.ScriptBlock;

  private static object?[] Arguments(CompletionContext context, string word)
    => [word, context.CommandPath, context.BoundOptions, context.BoundArguments];

  private static string? CacheKey(CompletionSource source, object?[] arguments) {
    if (source.CacheKey is not { } cacheKey) {
      return null;
    }

    var value = cacheKey.Invoke(arguments).Select(Text).FirstOrDefault(text => !string.IsNullOrEmpty(text));

    // No key means the source cannot say when its answer stops holding, so it runs again.
    return string.IsNullOrEmpty(value) ? null : value;
  }

  private static IReadOnlyList<SourceCandidate> Candidates(IEnumerable<PSObject?> results) {
    var candidates = new List<SourceCandidate>();

    foreach (var result in results) {
      if (result?.BaseObject is CompletionResult completion) {
        candidates.Add(new SourceCandidate(completion.CompletionText, completion.ToolTip, completion.ResultType));
        continue;
      }

      if (Text(result) is { Length: > 0 } text) {
        candidates.Add(new SourceCandidate(text, text, CompletionResultType.ParameterValue));
      }
    }

    return candidates;
  }

  private static string? Text(PSObject? value)
    => value?.BaseObject switch {
      null => null,
      string text => text,
      var other => LanguagePrimitives.ConvertTo<string>(other)
    };
}
