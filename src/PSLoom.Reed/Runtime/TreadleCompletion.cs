// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics.CodeAnalysis;
using PSLoom.Warp.Treadles;

namespace PSLoom.Reed.Runtime;

/// <summary>
///   Completion inherited through treadles: <c>glog</c>, defined as <c>git log --oneline</c>, completes like <c>git log
///   --oneline</c> would. Reed only tracks which names carry which baked tokens; prepending them is the bridge's job, so the
///   tokenizer, the resolver and the engine never learn a prefix existed.
/// </summary>
internal static class TreadleCompletion {
  /// <summary>
  ///   Follows the treadle catalog of a runspace, from the harness's <c>Compose</c>.
  /// </summary>
  public static void Follow(ReedSession session, ITreadleCatalog catalog) {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(catalog);

    foreach (var treadle in catalog.All) {
      session.Treadles[treadle.Name] = treadle;
    }

    catalog.Changed += (_, change) => OnChanged(session, change);
    Wire(session.Engine, session);
  }

  /// <summary>
  ///   Wires every treadle whose target command has a completer. Called after a registration too, so declaring the treadle before
  ///   or after the completer makes no difference.
  /// </summary>
  public static void Wire(EngineIntrinsics? engine, ReedSession session) {
    ArgumentNullException.ThrowIfNull(session);

    var names = session.Treadles.Values
      .Where(treadle => session.Completers.TryGet(treadle.TargetCommand, out _))
      .Select(treadle => treadle.Name)
      .Concat(session.PendingWiring)
      .Where(name => !session.Wired.Contains(name))
      .Distinct(StringComparer.OrdinalIgnoreCase)
      .ToArray();

    if (names.Length == 0) {
      return;
    }

    if (engine is null) {
      // A treadle declared before Reed ran any cmdlet of its own: remember the name and wire it at the next registration.
      session.PendingWiring.UnionWith(names);
      return;
    }

    CompleterWiring.Ensure(engine, session, names);
  }

  /// <summary>
  ///   Resolves the completer a treadle inherits, and the tokens to prepend before resolving the line.
  /// </summary>
  public static bool TryResolve(ReedSession session, string command, [NotNullWhen(true)] out CompleterRegistration? registration,
    out IReadOnlyList<string> prefix) {
    ArgumentNullException.ThrowIfNull(session);

    if (session.Treadles.TryGetValue(command, out var treadle) &&
        session.Completers.TryGet(treadle.TargetCommand, out registration)) {
      prefix = treadle.BakedTokens;
      return true;
    }

    registration = null;
    prefix = [];

    return false;
  }

  private static void OnChanged(ReedSession session, TreadleChangedEventArgs change) {
    if (change.Current is { } current) {
      session.Treadles[change.Name] = current;
      Wire(session.Engine, session);

      return;
    }

    session.Treadles.Remove(change.Name);
  }
}
