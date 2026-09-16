// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using JetBrains.Annotations;
using PSLoom.Reed.Cmdlets;
using PSLoom.Reed.Runtime;
using PSLoom.Reed.Tests.Utility;

namespace PSLoom.Reed.Tests.Cmdlets;

[TestSubject(typeof(NewCompleterCmdlet))]
[TestSubject(typeof(RegisterCompleterCmdlet))]
[TestSubject(typeof(GetCompleterCmdlet))]
[TestSubject(typeof(UnregisterCompleterCmdlet))]
[TestSubject(typeof(TestCompleterCmdlet))]
public sealed class CompleterCmdletTests {
  private const string GIT =
    """
    New-Completer git -Alias g -ScriptBlock {
      OptionGroup Common { Option --verbose -Alias '-v' }
      Option --config { Argument key }
      Command commit -Alias ci -Description 'Record changes' {
        Use OptionGroup Common
        Option --message -Alias '-m' { Argument text }
        Argument files -Variadic
      }
    }
    """;

  [Fact]
  public void ADeclarationBecomesACompleterTree() {
    using var session = new CompletionSession();

    session.Run($"$c = {GIT}");

    session.Streams.Error.ShouldBeEmpty();
    session.Run("$c.Command").Single().BaseObject.ShouldBe("git");
    session.Run("$c.Root.Commands.Name").Single().BaseObject.ShouldBe("commit");
    session.Run("($c.Root.Commands | Where-Object Name -eq commit).Options.Name")
      .Select(result => (string)result.BaseObject).ShouldBe(["--verbose", "--message"]);
    session.Run("($c.Root.Commands.Options | Where-Object Name -eq '--message').Value.Name").Single().BaseObject.ShouldBe("text");
    session.Run("$c.Root.Commands.Arguments.Variadic").Single().BaseObject.ShouldBe(true);
  }

  [Fact]
  public void AnOptionGroupIsCopied_NotShared() {
    using var session = new CompletionSession();

    session.Run($"$c = {GIT}");

    // The group's own option object must not be the one the command ends up with, or editing one would edit the other.
    session.Run("[object]::ReferenceEquals($c.Root.Commands.Options[0], $c.Root.Options[0])").Single().BaseObject.ShouldBe(false);
  }

  [Fact]
  public void AnUnknownOptionGroupIsAnError() {
    using var session = new CompletionSession();

    session.Run("New-Completer git -ScriptBlock { Command commit { Use OptionGroup Missing } }");

    session.Streams.Error.ShouldHaveSingleItem().FullyQualifiedErrorId.ShouldStartWith(ReedException.OPTION_GROUP_NOT_FOUND);
  }

  [Fact]
  public void AnErrorThreeScopesDeepIsReportedExactlyOnce() {
    using var session = new CompletionSession();

    // The same nesting as a draft, through the cmdlet path: IDslRunner collects the failure and New-Completer writes it once.
    session.Run("New-Completer git -ScriptBlock { Command remote { Command add { Use OptionGroup Missing } } }");

    session.Streams.Error.ShouldHaveSingleItem().FullyQualifiedErrorId.ShouldStartWith(ReedException.OPTION_GROUP_NOT_FOUND);
  }

  [Fact]
  public void UnregisteringLeavesTheNativeWiringInPlace() {
    using var session = new CompletionSession();
    session.Run($"{GIT} | Register-Completer");
    var reed = ReedSession.For(session.Runspace);

    session.Run("Unregister-Completer git");

    // PowerShell cannot remove a native completer from a live session, so Reed never tries: the names stay wired, and the bridge
    // simply finds no registration behind them.
    reed.Wired.ShouldBe(["git", "g"], true);
    session.Run($"{GIT} | Register-Completer");
    reed.Wired.Count.ShouldBe(2);
    session.Complete("git co").ShouldBe(["commit"]);
  }

  [Fact]
  public void AVerbOutsideItsScopeIsAnError() {
    using var session = new CompletionSession();

    session.Run("New-Completer git -ScriptBlock { OptionGroup Common { Argument stray } }");

    session.Streams.Error.ShouldHaveSingleItem().FullyQualifiedErrorId.ShouldStartWith("LOOM_VERB_OUT_OF_SCOPE");
  }

