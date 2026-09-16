// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Management.Automation.Language;
using JetBrains.Annotations;
using PSLoom.Reed.Completion;
using PSLoom.Reed.Model;

namespace PSLoom.Reed.Tests.Completion;

[TestSubject(typeof(TokenClassifier))]
[TestSubject(typeof(ContextResolver))]
[TestSubject(typeof(CompletionEngine))]
public sealed class CompletionPipelineTests {
  private static readonly CompiledCompleter _git = Compile();

  [Theory]
  [InlineData("git ", new string[0])]
  [InlineData("git co", new string[0])]
  [InlineData("git commit ", new[] { "commit" })]
  [InlineData("git commit --me", new[] { "commit" })]
  [InlineData("git commit -m 'a message' --a", new[] { "commit", "-m", "a message" })]
  [InlineData("git --config=core.editor sta", new[] { "--config=core.editor" })]
  public void TypedTokensExcludeTheWordBeingCompleted(string input, string[] expected)
    => TokenClassifier.Preceding(Command(input), input.Length).ShouldBe(expected);

  [Fact]
  public void TheWordUnderTheCursorIsExcludedEvenMidLine() {
    const string INPUT = "git com --verbose";

    TokenClassifier.Preceding(Command(INPUT), 7).ShouldBeEmpty();
  }

  [Fact]
  public void ASubcommandBecomesTheActiveCommand() {
    var context = ContextResolver.Resolve(_git, ["commit"]);

    context.Command.Name.ShouldBe("commit");
    context.ExpectingValueFor.ShouldBeNull();
  }

  [Fact]
  public void AnAliasResolvesLikeTheNameItStandsFor() => ContextResolver.Resolve(_git, ["ci"]).Command.Name.ShouldBe("commit");

  [Fact]
  public void AnOptionTakingAValueMakesTheNextWordThatValue() {
    var context = ContextResolver.Resolve(_git, ["commit", "-m"]);

    context.ExpectingValueFor.ShouldNotBeNull().Name.ShouldBe("--message");
  }

  [Fact]
  public void AnInlineValueDoesNotLeaveTheOptionExpectingOne() {
    var context = ContextResolver.Resolve(_git, ["commit", "--message=hello"]);

    context.ExpectingValueFor.ShouldBeNull();
    context.BoundOptions.ShouldContain("--message");
  }

  [Fact]
  public void AValueIsNotMistakenForASubcommand() {
    var context = ContextResolver.Resolve(_git, ["commit", "-m", "status"]);

    context.Command.Name.ShouldBe("commit");
    context.Positional.ShouldBe(0);
  }

  [Fact]
  public void PositionalsAreCounted() => ContextResolver.Resolve(_git, ["commit", "one", "two"]).Positional.ShouldBe(2);

  [Fact]
  public void SubcommandsCompleteByPrefix() => Complete("", "co").ShouldBe(["commit"]);

  [Fact]
  public void AnEmptyWordOffersSubcommandsAndOptions() => Complete("", "").ShouldBe(["commit", "status", "--config", "--verbose"]);

  [Fact]
  public void ADashOffersOnlyOptions() => Complete("commit", "--").ShouldBe(["--amend", "--message"]);

  [Fact]
  public void AnEmptyWordListsPrimaryNamesOnly() {
    Complete("", "").ShouldNotContain("ci");
    Complete("", "c").ShouldContain("ci");
  }

  [Fact]
  public void AliasesAreOfferedToo() => Complete("commit", "-m").ShouldBe(["-m"]);

  [Fact]
  public void AnOptionAlreadyOnTheLineIsNotOfferedAgain() {
    var context = ContextResolver.Resolve(_git, ["commit", "--amend"]);

    CompletionEngine.Complete(context, "--").Select(result => result.CompletionText).ShouldBe(["--message"]);
  }

  [Fact]
  public void AnOptionValueOffersNothingUntilSourcesExist()
    => CompletionEngine.Complete(ContextResolver.Resolve(_git, ["commit", "-m"]), "").ShouldBeEmpty();

  [Fact]
  public void TooltipsCarryTheDescription() => CompletionEngine.Complete(ContextResolver.Resolve(_git, []), "commit").ShouldHaveSingleItem().ToolTip
    .ShouldBe("Record changes");

  private static IReadOnlyList<string> Complete(string typed, string word) {
    var tokens = typed.Length == 0 ? [] : typed.Split(' ');

    return [.. CompletionEngine.Complete(ContextResolver.Resolve(_git, tokens), word).Select(result => result.CompletionText)];
  }

  private static CommandAst Command(string input)
    => (CommandAst)Parser.ParseInput(input, out var _, out var _).Find(node => node is CommandAst, true)!;

  private static CompiledCompleter Compile() {
    var definition = new CompleterDefinition("git", [], null);
    var commit = new CommandNode("commit", ["ci"], "Record changes");
    var message = new OptionNode("--message", ["-m"], "The message") { Value = new ArgumentNode("text", false) };

    commit.Options.Add(message);
    commit.Options.Add(new OptionNode("--amend", [], null));
    definition.Root.Commands.Add(commit);
    definition.Root.Commands.Add(new CommandNode("status", [], null));
    definition.Root.Options.Add(new OptionNode("--verbose", ["-v"], null));
    definition.Root.Options.Add(new OptionNode("--config", [], null) { Value = new ArgumentNode("key", false) });

    return CompiledCompleter.Compile(definition);
  }
}
