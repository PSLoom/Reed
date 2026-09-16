// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Model;
using PSLoom.Warp;

namespace PSLoom.Reed;

/// <summary>
///   Raised by Reed.
/// </summary>
public sealed class ReedException : PowerShellException {
  internal const string INVALID_DECLARATION = "REED_INVALID_DECLARATION";
  internal const string COMPLETER_INVALID = "REED_COMPLETER_INVALID";
  internal const string COMPLETER_NAME_TAKEN = "REED_COMPLETER_NAME_TAKEN";
  internal const string OPTION_GROUP_NOT_FOUND = "REED_OPTION_GROUP_NOT_FOUND";
  internal const string WIRING_FAILED = "REED_WIRING_FAILED";
  internal const string NO_RUNSPACE = "REED_NO_RUNSPACE";
  internal const string PROVIDER_NAME_TAKEN = "REED_PROVIDER_NAME_TAKEN";
  internal const string COMPLETER_NOT_PORTABLE = "REED_COMPLETER_NOT_PORTABLE";
  internal const string COMPLETER_NOT_READABLE = "REED_COMPLETER_NOT_READABLE";
  internal const string COMPLETER_NOT_FOUND = "REED_COMPLETER_NOT_FOUND";

  private ReedException(string errorId, ErrorCategory errorCategory, string message, object? targetObject, Exception? innerException = null)
    : base(errorId, errorCategory, message, targetObject, innerException) { }

  internal static ReedException NoRunspace()
    => new(NO_RUNSPACE, ErrorCategory.InvalidOperation, "Reed needs a runspace on this thread; there is none.", null);

  internal static ReedException InvalidDeclaration(string message, object? target = null)
    => new(INVALID_DECLARATION, ErrorCategory.InvalidArgument, message, target);

  internal static ReedException OptionGroupNotFound(string name)
    => new(OPTION_GROUP_NOT_FOUND, ErrorCategory.ObjectNotFound,
      $"No option group named '{name}' is defined yet; declare 'OptionGroup {name} {{ … }}' before using it.", name);

  internal static ReedException CompleterInvalid(string command, IReadOnlyList<CompleterProblem> problems)
    => new(COMPLETER_INVALID, ErrorCategory.InvalidData,
      $"The completer for '{command}' is invalid: {string.Join("; ", problems.Select(problem => problem.ToString()))}", command);

  internal static ReedException CompleterNameTaken(string name, string registeredCommand)
    => new(COMPLETER_NAME_TAKEN, ErrorCategory.ResourceExists, name.Equals(registeredCommand, StringComparison.OrdinalIgnoreCase)
      ? $"A completer for '{registeredCommand}' is already registered; use -Force to replace it."
      : $"'{name}' is already taken by the completer registered for '{registeredCommand}'; use -Force to replace it.", name);

  internal static ReedException ProviderNameTaken(string name)
    => new(PROVIDER_NAME_TAKEN, ErrorCategory.ResourceExists,
      $"A completion provider named '{name}' is already registered; use -Force to replace it.", name);

  internal static ReedException CompleterNotFound(string command)
    => new(COMPLETER_NOT_FOUND, ErrorCategory.ObjectNotFound, $"No completer is registered for '{command}' in this session.", command);

  internal static ReedException CompleterNotPortable(string command, IReadOnlyList<string> paths)
    => new(COMPLETER_NOT_PORTABLE, ErrorCategory.InvalidData,
      $"The completer for '{command}' declares inline sources, which cannot be written to a file: {string.Join(", ", paths)}. " +
      "Use -Provider instead, or -Force to export without them.", command);

  internal static ReedException CompleterNotReadable(string path, string reason, Exception? exception = null)
    => new(COMPLETER_NOT_READABLE, ErrorCategory.InvalidData, $"'{path}' is not a completer this version can read: {reason}", path,
      exception);

  internal static ReedException WiringFailed(string command, Exception exception)
    => new(WIRING_FAILED, ErrorCategory.NotSpecified,
      $"The completer for '{command}' could not be wired into tab completion: {exception.Message}", command, exception);
}
