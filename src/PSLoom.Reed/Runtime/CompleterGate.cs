// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Model;

namespace PSLoom.Reed.Runtime;

/// <summary>
///   The check every registration goes through, whether it comes from <c>Register-Completer</c> or from <c>Sley</c>: warnings are
///   written and the completer is still registered; an error is reported and it is not.
/// </summary>
internal static class CompleterGate {
  /// <summary>
  ///   Validates a completer, reporting what it finds.
  /// </summary>
  /// <param name="cmdlet">The cmdlet writing the warnings.</param>
  /// <param name="definition">The completer.</param>
  /// <param name="reportError">How to report an error; <see langword="null" /> writes it on the cmdlet.</param>
  /// <returns><see langword="true" /> when the completer may be registered.</returns>
  public static bool Approve(PSCmdlet cmdlet, CompleterDefinition definition, Action<ReedException>? reportError = null) {
    ArgumentNullException.ThrowIfNull(cmdlet);
    ArgumentNullException.ThrowIfNull(definition);

    var problems = CompleterValidator.Validate(definition);

    foreach (var warning in problems.Where(problem => !problem.IsError)) {
      cmdlet.WriteWarning($"{definition.Command}: {warning}");
    }

    if (problems.All(problem => !problem.IsError)) {
      return true;
    }

    var exception = ReedException.CompleterInvalid(definition.Command, [.. problems.Where(problem => problem.IsError)]);

    if (reportError is null) {
      cmdlet.WriteError(exception.ToErrorRecord());
      return false;
    }

    reportError(exception);

    return false;
  }
}
