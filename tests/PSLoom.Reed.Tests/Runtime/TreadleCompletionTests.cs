// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using JetBrains.Annotations;
using PSLoom.Reed.Runtime;
using PSLoom.Reed.Tests.Utility;

namespace PSLoom.Reed.Tests.Runtime;

[TestSubject(typeof(TreadleCompletion))]
public sealed class TreadleCompletionTests {
  private const string COMPLETER =
    """
    Sley git {
      Command log { Option --graph; Option --decorate }
      Command commit { Option --amend }
    }
    """;

  private const string TREADLE = "Treadle glog { git log --oneline }";

  [Fact]
  public void ATreadleCompletesLikeTheCommandItRuns() {
    using var session = Woven($"{COMPLETER}\n{TREADLE}");

    session.Complete("glog --").ShouldBe(["--decorate", "--graph"]);
  }

  [Fact]
  public void TheOrderOfTheDeclarationsDoesNotMatter() {
    using var session = Woven($"{TREADLE}\n{COMPLETER}");

    session.Complete("glog --").ShouldBe(["--decorate", "--graph"]);
  }

  [Fact]
  public void TheBakedTokensAreContext_NotCandidates() {
    using var session = Woven($"{COMPLETER}\n{TREADLE}");

    // 'git log --oneline' is already on the line as far as the resolver is concerned, so 'commit' is not reachable any more and
    // the baked option is not offered again.
    session.Complete("glog ").ShouldNotContain("commit");
    session.Complete("glog --o").ShouldBeEmpty();
  }

  [Fact]
  public void ATreadleWhoseCommandHasNoCompleterIsLeftAlone() {
    using var session = Woven("Treadle hgs { hg status --short }");

    session.Complete("hgs --").ShouldNotContain("--short");
    session.Streams.Error.ShouldBeEmpty();
  }

  [Fact]
  public void RemovingTheTreadleStopsTheInheritance() {
    using var session = Woven($"{COMPLETER}\n{TREADLE}");
    session.Complete("glog --").ShouldNotBeEmpty();

    session.Run("Remove-Treadle glog");

    session.Complete("glog --").ShouldNotContain("--graph");
    session.Streams.Error.ShouldBeEmpty();
  }

  [Fact]
  public void UnregisteringTheCompleterStopsTheInheritance() {
    using var session = Woven($"{COMPLETER}\n{TREADLE}");

    session.Run("Unregister-Completer git");

    session.Complete("glog --").ShouldNotContain("--graph");
  }

  [Fact]
  public void ATreadleDeclaredOutsideADraftIsFollowedToo() {
    using var session = Woven(COMPLETER);

    session.Run("New-Treadle gst { git commit --amend }");

    session.Complete("gst --").ShouldBeEmpty();
    session.Complete("gst ").ShouldNotContain("commit");
  }

  private static CompletionSession Woven(string draft) {
    var session = new CompletionSession();
    session.Run($"Invoke-Loom {{\nThread Reed\n{draft}\n}}");
    session.Streams.Error.ShouldBeEmpty();

    return session;
  }
}
