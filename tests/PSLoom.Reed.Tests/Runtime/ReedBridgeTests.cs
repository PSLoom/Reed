// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Management.Automation.Language;
using JetBrains.Annotations;
using PSLoom.Reed.Runtime;
using PSLoom.Reed.Tests.Utility;
using PSLoom.TestKit;

namespace PSLoom.Reed.Tests.Runtime;

[TestSubject(typeof(ReedBridge))]
public sealed class ReedBridgeTests {
  [Fact]
  public void WithoutARunspaceItReturnsNothingInsteadOfThrowing() {
    using var _ = PowerShellHost.UseAsDefault(null);

    // No runspace means no session to even look a completer up in; a tab press still must not break the command line.
    Should.NotThrow(() => ReedBridge.Complete("co", Command("git co"), 6).ShouldBeEmpty());
  }

  [Fact]
  public void AnUnknownCommandCompletesNothing_AndIsStillTraced() {
    using var session = new CompletionSession();
    session.Run("Invoke-Loom { Thread Reed }");

    using (PowerShellHost.UseAsDefault(session.Runspace)) {
      ReedBridge.Complete("sta", Command("hg sta"), 6).ShouldBeEmpty();
    }

    var trace = session.Run("Trace-Completion").ShouldHaveSingleItem();
    trace.Properties["Command"].Value.ShouldBe("hg");
    trace.Properties["Candidates"].Value.ShouldBe(0);
  }

  [Fact]
  public void AMissingCommandAstCompletesNothing() {
    using var session = new CompletionSession();

    using var _ = PowerShellHost.UseAsDefault(session.Runspace);
    Should.NotThrow(() => ReedBridge.Complete("co", null, 0).ShouldBeEmpty());
  }

  private static CommandAst Command(string input)
    => (CommandAst)Parser.ParseInput(input, out var _, out var _).Find(node => node is CommandAst, true)!;
}