  [Fact]
  public void DuplicateNamesBlockRegistration() {
    using var session = new CompletionSession();

    session.Run("New-Completer git -ScriptBlock { Command commit { }; Command ci -Alias commit { } } | Register-Completer");

    var error = session.Streams.Error.ShouldHaveSingleItem();
    error.FullyQualifiedErrorId.ShouldStartWith(ReedException.COMPLETER_INVALID);
    error.Exception.Message.ShouldContain("'commit' is declared by both subcommands");
    session.Run("Get-Completer").ShouldBeEmpty();
  }

  [Fact]
  public void TestCompleterReportsProblemsWithoutRegistering() {
    using var session = new CompletionSession();

    var problems = session.Run(
      "New-Completer git -ScriptBlock { Command commit { Argument a -Variadic; Argument b } } | Test-Completer");

    problems.ShouldHaveSingleItem().Properties["Path"].Value.ShouldBe("git/commit/a");
    session.Run("New-Completer hub -ScriptBlock { Command pr { } } | Test-Completer -Quiet").Single().BaseObject.ShouldBe(true);
    session.Run("Get-Completer").ShouldBeEmpty();
  }

  [Fact]
  public void RegisteringListsAndWiresEveryName() {
    using var session = new CompletionSession();

    session.Run($"{GIT} | Register-Completer");

    session.Streams.Error.ShouldBeEmpty();
    session.Run("(Get-Completer).Names").Select(result => (string)result.BaseObject).ShouldBe(["git", "g"]);
    session.Run("(Get-Completer g).Command").Single().BaseObject.ShouldBe("git");
    session.Run("(Get-Completer 'g*').Command").Single().BaseObject.ShouldBe("git");
  }

  [Fact]
  public void ATakenNameCollidesUntilForced() {
    using var session = new CompletionSession();
    session.Run($"{GIT} | Register-Completer");

    session.Run("New-Completer git -ScriptBlock { Command log { } } | Register-Completer");

    session.Streams.Error.ShouldHaveSingleItem().FullyQualifiedErrorId.ShouldStartWith(ReedException.COMPLETER_NAME_TAKEN);
    session.Streams.Error.Clear();

    session.Run("New-Completer git -ScriptBlock { Command log { } } | Register-Completer -Force");
    session.Streams.Error.ShouldBeEmpty();
    session.Run("(Get-Completer git).Definition.Root.Commands.Name").Single().BaseObject.ShouldBe("log");
  }

  [Fact]
  public void ACollisionOnAnAliasNamesTheCommandThatHoldsIt() {
    using var session = new CompletionSession();
    session.Run("New-Completer hub -Alias g -ScriptBlock { Command pr { } } | Register-Completer");

    session.Run("New-Completer git -Alias g -ScriptBlock { Command commit { } } | Register-Completer");

    session.Streams.Error.ShouldHaveSingleItem().Exception.Message.ShouldContain("registered for 'hub'");
  }

  [Fact]
  public void ForcingOverAnAliasCollisionRemovesTheWholeRegistration() {
    using var session = new CompletionSession();
    session.Run("New-Completer hub -Alias g -ScriptBlock { Command pr { } } | Register-Completer");

    session.Run("New-Completer git -Alias g -ScriptBlock { Command commit { } } | Register-Completer -Force");

    session.Streams.Error.ShouldBeEmpty();
    session.Run("(Get-Completer).Command").Select(result => (string)result.BaseObject).ShouldBe(["git"]);
    session.Complete("hub ").ShouldNotContain("pr");
    session.Complete("g ").ShouldContain("commit");
  }

  [Fact]
  public void UnregisteringMakesCompletionSilent_AndWarnsWhenUnknown() {
    using var session = new CompletionSession();
    session.Run($"{GIT} | Register-Completer");

    session.Run("Unregister-Completer git");

    session.Run("Get-Completer").ShouldBeEmpty();

    // Reed offers nothing, so PowerShell falls back to its own native completion; what must be gone are Reed's candidates.
    session.Complete("git ").ShouldNotContain("commit");
    session.Complete("g ").ShouldNotContain("commit");

    session.Run("Unregister-Completer git");
    session.Streams.Warning.ShouldHaveSingleItem().Message.ShouldContain("git");
    session.Streams.Error.ShouldBeEmpty();
  }
}
