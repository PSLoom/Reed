// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Text;

namespace PSLoom.Benchmarks.Completion;

/// <summary>
///   The "medium completer" the Tab budget is measured against: 30 subcommands, two levels deep, 10 options per command.
/// </summary>
public static class MediumCompleter {
  /// <summary>
  ///   Gets the command the fixture completes.
  /// </summary>
  public const string COMMAND = "medium";

  /// <summary>
  ///   Builds the declaration script that registers the fixture.
  /// </summary>
  public static string Script() {
    var builder = new StringBuilder($"New-Completer {COMMAND} -ScriptBlock {{").AppendLine();
    Options(builder, "root", 1);

    for (var first = 0; first < 30; first++) {
      builder.AppendLine($"  Command command{first:D2} -Alias c{first:D2} -Description 'Subcommand {first}' {{");
      Options(builder, $"c{first:D2}", 2);

      for (var second = 0; second < 30; second++) {
        builder.AppendLine($"    Command nested{second:D2} -Description 'Nested {second}' {{");
        Options(builder, $"n{second:D2}", 3);
        builder.AppendLine("    }");
      }

      builder.AppendLine("  }");
    }

    return builder.AppendLine("} | Register-Completer -Force").ToString();
  }

  private static void Options(StringBuilder builder, string prefix, int depth) {
    var indent = new string(' ', depth * 2);

    for (var index = 0; index < 10; index++) {
      builder.AppendLine($"{indent}Option --{prefix}-option{index:D2} -Description 'Option {index}'");
    }
  }
}
