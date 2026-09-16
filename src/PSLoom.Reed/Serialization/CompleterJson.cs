// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Text.Json;
using System.Text.Json.Serialization;
using PSLoom.Reed.Model;

namespace PSLoom.Reed.Serialization;

/// <summary>
///   The serializer context: source generation, so writing a completer needs no reflection at runtime.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(CompleterDocument))]
internal sealed partial class CompleterJsonContext : JsonSerializerContext;

/// <summary>
///   Converts a completer to and from its portable form.
/// </summary>
internal static class CompleterJson {
  /// <summary>
  ///   Writes a completer as JSON, leaving out the sources that cannot travel.
  /// </summary>
  public static string Write(CompleterDefinition definition) {
    ArgumentNullException.ThrowIfNull(definition);

    var document = new CompleterDocument {
      Command = definition.Command,
      Aliases = [.. definition.Aliases],
      Description = definition.Description,
      Root = Write(definition.Root)
    };

    return JsonSerializer.Serialize(document, CompleterJsonContext.Default.CompleterDocument);
  }

  /// <summary>
  ///   Reads a completer from JSON.
  /// </summary>
  /// <exception cref="ReedException">The document is malformed or of an unknown schema (<c>REED_COMPLETER_NOT_READABLE</c>).</exception>
  public static CompleterDefinition Read(string json, string origin) {
    ArgumentNullException.ThrowIfNull(json);

    CompleterDocument? document;

    try {
      document = JsonSerializer.Deserialize(json, CompleterJsonContext.Default.CompleterDocument);
    }
    catch (JsonException exception) {
      throw ReedException.CompleterNotReadable(origin, exception.Message, exception);
    }

    if (document is null) {
      throw ReedException.CompleterNotReadable(origin, "it holds no completer.");
    }

    if (document.Schema != CompleterDocument.CURRENT_SCHEMA) {
      throw ReedException.CompleterNotReadable(origin,
        $"schema {document.Schema}, and this version reads schema {CompleterDocument.CURRENT_SCHEMA}.");
    }

    if (string.IsNullOrWhiteSpace(document.Command)) {
      throw ReedException.CompleterNotReadable(origin, "it names no command.");
    }

    var definition = new CompleterDefinition(document.Command, [.. document.Aliases], document.Description);
    Read(document.Root, definition.Root);

    return definition;
  }

  private static CommandDocument Write(CommandNode node)
    => new() {
      Name = node.Name,
      Aliases = [.. node.Aliases],
      Description = node.Description,
      Commands = [.. node.Commands.Select(Write)],
      Options = [
        .. node.Options.Select(option => new OptionDocument {
          Name = option.Name,
          Aliases = [.. option.Aliases],
          Description = option.Description,
          Value = option.Value is { } value ? Write(value) : null
        })
      ],
      Arguments = [.. node.Arguments.Select(Write)]
    };

  private static ArgumentDocument Write(ArgumentNode argument)
    => new() {
      Name = argument.Name,
      Variadic = argument.Variadic,
      Provider = argument.Source is { Kind: CompletionSourceKind.Provider } source ? source.ProviderName : null
    };

  private static void Read(CommandDocument document, CommandNode node) {
    foreach (var argument in document.Arguments) {
      node.Arguments.Add(Read(argument));
    }

    foreach (var option in document.Options) {
      node.Options.Add(new OptionNode(option.Name, [.. option.Aliases], option.Description) {
        Value = option.Value is { } value ? Read(value) : null
      });
    }

    foreach (var command in document.Commands) {
      var child = new CommandNode(command.Name, [.. command.Aliases], command.Description);
      node.Commands.Add(child);
      Read(command, child);
    }
  }

  private static ArgumentNode Read(ArgumentDocument document)
    => new(document.Name, document.Variadic) {
      Source = string.IsNullOrWhiteSpace(document.Provider) ? null : new CompletionSource(document.Provider, null)
    };
}
