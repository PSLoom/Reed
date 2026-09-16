// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.Management.Automation.Language;
using System.Text;
using JetBrains.Annotations;
using PSLoom.Reed.Runtime;
using PSLoom.Reed.Tests.Utility;
using PSLoom.TestKit;

namespace PSLoom.Reed.Tests.Completion;

/// <summary>
///   The spec's Tab budget, on the medium completer it defines: 30 subcommands, two levels deep, 10 options per command. The
///   budget is 5 ms; the margin here is wide on purpose, so this fails on a regression in kind, not on a slow machine.
/// </summary>
[TestSubject(typeof(ReedBridge))]
public sealed class CompletionBudgetTests {
  private const int RUNS = 21;

  [Fact]
  public void ATabPressOnAMediumCompleterStaysUnderFiveMilliseconds() {
    using var session = new CompletionSession();
    session.Run("Invoke-Loom { Thread Reed }");
    session.Run(MediumCompleter());
    session.Streams.Error.ShouldBeEmpty();

    using var scope = PowerShellHost.UseAsDefault(session.Runspace);
    const string INPUT = "medium command15 nested20 --n20-option0";
    var ast = (CommandAst)Parser.ParseInput(INPUT, out var _, out var _).Find(node => node is CommandAst, true)!;
    var timings = new List<double>(RUNS);

    // Warm the path once: the first call pays JIT, which the budget explicitly does not cover.
    ReedBridge.Complete("--n20-option0", ast, INPUT.Length).Count().ShouldBe(10);

    for (var run = 0; run < RUNS; run++) {
      var started = Stopwatch.GetTimestamp();
      ReedBridge.Complete("--n20-option0", ast, INPUT.Length).Count();
      timings.Add(Stopwatch.GetElapsedTime(started).TotalMilliseconds);
    }

    timings.Sort();
    timings[RUNS / 2].ShouldBeLessThan(5);
  }

  private static string MediumCompleter() {
    var builder = new StringBuilder("New-Completer medium -ScriptBlock {");
    Options(builder, "root");

    for (var first = 0; first < 30; first++) {
      builder.AppendLine($"Command command{first:D2} -Alias c{first:D2} -Description 'Subcommand {first}' {{");
      Options(builder, $"c{first:D2}");

      for (var second = 0; second < 30; second++) {
        builder.AppendLine($"Command nested{second:D2} -Description 'Nested {second}' {{");
        Options(builder, $"n{second:D2}");
        builder.AppendLine("}");
      }

      builder.AppendLine("}");
    }

    return builder.AppendLine("} | Register-Completer").ToString();
  }

  private static void Options(StringBuilder builder, string prefix) {
    for (var index = 0; index < 10; index++) {
      builder.AppendLine($"Option --{prefix}-option{index:D2} -Description 'Option {index}'");
    }
  }
}
