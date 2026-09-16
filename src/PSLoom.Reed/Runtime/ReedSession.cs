// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Management.Automation.Runspaces;
using System.Runtime.CompilerServices;
using PSLoom.Warp.Diagnostics;

namespace PSLoom.Reed.Runtime;

/// <summary>
///   Reed's state in one runspace: the registry, the providers, the cache, the trace log and the names already wired into tab
///   completion. Like every kernel store, it lives as long as its runspace and never leaks into another.
/// </summary>
internal sealed class ReedSession {
  private static readonly ConditionalWeakTable<Runspace, ReedSession> _sessions = [];

  /// <summary>
  ///   Creates a detached session. Every runspace's session comes from <see cref="For" />; a loose one is what a unit test of the
  ///   completion path wants, with no runspace in sight.
  /// </summary>
  internal ReedSession() { }

  /// <summary>
  ///   Gets the completers registered in this runspace.
  /// </summary>
  public CompleterRegistry Completers { get; } = new();

  /// <summary>
  ///   Gets the named sources registered in this runspace.
  /// </summary>
  public ProviderRegistry Providers { get; } = new();

  /// <summary>
  ///   Gets the candidates sources cached in this runspace.
  /// </summary>
  public CompletionCache Cache { get; } = new();

  /// <summary>
  ///   Gets the names already wired with <c>Register-ArgumentCompleter -Native</c>. Wiring is never unwound, so a name is wired
  ///   at most once per runspace.
  /// </summary>
  public HashSet<string> Wired { get; } = new(StringComparer.OrdinalIgnoreCase);

  /// <summary>
  ///   Gets or sets the trace log, provided when the harness is composed.
  /// </summary>
  public IDiagnosticLog<CompletionTrace>? Traces { get; set; }

  /// <summary>
  ///   Gets or sets the script block every wired name shares.
  /// </summary>
  public ScriptBlock? Bridge { get; set; }

  public static ReedSession For(Runspace runspace) {
    ArgumentNullException.ThrowIfNull(runspace);
    return _sessions.GetValue(runspace, static _ => new ReedSession());
  }

  /// <summary>
  ///   Gets the session of the runspace running this thread.
  /// </summary>
  /// <exception cref="ReedException">There is no current runspace (<c>REED_NO_RUNSPACE</c>).</exception>
  public static ReedSession ForCurrent()
    => For(Runspace.DefaultRunspace ?? throw ReedException.NoRunspace());

  /// <summary>
  ///   Records a tab press, when the harness gave Reed a log.
  /// </summary>
  public void Record(string command, string word, TimeSpan elapsed, int candidates, Exception? exception)
    => Traces?.Record(new CompletionTrace(command, word, elapsed, candidates, exception));
}
