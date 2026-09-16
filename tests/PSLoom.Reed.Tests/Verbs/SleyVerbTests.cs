// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using JetBrains.Annotations;
using PSLoom.Reed.Tests.Utility;
using PSLoom.Reed.Verbs;

namespace PSLoom.Reed.Tests.Verbs;

[TestSubject(typeof(SleyVerb))]
public sealed class SleyVerbTests {
  private const string DRAFT =
    """
    Thread Reed
    Sley git -Alias g -Description 'The stupid content tracker' {
      Option --version
      Command commit -Alias ci -Description 'Record changes' {
        Option --message -Alias '-m' { Argument text }
        Option --amend
      }
      Command status { Option --short }
    }
    """;

  [Fact]
  public void ADraftRegistersAndCompletes() {
    using var session = new CompletionSession();

    session.Run($"Invoke-Loom {{\n{DRAFT}\n}}");

    session.Streams.Error.ShouldBeEmpty();
    session.Complete("git co").ShouldBe(["commit"]);
    session.Complete("git commit --").ShouldBe(["--amend", "--message"]);
    session.Complete("git commit -m").ShouldBe(["-m"]);
    session.Complete("git status --s").ShouldBe(["--short"]);
  }

  [Fact]
  public void AnAliasCompletesLikeTheCommand() {
    using var session = new CompletionSession();
    session.Run($"Invoke-Loom {{\n{DRAFT}\n}}");

    session.Complete("g co").ShouldBe(["commit"]);
    session.Complete("g ci --a").ShouldBe(["--amend"]);
  }

  [Fact]
  public void AnInvalidDeclarationIsReported_AndTheDraftCarriesOn() {
    using var session = new CompletionSession();

    session.Run(
      """
      Invoke-Loom {
        Thread Reed
        Sley git { Command commit { }; Command ci -Alias commit { } }
        Style 'app:*' 'color' 'Cyan'
      }
      """);

    session.Streams.Error.ShouldHaveSingleItem().FullyQualifiedErrorId.ShouldStartWith(ReedException.COMPLETER_INVALID);
    session.Run("Get-Completer").ShouldBeEmpty();
    session.Run("(Get-Style 'app:main' 'color')").Single().BaseObject.ShouldBe("Cyan");
  }

  [Fact]
  public void ValidateRegistersNothing() {
    using var session = new CompletionSession();

    session.Run($"Invoke-Loom -Validate {{\n{DRAFT}\n}}");

    session.Streams.Error.ShouldBeEmpty();
    session.Run("Get-Completer").ShouldBeEmpty();
  }

  [Fact]
  public void ReweaveReplacesAChangedCompleter() {
    using var session = new CompletionSession();
    session.Run($"Invoke-Loom {{\n{DRAFT}\n}}");

    session.Run($"Invoke-Loom -Reweave {{\n{DRAFT.Replace("Command status { Option --short }", "Command stash { }")}\n}}");

    session.Streams.Error.ShouldBeEmpty();
    session.Complete("git st").ShouldBe(["stash"]);
  }

  [Fact]
  public void ReweaveUnregistersACompleterWhoseLineIsGone() {
    using var session = new CompletionSession();
    session.Run($"Invoke-Loom {{\n{DRAFT}\n}}");

    session.Run("Invoke-Loom -Reweave { Thread Reed }");

    session.Streams.Error.ShouldBeEmpty();
    session.Streams.Warning.ShouldBeEmpty();
    session.Run("Get-Completer").ShouldBeEmpty();
    session.Complete("git co").ShouldNotContain("commit");
  }

  [Fact]
  public void EveryTabPressIsTraced() {
    using var session = new CompletionSession();
    session.Run($"Invoke-Loom {{\n{DRAFT}\n}}");

    session.Complete("git co");
    session.Complete("git commit --");

    var traces = session.Run("Trace-Completion");
    traces.Count.ShouldBe(2);
    traces[0].Properties["WordToComplete"].Value.ShouldBe("co");
    traces[0].Properties["Candidates"].Value.ShouldBe(1);
    traces[1].Properties["Candidates"].Value.ShouldBe(2);
  }
}
