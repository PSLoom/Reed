// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.Management.Automation.Language;
using PSLoom.Reed.Completion;

namespace PSLoom.Reed.Runtime;

/// <summary>
///   What PowerShell calls on every Tab press for a command Reed completes. Public because the registered script block names this
///   type; it is not part of Reed's supported surface.
/// </summary>
public static class ReedBridge {
  /// <summary>
  ///   The script block registered for every completed name. It does nothing but marshal into <see cref="Complete" />.
  /// </summary>
  internal const string SCRIPT =
    "param($wordToComplete, $commandAst, $cursorPosition) [PSLoom.Reed.Runtime.ReedBridge]::Complete($wordToComplete, $commandAst, $cursorPosition)";

  /// <summary>
  ///   Completes a word of a native command line. It never throws: a Tab press must not break the command line the user is
  ///   typing, so a failure returns nothing and is recorded for <c>Trace-Completion</c>.
  /// </summary>
  /// <param name="wordToComplete">The word being completed.</param>
  /// <param name="commandAst">The command being typed.</param>
  /// <param name="cursorPosition">The cursor offset in the input.</param>
  /// <returns>The candidates, or nothing.</returns>
  public static IEnumerable<CompletionResult> Complete(string? wordToComplete, CommandAst? commandAst, int cursorPosition) {
    var word = wordToComplete ?? string.Empty;
    var command = commandAst?.GetCommandName() ?? string.Empty;
    var started = Stopwatch.GetTimestamp();
    ReedSession? session = null;

    try {
      session = ReedSession.ForCurrent();

      if (commandAst is null ||
          !session.Completers.TryGet(command, out var registration)) {
        session.Record(command, word, Stopwatch.GetElapsedTime(started), 0, null);
        return [];
      }

      var tokens = TokenClassifier.Preceding(commandAst, cursorPosition);
      var results = CompletionEngine.Complete(session, ContextResolver.Resolve(registration.Compiled, tokens), word);

      session.Record(command, word, Stopwatch.GetElapsedTime(started), results.Count, null);

      return results;
    }
    catch (Exception exception) when (exception is not (OutOfMemoryException or StackOverflowException)) {
      session?.Record(command, word, Stopwatch.GetElapsedTime(started), 0, exception);
      return [];
    }
  }
}
