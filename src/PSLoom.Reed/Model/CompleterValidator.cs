// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace PSLoom.Reed.Model;

/// <summary>
///   Checks a declared completer before it is registered. Everything here is a structural rule: what the declaration says can
///   never resolve, or resolves ambiguously.
/// </summary>
public static class CompleterValidator {
  /// <summary>
  ///   Validates a completer.
  /// </summary>
  /// <param name="definition">The completer.</param>
  /// <returns>Every problem found, errors and warnings, in declaration order.</returns>
  public static IReadOnlyList<CompleterProblem> Validate(CompleterDefinition definition) {
    ArgumentNullException.ThrowIfNull(definition);

    var problems = new List<CompleterProblem>();
    Walk(definition.Root, definition.Command, problems);

    return problems;
  }

  private static void Walk(CommandNode node, string path, List<CompleterProblem> problems) {
    var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    foreach (var command in node.Commands) {
      foreach (var name in Spellings(command.Name, command.Aliases)) {
        Claim(names, name, command.Name, "subcommand", path, problems);
      }
    }

    var optionNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    foreach (var option in node.Options) {
      foreach (var name in option.Names) {
        Claim(optionNames, name, option.Name, "option", path, problems);
      }
    }

    for (var index = 0; index < node.Arguments.Count; index++) {
      if (node.Arguments[index].Variadic &&
          index != node.Arguments.Count - 1) {
        problems.Add(new CompleterProblem($"{path}/{node.Arguments[index].Name}",
          "a variadic argument takes every remaining value, so nothing may follow it", true));
      }
    }

    foreach (var command in node.Commands) {
      Walk(command, $"{path}/{command.Name}", problems);
    }
  }

  private static IEnumerable<string> Spellings(string name, IReadOnlyList<string> aliases)
    => [name, .. aliases];

  private static void Claim(Dictionary<string, string> claimed, string name, string owner, string kind, string path,
    List<CompleterProblem> problems) {
    if (string.IsNullOrWhiteSpace(name)) {
      problems.Add(new CompleterProblem(path, $"a {kind} has an empty name", true));
      return;
    }

    if (!claimed.TryAdd(name, owner)) {
      problems.Add(new CompleterProblem($"{path}/{name}",
        claimed[name].Equals(owner, StringComparison.OrdinalIgnoreCase)
          ? $"the {kind} '{owner}' declares '{name}' twice"
          : $"'{name}' is declared by both {kind}s '{claimed[name]}' and '{owner}'", true));
    }
  }
}
