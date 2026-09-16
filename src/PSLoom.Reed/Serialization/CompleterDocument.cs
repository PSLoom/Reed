// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Text.Json.Serialization;

namespace PSLoom.Reed.Serialization;

/// <summary>
///   The on-disk shape of a completer. It mirrors the model rather than reusing it, so the file format and the runtime tree can
///   change apart from each other.
/// </summary>
public sealed record CompleterDocument {
  /// <summary>
  ///   The format this document was written in. A reader that does not know the number refuses the file instead of guessing.
  /// </summary>
  public const int CURRENT_SCHEMA = 1;

  /// <summary>
  ///   Gets the schema version.
  /// </summary>
  [JsonPropertyName("schema")]
  public int Schema { get; init; } = CURRENT_SCHEMA;

  /// <summary>
  ///   Gets the native command the completer completes.
  /// </summary>
  [JsonPropertyName("command")]
  public string Command { get; init; } = string.Empty;

  /// <summary>
  ///   Gets the other names it answers to.
  /// </summary>
  [JsonPropertyName("aliases")]
  public IReadOnlyList<string> Aliases { get; init; } = [];

  /// <summary>
  ///   Gets the description shown for the command itself.
  /// </summary>
  [JsonPropertyName("description")]
  public string? Description { get; init; }

  /// <summary>
  ///   Gets what is valid right after the command name.
  /// </summary>
  [JsonPropertyName("root")]
  public CommandDocument Root { get; init; } = new();
}

/// <summary>
///   A command or subcommand, on disk.
/// </summary>
public sealed record CommandDocument {
  /// <summary>
  ///   Gets the subcommand name.
  /// </summary>
  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  /// <summary>
  ///   Gets the other names it answers to.
  /// </summary>
  [JsonPropertyName("aliases")]
  public IReadOnlyList<string> Aliases { get; init; } = [];

  /// <summary>
  ///   Gets the description shown beside the candidate.
  /// </summary>
  [JsonPropertyName("description")]
  public string? Description { get; init; }

  /// <summary>
  ///   Gets the subcommands.
  /// </summary>
  [JsonPropertyName("commands")]
  public IReadOnlyList<CommandDocument> Commands { get; init; } = [];

  /// <summary>
  ///   Gets the options.
  /// </summary>
  [JsonPropertyName("options")]
  public IReadOnlyList<OptionDocument> Options { get; init; } = [];

  /// <summary>
  ///   Gets the positional arguments.
  /// </summary>
  [JsonPropertyName("arguments")]
  public IReadOnlyList<ArgumentDocument> Arguments { get; init; } = [];
}

/// <summary>
///   An option, on disk.
/// </summary>
public sealed record OptionDocument {
  /// <summary>
  ///   Gets the option as typed.
  /// </summary>
  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  /// <summary>
  ///   Gets the other spellings.
  /// </summary>
  [JsonPropertyName("aliases")]
  public IReadOnlyList<string> Aliases { get; init; } = [];

  /// <summary>
  ///   Gets the description shown beside the candidate.
  /// </summary>
  [JsonPropertyName("description")]
  public string? Description { get; init; }

  /// <summary>
  ///   Gets the value the option takes, when it takes one.
  /// </summary>
  [JsonPropertyName("value")]
  public ArgumentDocument? Value { get; init; }
}

/// <summary>
///   A value slot, on disk. Only a provider name travels: an inline script block belongs to the session that declared it.
/// </summary>
public sealed record ArgumentDocument {
  /// <summary>
  ///   Gets the slot name.
  /// </summary>
  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  /// <summary>
  ///   Gets a value indicating whether the slot takes every remaining value.
  /// </summary>
  [JsonPropertyName("variadic")]
  public bool Variadic { get; init; }

  /// <summary>
  ///   Gets the provider completing the slot.
  /// </summary>
  [JsonPropertyName("provider")]
  public string? Provider { get; init; }
}
