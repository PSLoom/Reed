// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Management.Automation.Language;

namespace PSLoom.Reed.Completion;

/// <summary>
///   Turns the command line PowerShell hands the completer into the argument tokens already typed, without the command name and
///   without the word being completed.
/// </summary>
internal static class TokenClassifier {
  /// <summary>
  ///   Extracts the typed argument tokens of a command.
  /// </summary>
  /// <param name="commandAst">The command being completed.</param>
  /// <param name="cursorPosition">The cursor offset in the whole input.</param>
  /// <returns>The tokens preceding the cursor, in order.</returns>
  public static IReadOnlyList<string> Preceding(CommandAst commandAst, int cursorPosition) {
    ArgumentNullException.ThrowIfNull(commandAst);

    var tokens = new List<string>(commandAst.CommandElements.Count);

    foreach (var element in commandAst.CommandElements.Skip(1)) {
      // The element under the cursor is the word in progress: it is what we are completing, not context.
      if (element.Extent.StartOffset < cursorPosition &&
          cursorPosition <= element.Extent.EndOffset) {
        continue;
      }

      if (element.Extent.StartOffset >= cursorPosition) {
        // Everything after the cursor belongs to a later word the user already typed; it is not context for this one.
        break;
      }

      tokens.Add(Text(element));
    }

    return tokens;
  }

  private static string Text(CommandElementAst element)
    => element switch {
      CommandParameterAst parameter => parameter.Extent.Text,
      StringConstantExpressionAst text => text.Value,
      var _ => element.Extent.Text
    };
}
