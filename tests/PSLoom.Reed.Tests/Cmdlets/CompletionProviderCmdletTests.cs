// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using JetBrains.Annotations;
using PSLoom.Reed.Cmdlets;
using PSLoom.Reed.Tests.Utility;

namespace PSLoom.Reed.Tests.Cmdlets;

[TestSubject(typeof(RegisterCompletionProviderCmdlet))]
[TestSubject(typeof(GetCompletionProviderCmdlet))]
[TestSubject(typeof(UnregisterCompletionProviderCmdlet))]
public sealed class CompletionProviderCmdletTests {
  [Fact]
  public void ProvidersAreListedByNameAndPattern() {
    using var session = new CompletionSession();

    session.Run("Register-CompletionProvider git-remotes { 'origin' } -Description 'the remotes'");
    session.Run("Register-CompletionProvider git-branches { 'main' }");
    session.Run("Register-CompletionProvider docker-images { 'alpine' }");

    Names(session, "Get-CompletionProvider").ShouldBe(["docker-images", "git-branches", "git-remotes"]);
    Names(session, "Get-CompletionProvider git-*").ShouldBe(["git-branches", "git-remotes"]);
    session.Run("(Get-CompletionProvider git-remotes).Description").Single().BaseObject.ShouldBe("the remotes");
  }

  [Fact]
  public void ATakenNameCollidesUntilForced() {
    using var session = new CompletionSession();
    session.Run("Register-CompletionProvider git-remotes { 'origin' }");

    session.Run("Register-CompletionProvider git-remotes { 'fork' }");

    session.Streams.Error.ShouldHaveSingleItem().FullyQualifiedErrorId.ShouldStartWith(ReedException.PROVIDER_NAME_TAKEN);
    session.Streams.Error.Clear();

    session.Run("Register-CompletionProvider git-remotes { 'fork' } -Force -PassThru").ShouldHaveSingleItem();
    session.Streams.Error.ShouldBeEmpty();
  }

  [Fact]
  public void UnregisteringWarnsWhenTheNameIsUnknown() {
    using var session = new CompletionSession();
    session.Run("Register-CompletionProvider git-remotes { 'origin' }");

    session.Run("Unregister-CompletionProvider git-remotes");
    session.Run("Get-CompletionProvider").ShouldBeEmpty();

    session.Run("Unregister-CompletionProvider git-remotes");
    session.Streams.Warning.ShouldHaveSingleItem().Message.ShouldContain("git-remotes");
    session.Streams.Error.ShouldBeEmpty();
  }

  [Fact]
  public void ARemovedProviderLeavesTheCompleterWorking() {
    using var session = new CompletionSession();
    session.Run("Register-CompletionProvider git-remotes { 'origin' }");
    session.Run("Invoke-Loom { Thread Reed; Sley git { Command push { }; Argument remote -Provider git-remotes } }");
    session.Complete("git o").ShouldBe(["origin"]);

    session.Run("Unregister-CompletionProvider git-remotes");

    session.Complete("git p").ShouldBe(["push"]);
    session.Streams.Error.ShouldBeEmpty();
  }

  private static IEnumerable<string?> Names(CompletionSession session, string script)
    => session.Run(script).Select(result => (string)result.Properties["Name"].Value);
}
