// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using JetBrains.Annotations;
using PSLoom.Reed.Completion;
using PSLoom.Reed.Tests.Utility;

namespace PSLoom.Reed.Tests.Completion;

[TestSubject(typeof(CompletionSourceInvoker))]
[TestSubject(typeof(CompletionEngine))]
public sealed class CompletionSourceTests {
  [Fact]
  public void AnInlineSourceCompletesAnOptionValue() {
    using var session = Woven(
      """
      Sley git {
        Command commit {
          Option --message -Alias '-m' { Argument text -Source { 'fix: ', 'feat: ' } }
        }
      }
      """);

    session.Complete("git commit -m ").ShouldBe(["'fix: '", "'feat: '"]);
    session.Complete("git commit -m f").ShouldBe(["'fix: '", "'feat: '"]);
    session.Complete("git commit -m fe").ShouldBe(["'feat: '"]);
  }

  [Fact]
  public void APositionalSourceCompletesBesideSubcommands() {
    using var session = Woven(
      """
      Sley git {
        Command commit { }
        Argument target -Source { 'origin', 'upstream' }
      }
      """);

    session.Complete("git ").ShouldBe(["commit", "origin", "upstream"]);
    session.Complete("git o").ShouldBe(["origin"]);
  }

  [Fact]
  public void ASourceSeesTheWordThePathTheOptionsAndTheArguments() {
    using var session = Woven(
      """
      Sley git {
        Command commit {
          Option --amend
          Argument files -Variadic -Source {
            param($word, $path, $options, $arguments)
            "word=$word", "path=$($path -join '/')", "options=$($options -join ',')", "arguments=$($arguments -join ',')"
          }
        }
      }
      """);

    session.Complete("git commit --amend one two ").ShouldBe(["word=", "path=git/commit", "options=--amend", "arguments=one,two"]);
  }

  [Fact]
  public void ASourceMayReturnCompletionResults() {
    using var session = Woven(
      """
      Sley git {
        Command switch {
          Argument branch -Source {
            [System.Management.Automation.CompletionResult]::new('main', 'main', 'ParameterValue', 'the default branch')
          }
        }
      }
      """);

    var matches = session.Run("(TabExpansion2 -inputScript 'git switch ' -cursorColumn 11).CompletionMatches");

    matches.ShouldHaveSingleItem().Properties["ToolTip"].Value.ShouldBe("the default branch");
  }

  [Fact]
  public void AFailingSourceCompletesNothing_AndIsTraced() {
    using var session = Woven("Sley git { Argument target -Source { throw 'source exploded' } }");

    // Reed offers nothing, so PowerShell falls back to its own native completion; what matters is that the press survived.
    session.Complete("git ").ShouldNotContain("target");

    var trace = session.Run("Trace-Completion").ShouldHaveSingleItem();
    trace.Properties["Candidates"].Value.ShouldBe(0);
    trace.Properties["Exception"].Value.ShouldNotBeNull();
  }

  [Fact]
  public void AProviderCompletesOnceRegistered_AndIsSilentBefore() {
    using var session = Woven("Sley git { Argument remote -Provider 'git-remotes' }");

    // An unregistered provider is silent, not an error: an imported completer must survive a session that never registered it.
    session.Complete("git ").ShouldNotContain("origin");
    session.Streams.Error.ShouldBeEmpty();

    session.Run("Register-CompletionProvider git-remotes { 'origin', 'fork' }");

    session.Complete("git ").ShouldBe(["origin", "fork"]);
    session.Complete("git f").ShouldBe(["fork"]);
  }

  [Fact]
  public void ACachedSourceRunsOncePerKey() {
    using var session = Woven(
      """
      Sley git {
        Command switch {
          Argument branch -Source { $global:calls++; 'main' } -CacheKey { 'stable' }
        }
      }
      """);
    session.Run("$global:calls = 0");

    session.Complete("git switch ");
    session.Complete("git switch m");
    session.Complete("git switch ma");

    session.Run("$global:calls").Single().BaseObject.ShouldBe(1);
  }

  [Fact]
  public void AChangedKeyAsksTheSourceAgain() {
    using var session = Woven(
      """
      Sley git {
        Command switch {
          Argument branch -Source { $global:calls++; 'main' } -CacheKey { $global:key }
        }
      }
      """);
    session.Run("$global:calls = 0; $global:key = 'a'");

    session.Complete("git switch ");
    session.Run("$global:key = 'b'");
    session.Complete("git switch ");

    session.Run("$global:calls").Single().BaseObject.ShouldBe(2);
  }

  [Fact]
  public void AnEmptyKeyMeansNoCaching() {
    using var session = Woven(
      """
      Sley git {
        Command switch {
          Argument branch -Source { $global:calls++; 'main' } -CacheKey { '' }
        }
      }
      """);
    session.Run("$global:calls = 0");

    session.Complete("git switch ");
    session.Complete("git switch ");

    session.Run("$global:calls").Single().BaseObject.ShouldBe(2);
  }

  [Fact]
  public void ClearCompletionCacheMakesTheSourceRunAgain() {
    using var session = Woven(
      """
      Sley git {
        Command switch {
          Argument branch -Source { $global:calls++; 'main' } -CacheKey { 'stable' }
        }
      }
      """);
    session.Run("$global:calls = 0");
    session.Complete("git switch ");

    session.Run("Clear-CompletionCache -Verbose");
    session.Complete("git switch ");

    session.Run("$global:calls").Single().BaseObject.ShouldBe(2);
    session.Streams.Verbose.ShouldContain(record => record.Message.Contains("1 cached completion entry"));
  }

  [Fact]
  public void ARedeclaredCompleterDoesNotServeTheOldCache() {
    const string DRAFT =
      """
      Sley git {
        Command switch { Argument branch -Source { $global:answer } -CacheKey { 'stable' } }
      }
      """;
    using var session = Woven(DRAFT);
    session.Run("$global:answer = 'old'");
    session.Complete("git switch ").ShouldBe(["old"]);

    session.Run("$global:answer = 'new'");
    session.Run($"Invoke-Loom -Reweave {{ Thread Reed\n{DRAFT.Replace("Command switch", "Command checkout")}\n}}");

    session.Complete("git checkout ").ShouldBe(["new"]);
  }

  [Fact]
  public void ACacheKeyWithoutASourceIsADeclarationError() {
    using var session = new CompletionSession();

    session.Run("Invoke-Loom { Thread Reed; Sley git { Argument target -CacheKey { 'x' } } }");

    session.Streams.Error.ShouldHaveSingleItem().FullyQualifiedErrorId.ShouldStartWith(ReedException.INVALID_DECLARATION);
  }

  private static CompletionSession Woven(string declaration) {
    var session = new CompletionSession();
    session.Run($"Invoke-Loom {{\nThread Reed\n{declaration}\n}}");
    session.Streams.Error.ShouldBeEmpty();

    return session;
  }
}
