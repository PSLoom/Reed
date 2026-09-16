// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Model;

namespace PSLoom.Reed.Completion;

/// <summary>
///   A command of the compiled model: case-insensitive lookups for resolution and pre-sorted candidate arrays for filtering. Built
///   once, at registration, because every Tab press walks it.
/// </summary>
/// <remarks>
///   The lookups are plain dictionaries, not frozen ones. A completer node holds a handful of names, where a frozen dictionary buys
///   nothing measurable on a Tab press already ~10,000 times under budget, but its first construction costs several milliseconds of
///   JIT per generic instantiation — paid inside the draft, whose budget has no such headroom.
/// </remarks>
internal sealed class CompiledCommand {
  private CompiledCommand(string name, IReadOnlyDictionary<string, CompiledCommand> commands, IReadOnlyDictionary<string, CompiledOption> options,
    CompiledCandidate[] commandCandidates, CompiledCandidate[] optionCandidates, IReadOnlyList<ArgumentNode> arguments) {
    Name = name;
    Commands = commands;
    Options = options;
    CommandCandidates = commandCandidates;
    OptionCandidates = optionCandidates;
    Arguments = arguments;
  }

  public string Name { get; }

  /// <summary>
  ///   Gets the subcommands, by name and by alias.
  /// </summary>
  public IReadOnlyDictionary<string, CompiledCommand> Commands { get; }

  /// <summary>
  ///   Gets the options, by name and by alias.
  /// </summary>
  public IReadOnlyDictionary<string, CompiledOption> Options { get; }

  /// <summary>
  ///   Gets the subcommand candidates, ordinal-sorted for prefix filtering.
  /// </summary>
  public CompiledCandidate[] CommandCandidates { get; }

  /// <summary>
  ///   Gets the option candidates, ordinal-sorted for prefix filtering.
  /// </summary>
  public CompiledCandidate[] OptionCandidates { get; }

  /// <summary>
  ///   Gets the positional arguments, in declaration order.
  /// </summary>
  public IReadOnlyList<ArgumentNode> Arguments { get; }

  public static CompiledCommand Compile(CommandNode node) {
    var commands = new Dictionary<string, CompiledCommand>(StringComparer.OrdinalIgnoreCase);
    var commandCandidates = new List<CompiledCandidate>();

    foreach (var child in node.Commands) {
      var compiled = Compile(child);

      foreach (var name in Spellings(child.Name, child.Aliases)) {
        commands[name.Text] = compiled;
        commandCandidates.Add(new CompiledCandidate(name.Text, child.Description ?? child.Name, name.IsAlias));
      }
    }

    var options = new Dictionary<string, CompiledOption>(StringComparer.OrdinalIgnoreCase);
    var optionCandidates = new List<CompiledCandidate>();

    foreach (var option in node.Options) {
      var compiled = new CompiledOption(option.Name, option.Value);

      foreach (var name in Spellings(option.Name, option.Aliases)) {
        options[name.Text] = compiled;
        optionCandidates.Add(new CompiledCandidate(name.Text, option.Description ?? option.Name, name.IsAlias));
      }
    }

    return new CompiledCommand(node.Name, commands, options, Sorted(commandCandidates), Sorted(optionCandidates), [.. node.Arguments]);
  }

  private static IEnumerable<(string Text, bool IsAlias)> Spellings(string name, IReadOnlyList<string> aliases)
    => [(name, false), .. aliases.Select(alias => (alias, true))];

  private static CompiledCandidate[] Sorted(List<CompiledCandidate> candidates) {
    var sorted = candidates.ToArray();
    Array.Sort(sorted, static (left, right) => string.CompareOrdinal(left.Text, right.Text));

    return sorted;
  }
}
