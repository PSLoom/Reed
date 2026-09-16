// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace PSLoom.Reed.Runtime;

/// <summary>
///   One tab press, recorded whether it succeeded or not.
/// </summary>
public sealed class CompletionTrace {
  internal CompletionTrace(string command, string wordToComplete, TimeSpan elapsed, int candidates, Exception? exception) {
    Command = command;
    WordToComplete = wordToComplete;
    Elapsed = elapsed;
    Candidates = candidates;
    Exception = exception;
    Timestamp = DateTimeOffset.Now;
  }

  /// <summary>
  ///   Gets when the completion ran.
  /// </summary>
  public DateTimeOffset Timestamp { get; }

  /// <summary>
  ///   Gets the command being completed.
  /// </summary>
  public string Command { get; }

  /// <summary>
  ///   Gets the word being completed.
  /// </summary>
  public string WordToComplete { get; }

  /// <summary>
  ///   Gets how long the completion took.
  /// </summary>
  public TimeSpan Elapsed { get; }

  /// <summary>
  ///   Gets how many candidates were returned.
  /// </summary>
  public int Candidates { get; }

  /// <summary>
  ///   Gets the failure the bridge swallowed, if any.
  /// </summary>
  public Exception? Exception { get; }

  /// <inheritdoc />
  public override string ToString()
    => Exception is null
      ? $"{Command} '{WordToComplete}' → {Candidates} in {Elapsed.TotalMilliseconds:F2} ms"
      : $"{Command} '{WordToComplete}' failed in {Elapsed.TotalMilliseconds:F2} ms: {Exception.Message}";
}
