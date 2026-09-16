// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics.CodeAnalysis;
using PSLoom.Reed.Completion;

namespace PSLoom.Reed.Runtime;

/// <summary>
///   Candidates a source produced, kept while its cache key stays the same. Entries live for the session: a key is the
///   declaration's own statement of when its answer is still good, and <c>Clear-CompletionCache</c> is the way out when that
///   statement turns out to be too coarse.
/// </summary>
internal sealed class CompletionCache {
  private readonly Dictionary<(Guid Source, string Key), IReadOnlyList<SourceCandidate>> _entries = [];
  private readonly Lock _lock = new();

  /// <summary>
  ///   Gets how many entries are cached.
  /// </summary>
  public int Count {
    get {
      lock (_lock) {
        return _entries.Count;
      }
    }
  }

  public bool TryGet(Guid source, string key, [NotNullWhen(true)] out IReadOnlyList<SourceCandidate>? candidates) {
    lock (_lock) {
      return _entries.TryGetValue((source, key), out candidates);
    }
  }

  public void Set(Guid source, string key, IReadOnlyList<SourceCandidate> candidates) {
    lock (_lock) {
      _entries[(source, key)] = candidates;
    }
  }

  /// <summary>
  ///   Forgets everything a set of sources cached, so a redeclared completer never serves the previous declaration's candidates.
  /// </summary>
  public void Remove(IEnumerable<Guid> sources) {
    ArgumentNullException.ThrowIfNull(sources);

    var dropped = new HashSet<Guid>(sources);

    if (dropped.Count == 0) {
      return;
    }

    lock (_lock) {
      foreach (var entry in _entries.Keys.Where(key => dropped.Contains(key.Source)).ToArray()) {
        _entries.Remove(entry);
      }
    }
  }

  /// <summary>
  ///   Forgets everything.
  /// </summary>
  /// <returns>How many entries were dropped.</returns>
  public int Clear() {
    lock (_lock) {
      var count = _entries.Count;
      _entries.Clear();

      return count;
    }
  }
}
